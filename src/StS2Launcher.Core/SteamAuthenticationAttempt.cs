using System.Diagnostics;
using System.Net.Http;
using SteamKit2;
using SteamKit2.Authentication;

namespace StS2Launcher.Core;

/// <summary>
/// Step 06.1 authentication boundary.
///
/// Scope:
/// - connect using the Step 05-proven CM WebSocket path;
/// - begin modern Steam credential authentication;
/// - if Steam requests mobile-app confirmation, keep the same auth session
///   alive and poll until the user approves it in the Steam mobile app;
/// - use the transient refresh token to perform SteamUser.LogOn;
/// - report the authenticated account name and SteamID;
/// - continue to stop at device-code/email-code challenges.
///
/// This class never persists the password, refresh token, access token, guard
/// data, or any Steam credential. Step 06.2 owns persistence later.
/// </summary>
public sealed class SteamAuthenticationAttempt
{
    public const string DeviceFriendlyName = "StS2 Launcher iOS";

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
        EResult? logonResult = null;
        EResult? extendedLogonResult = null;
        string? accountName = null;
        string? steamId64 = null;
        SteamGuardChallenge? guardChallenge = null;
        string? currentEndPoint = null;
        string? error = null;
        var outcome = SteamAuthenticationOutcome.Failed;

        Report(SteamAuthenticationStage.Starting, "Starting Steam authentication session.");

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

            Report(SteamAuthenticationStage.CmConnected, "Steam CM connected. Beginning credential authentication.");

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
                        IsPersistentSession = false,
                        GuardData = null,
                        Authenticator = authenticator,
                    })
                    .WaitAsync(token)
                    .ConfigureAwait(false);
                authSessionStarted = true;
                Report(SteamAuthenticationStage.AuthSessionStarted, "Steam credential authentication session started.");
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
                // If Steam prefers DeviceConfirmation, our authenticator returns
                // true from AcceptDeviceConfirmationAsync. SteamKit then polls
                // this same session until the user approves it in the Steam app.
                pollResult = await authSession
                    .PollingWaitForResultAsync(token)
                    .ConfigureAwait(false);
            }
            catch (SteamGuardChallengeRequiredException ex)
            {
                // Step 06.1 handles mobile approval only. Authenticator-code and
                // email-code entry remain explicit later substeps.
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

            // The refresh token is used only for this in-memory logon. It is
            // never returned in SteamAuthenticationResult, logged, or stored.
            Report(SteamAuthenticationStage.LoggingOn, "Authentication approved. Logging on to Steam with the transient session token.");
            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = pollResult.AccountName,
                AccessToken = pollResult.RefreshToken,
                ShouldRememberPassword = false,
                ClientOSType = EOSType.IOSUnknown,
                MachineName = DeviceFriendlyName,
            });

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
            outcome = SteamAuthenticationOutcome.Authenticated;
            Report(SteamAuthenticationStage.Authenticated, "Steam authentication completed successfully.");
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
            if (steamUser is not null && outcome == SteamAuthenticationOutcome.Authenticated)
            {
                try
                {
                    steamUser.LogOff();
                }
                catch
                {
                    // Best-effort cleanup only. Step 06.1 proves auth/approval;
                    // it does not keep a persistent Steam session alive yet.
                }
            }

            if (steamClient is not null)
            {
                try
                {
                    if (steamClient.IsConnected)
                        steamClient.Disconnect();
                }
                catch
                {
                    // Best-effort cleanup.
                }
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
                    // Expected when the callback pump is stopped.
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
            AccountName: accountName,
            SteamId64: steamId64,
            GuardChallenge: guardChallenge ?? authenticator?.LastChallenge,
            CurrentEndPoint: currentEndPoint,
            Elapsed: sw.Elapsed,
            Error: error);
    }

    private static TaskCompletionSource<T> NewTcs<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static void PumpCallbacks(
        CallbackManager manager,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(250));
        }
    }
}
