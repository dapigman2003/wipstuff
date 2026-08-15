using System.Diagnostics;
using System.Net.Http;
using SteamKit2;
using SteamKit2.Authentication;

namespace StS2Launcher.Core;

/// <summary>
/// Step 06.3.1 persistent credential-authentication boundary.
///
/// Scope:
/// - retain the Step 06 / 06.1 modern credential + mobile Guard flow;
/// - request a persistent Steam auth session;
/// - after SteamUser.LogOn succeeds, save only account identity metadata and
///   the returned refresh token through <see cref="SteamSessionStore"/>;
/// - never persist the password, access token, or Steam Guard secret/code.
/// </summary>
public sealed class SteamAuthenticationAttempt
{
    public const string DeviceFriendlyName = "StS2 Launcher iOS";

    private readonly SteamSessionStore _sessionStore;

    public SteamAuthenticationAttempt(SteamSessionStore sessionStore)
    {
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
    }

    public async Task<SteamAuthenticationResult> RunAsync(
        string username,
        string password,
        TimeSpan timeout,
        CancellationToken cancellationToken = default,
        IProgress<SteamAuthenticationProgress>? progress = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Steam username is required.", nameof(username));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Steam password is required.", nameof(password));
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        username = username.Trim();
        var sw = Stopwatch.StartNew();
        using var operationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        operationCts.CancelAfter(timeout);
        using var pumpCts = CancellationTokenSource.CreateLinkedTokenSource(operationCts.Token);
        var token = operationCts.Token;

        SteamClient? steamClient = null;
        SteamUser? steamUser = null;
        Task? callbackPump = null;
        SteamGuardChallengeAuthenticator? authenticator = null;
        var cmConnected = false;
        var authSessionStarted = false;
        var mobileApprovalCompleted = false;
        var loggedOnCallbackReceived = false;
        var sessionPersisted = false;
        EResult? logonResult = null;
        EResult? extendedLogonResult = null;
        string? accountName = null;
        string? steamId64 = null;
        SteamGuardChallenge? guardChallenge = null;
        string? currentEndPoint = null;
        string? error = null;
        var outcome = SteamAuthenticationOutcome.Failed;
        uint? loginId = null;
        DateTimeOffset? refreshTokenExpiresAtUtc = null;
        bool? refreshTokenExpiredAtAttempt = null;

        Report(SteamAuthenticationStage.Starting, "Starting persistent Steam authentication session.");

        try
        {
            HttpClient Factory(HttpClientPurpose purpose) =>
                SteamHttpClientFactory.Create(purpose);

            var configuration = SteamConfiguration.Create(builder => builder
                .WithProtocolTypes(ProtocolTypes.WebSocket)
                .WithHttpClientFactory(Factory));

            steamClient = new SteamClient(configuration);
            steamUser = steamClient.GetHandler<SteamUser>()
                ?? throw new InvalidOperationException("SteamUser handler is unavailable.");

            var manager = new CallbackManager(steamClient);
            var connectedTcs = NewTcs<SteamClient.ConnectedCallback>();
            var disconnectedTcs = NewTcs<SteamClient.DisconnectedCallback>();
            var loggedOnTcs = NewTcs<SteamUser.LoggedOnCallback>();

            manager.Subscribe<SteamClient.ConnectedCallback>(callback =>
            {
                cmConnected = true;
                connectedTcs.TrySetResult(callback);
            });
            manager.Subscribe<SteamClient.DisconnectedCallback>(callback =>
                disconnectedTcs.TrySetResult(callback));
            manager.Subscribe<SteamUser.LoggedOnCallback>(callback =>
            {
                loggedOnCallbackReceived = true;
                logonResult = callback.Result;
                extendedLogonResult = callback.ExtendedResult;
                loggedOnTcs.TrySetResult(callback);
            });

            callbackPump = Task.Run(() => PumpCallbacks(manager, pumpCts.Token));
            Report(SteamAuthenticationStage.Connecting, "Connecting to the Steam CM over WebSocket.");
            steamClient.Connect();

            var connectWinner = await Task.WhenAny(
                    connectedTcs.Task,
                    disconnectedTcs.Task)
                .WaitAsync(token)
                .ConfigureAwait(false);

            currentEndPoint = steamClient.CurrentEndPoint?.ToString();
            if (connectWinner == disconnectedTcs.Task || !cmConnected)
            {
                error = "Steam disconnected before ConnectedCallback.";
                Report(SteamAuthenticationStage.Failed, error);
                return BuildResult();
            }

            Report(SteamAuthenticationStage.CmConnected, "Steam CM connected. Beginning persistent credential authentication.");

            authenticator = new SteamGuardChallengeAuthenticator(progress);
            CredentialsAuthSession authSession;

            try
            {
                authSession = await steamClient.Authentication
                    .BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails
                    {
                        Username = username,
                        Password = password,
                        DeviceFriendlyName = DeviceFriendlyName,
                        ClientOSType = EOSType.IOSUnknown,
                        IsPersistentSession = true,
                        GuardData = null,
                        Authenticator = authenticator,
                    })
                    .WaitAsync(token)
                    .ConfigureAwait(false);
                authSessionStarted = true;
                Report(SteamAuthenticationStage.AuthSessionStarted, "Persistent Steam credential authentication session started.");
            }
            catch (AuthenticationException ex)
            {
                error = $"AuthenticationException ({ex.Result}): {ex.Message}";
                Report(SteamAuthenticationStage.Failed, error);
                return BuildResult();
            }

