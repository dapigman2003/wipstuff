using System.Diagnostics;
using System.Net.Http;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 06.2/06.3 saved-session boundary.
///
/// Loads a refresh token from the platform credential store, connects using
/// the Step 05-proven WebSocket route, and performs SteamUser.LogOn without a
/// password or a new Steam Guard challenge.
/// </summary>
public sealed class SteamSessionResumeAttempt
{
    private readonly SteamSessionStore _sessionStore;

    public SteamSessionResumeAttempt(SteamSessionStore sessionStore)
    {
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
    }

    public async Task<SteamSessionResumeResult> RunAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        var sw = Stopwatch.StartNew();
        SteamSavedSession? savedSession;
        try
        {
            savedSession = _sessionStore.Load();
        }
        catch (Exception ex)
        {
            return new SteamSessionResumeResult(
                Outcome: SteamSessionResumeOutcome.InvalidLocalSession,
                SavedSessionFound: true,
                CmConnected: false,
                LoggedOnCallbackReceived: false,
                LogonResult: null,
                ExtendedLogonResult: null,
                IdentityMatched: false,
                AccountName: null,
                SteamId64: null,
                CurrentEndPoint: null,
                Elapsed: sw.Elapsed,
                Error: $"Saved session could not be read: {ex.GetType().Name}: {ex.Message}");
        }

        if (savedSession is null)
        {
            return new SteamSessionResumeResult(
                Outcome: SteamSessionResumeOutcome.NoSavedSession,
                SavedSessionFound: false,
                CmConnected: false,
                LoggedOnCallbackReceived: false,
                LogonResult: null,
                ExtendedLogonResult: null,
                IdentityMatched: false,
                AccountName: null,
                SteamId64: null,
                CurrentEndPoint: null,
                Elapsed: sw.Elapsed,
                Error: null);
        }

        using var operationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        operationCts.CancelAfter(timeout);
        using var pumpCts = CancellationTokenSource.CreateLinkedTokenSource(operationCts.Token);
        var token = operationCts.Token;

        SteamClient? steamClient = null;
        SteamUser? steamUser = null;
        Task? callbackPump = null;
        var cmConnected = false;
        var loggedOnCallbackReceived = false;
        EResult? logonResult = null;
        EResult? extendedLogonResult = null;
        string? currentEndPoint = null;
        string? returnedSteamId64 = null;
        var identityMatched = false;
        var outcome = SteamSessionResumeOutcome.Failed;
        string? error = null;

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

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = savedSession.AccountName,
                AccessToken = savedSession.RefreshToken,
                ShouldRememberPassword = false,
                ClientOSType = EOSType.IOSUnknown,
                MachineName = SteamAuthenticationAttempt.DeviceFriendlyName,
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
                outcome = SteamSessionResumeOutcome.Rejected;
                error = $"Saved Steam session was rejected: {logonResult} / {extendedLogonResult}.";
                return BuildResult();
            }

            returnedSteamId64 = steamUser.SteamID?.ConvertToUInt64().ToString();
            identityMatched = string.Equals(
                returnedSteamId64,
                savedSession.SteamId64,
                StringComparison.Ordinal);

            if (!identityMatched)
            {
                outcome = SteamSessionResumeOutcome.IdentityMismatch;
                error = "Saved session authenticated, but the returned SteamID did not match the stored identity.";
                return BuildResult();
            }

            outcome = SteamSessionResumeOutcome.Authenticated;
            return BuildResult();
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outcome = SteamSessionResumeOutcome.Cancelled;
                error = "Saved-session resume cancelled by user.";
            }
            else
            {
                outcome = SteamSessionResumeOutcome.TimedOut;
                error = $"Saved-session resume timed out after {timeout.TotalSeconds:F0}s.";
            }

            return BuildResult();
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return BuildResult();
        }
        finally
        {
            if (steamUser is not null && outcome == SteamSessionResumeOutcome.Authenticated)
            {
                try { steamUser.LogOff(); } catch { }
            }

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

        SteamSessionResumeResult BuildResult() => new(
            Outcome: outcome,
            SavedSessionFound: true,
            CmConnected: cmConnected,
            LoggedOnCallbackReceived: loggedOnCallbackReceived,
            LogonResult: logonResult,
            ExtendedLogonResult: extendedLogonResult,
            IdentityMatched: identityMatched,
            AccountName: savedSession.AccountName,
            SteamId64: returnedSteamId64 ?? savedSession.SteamId64,
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
            manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(250));
    }
}
