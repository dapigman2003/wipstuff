using System.Diagnostics;
using System.Net.Http;
using SteamKit2;
using SteamKit2.Authentication;

namespace StS2Launcher.Core;

/// <summary>
/// Step 06 credential authentication boundary.
///
/// Scope:
/// - connect using the Step 05-proven CM WebSocket path;
/// - begin modern Steam credential authentication;
/// - if no Steam Guard challenge is required, obtain the transient refresh
///   token and use it to perform SteamUser.LogOn;
/// - report the authenticated account name and SteamID;
/// - if Steam Guard is required, stop before handling the challenge.
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
        CancellationToken cancellationToken = default)
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
        var cmConnected = false;
        var authSessionStarted = false;
        var loggedOnCallbackReceived = false;
        EResult? logonResult = null;
        EResult? extendedLogonResult = null;
        string? accountName = null;
        string? steamId64 = null;
        SteamGuardChallenge? guardChallenge = null;
        string? currentEndPoint = null;
        string? error = null;
        var outcome = SteamAuthenticationOutcome.Failed;

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
                return BuildResult();
            }

            var authenticator = new SteamGuardChallengeAuthenticator();
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
            }
            catch (AuthenticationException ex)
            {
                error = $"AuthenticationException ({ex.Result}): {ex.Message}";
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
                return BuildResult();
            }
            catch (AuthenticationException ex)
            {
                error = $"AuthenticationException ({ex.Result}): {ex.Message}";
                return BuildResult();
            }

            accountName = pollResult.AccountName;

            // The refresh token is used only for this in-memory logon. It is
            // never returned in SteamAuthenticationResult, logged, or stored.
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
                return BuildResult();
            }

            if (logonResult != EResult.OK)
            {
                error = $"Steam logon rejected: {logonResult} / {extendedLogonResult}.";
                return BuildResult();
            }

            steamId64 = steamUser.SteamID?.ConvertToUInt64().ToString();
            outcome = SteamAuthenticationOutcome.Authenticated;
            return BuildResult();
        }
        catch (OperationCanceledException)
        {
            outcome = SteamAuthenticationOutcome.Cancelled;
            error = cancellationToken.IsCancellationRequested
                ? "Authentication cancelled by user."
                : $"Authentication timed out after {timeout.TotalSeconds:F0}s.";
            return BuildResult();
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
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
                    // Best-effort cleanup only. Step 06 proves authentication;
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

        SteamAuthenticationResult BuildResult() => new(
            Outcome: outcome,
            CmConnected: cmConnected,
            AuthSessionStarted: authSessionStarted,
            LoggedOnCallbackReceived: loggedOnCallbackReceived,
            LogonResult: logonResult,
            ExtendedLogonResult: extendedLogonResult,
            AccountName: accountName,
            SteamId64: steamId64,
            GuardChallenge: guardChallenge,
            CurrentEndPoint: currentEndPoint,
            Elapsed: sw.Elapsed,
            Error: error);
    }

    private static async Task PumpCallbacks(
        CallbackManager manager,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(50));
            await Task.Yield();
        }
    }

    private static TaskCompletionSource<T> NewTcs<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}