            AuthPollResult pollResult;
            try
            {
                pollResult = await authSession
                    .PollingWaitForResultAsync(token)
                    .ConfigureAwait(false);
            }
            catch (SteamGuardChallengeRequiredException ex)
            {
                guardChallenge = ex.Challenge;
                outcome = SteamAuthenticationOutcome.GuardRequired;
                Report(SteamAuthenticationStage.Failed, ex.Challenge.Summary);
                return BuildResult();
            }
            catch (AuthenticationException ex)
            {
                error = $"AuthenticationException ({ex.Result}): {ex.Message}";
                Report(SteamAuthenticationStage.Failed, error);
                return BuildResult();
            }

            if (authenticator.MobileApprovalRequested)
            {
                mobileApprovalCompleted = true;
                Report(
                    SteamAuthenticationStage.MobileApprovalAccepted,
                    "Steam Guard mobile approval accepted. Completing Steam logon.");
            }

            accountName = pollResult.AccountName;

            // Decode only non-secret timing metadata from the JWT. The raw
            // refresh token is never displayed or logged.
            if (SteamRefreshTokenMetadata.TryParse(pollResult.RefreshToken, out var tokenMetadata) &&
                tokenMetadata is not null)
            {
                refreshTokenExpiresAtUtc = tokenMetadata.ExpiresAtUtc;
                refreshTokenExpiredAtAttempt = tokenMetadata.IsExpiredAt(DateTimeOffset.UtcNow);
            }

            // IsPersistentSession=true above requires ShouldRememberPassword=true
            // on the token-based SteamUser.LogOn call. Keep this policy in one
            // unit-tested factory used by both fresh auth and saved-session resume.
            var logOnDetails = SteamPersistentLogOnDetails.Create(
                pollResult.AccountName,
                pollResult.RefreshToken);
            loginId = logOnDetails.LoginID;
            Report(SteamAuthenticationStage.LoggingOn, "Authentication approved. Logging on with the persistent refresh token.");
            steamUser.LogOn(logOnDetails);

            var logonWinner = await Task.WhenAny(
                    loggedOnTcs.Task,
                    disconnectedTcs.Task)
                .WaitAsync(token)
                .ConfigureAwait(false);

            currentEndPoint = steamClient.CurrentEndPoint?.ToString() ?? currentEndPoint;
            if (logonWinner == disconnectedTcs.Task || !loggedOnCallbackReceived)
            {
                error = "Steam disconnected before LoggedOnCallback.";
                Report(SteamAuthenticationStage.Failed, error);
                return BuildResult();
            }

            if (logonResult != EResult.OK)
            {
                error = $"Steam logon rejected: {logonResult} / {extendedLogonResult}.";
                Report(SteamAuthenticationStage.Failed, error);
                return BuildResult();
            }

            steamId64 = steamUser.SteamID?.ConvertToUInt64().ToString();
            if (string.IsNullOrWhiteSpace(steamId64))
            {
                error = "Steam logon succeeded but SteamID64 was not available; session was not persisted.";
                Report(SteamAuthenticationStage.Failed, error);
                return BuildResult();
            }

            try
            {
                _sessionStore.Save(new SteamSavedSession(
                    pollResult.AccountName,
                    steamId64,
                    pollResult.RefreshToken));
                sessionPersisted = true;
            }
            catch (Exception ex)
            {
                error = $"Steam authenticated, but Keychain session persistence failed: {ex.GetType().Name}: {ex.Message}";
                Report(SteamAuthenticationStage.Failed, error);
                return BuildResult();
            }

            outcome = SteamAuthenticationOutcome.Authenticated;
            Report(SteamAuthenticationStage.Authenticated, "Steam authentication completed and the reusable session was saved to Keychain.");
            return BuildResult();
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outcome = SteamAuthenticationOutcome.Cancelled;
                error = "Authentication cancelled by user.";
                Report(SteamAuthenticationStage.Cancelled, error);
            }
            else
            {
                outcome = SteamAuthenticationOutcome.TimedOut;
                error = $"Authentication timed out after {timeout.TotalSeconds:F0}s.";
                Report(SteamAuthenticationStage.TimedOut, error);
            }

            return BuildResult();
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            Report(SteamAuthenticationStage.Failed, error);
            return BuildResult();
        }
        finally
        {
            // Do not explicitly SteamUser.LogOff after proving a persistent
            // token. Close the transport only.
            if (steamClient is not null)
            {
                try
                {
                    if (steamClient.IsConnected)
                        steamClient.Disconnect();
                }
                catch { }
            }

            pumpCts.Cancel();
            if (callbackPump is not null)
            {
                try
                {
                    await callbackPump.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
            }

            sw.Stop();
        }

        void Report(SteamAuthenticationStage stage, string message) =>
            progress?.Report(new SteamAuthenticationProgress(stage, message));

        SteamAuthenticationResult BuildResult() => new(
            Outcome: outcome,
            CmConnected: cmConnected,
            AuthSessionStarted: authSessionStarted,
            MobileApprovalRequested: authenticator?.MobileApprovalRequested == true,
            MobileApprovalCompleted: mobileApprovalCompleted,
            LoggedOnCallbackReceived: loggedOnCallbackReceived,
            LogonResult: logonResult,
            ExtendedLogonResult: extendedLogonResult,
            SessionPersisted: sessionPersisted,
            AccountName: accountName,
            SteamId64: steamId64,
            GuardChallenge: guardChallenge ?? authenticator?.LastChallenge,
            CurrentEndPoint: currentEndPoint,
            Elapsed: sw.Elapsed,
            Error: error,
            LoginId: loginId,
            RefreshTokenExpiresAtUtc: refreshTokenExpiresAtUtc,
            RefreshTokenExpiredAtAttempt: refreshTokenExpiredAtAttempt);
    }

    private static TaskCompletionSource<T> NewTcs<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static void PumpCallbacks(
        CallbackManager manager,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
            manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(250));
    }
}
