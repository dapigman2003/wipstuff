using System.Diagnostics;
using System.Net.Http;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 07 boundary: authenticate with the already-proven saved Steam session
/// and request an app ownership ticket for Slay the Spire 2 only.
///
/// No PICS product info, depot, manifest, CDN, or download request is made.
/// The ownership-ticket bytes are never logged, displayed, or persisted.
/// </summary>
public sealed class SteamOwnershipVerificationAttempt
{
    public const uint TargetAppId = 2868840;

    private readonly SteamSessionStore _sessionStore;

    public SteamOwnershipVerificationAttempt(SteamSessionStore sessionStore)
    {
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
    }

    public async Task<SteamOwnershipVerificationResult> RunAsync(
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
            return new SteamOwnershipVerificationResult(
                Outcome: SteamOwnershipVerificationOutcome.InvalidLocalSession,
                TargetAppId: TargetAppId,
                SavedSessionFound: true,
                CmConnected: false,
                LoggedOnCallbackReceived: false,
                LogonResult: null,
                ExtendedLogonResult: null,
                IdentityMatched: false,
                OwnershipTicketCallbackReceived: false,
                OwnershipResult: null,
                OwnershipAppId: null,
                OwnershipTicketLength: 0,
                AccountName: null,
                SteamId64: null,
                CurrentEndPoint: null,
                Elapsed: sw.Elapsed,
                Error: $"Saved session could not be read: {ex.GetType().Name}: {ex.Message}");
        }

        if (savedSession is null)
        {
            return new SteamOwnershipVerificationResult(
                Outcome: SteamOwnershipVerificationOutcome.NoSavedSession,
                TargetAppId: TargetAppId,
                SavedSessionFound: false,
                CmConnected: false,
                LoggedOnCallbackReceived: false,
                LogonResult: null,
                ExtendedLogonResult: null,
                IdentityMatched: false,
                OwnershipTicketCallbackReceived: false,
                OwnershipResult: null,
                OwnershipAppId: null,
                OwnershipTicketLength: 0,
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
        Task? callbackPump = null;
        var cmConnected = false;
        var loggedOnCallbackReceived = false;
        EResult? logonResult = null;
        EResult? extendedLogonResult = null;
        var identityMatched = false;
        var ownershipTicketCallbackReceived = false;
        EResult? ownershipResult = null;
        uint? ownershipAppId = null;
        var ownershipTicketLength = 0;
        string? currentEndPoint = null;
        string? returnedSteamId64 = null;
        var outcome = SteamOwnershipVerificationOutcome.Failed;
        string? error = null;
        uint? loginId = null;

        try
        {
            HttpClient Factory(HttpClientPurpose purpose) =>
                SteamHttpClientFactory.Create(purpose);

            var configuration = SteamConfiguration.Create(builder => builder
                .WithProtocolTypes(ProtocolTypes.WebSocket)
                .WithHttpClientFactory(Factory));

            steamClient = new SteamClient(configuration);
            var steamUser = steamClient.GetHandler<SteamUser>()
                ?? throw new InvalidOperationException("SteamUser handler is unavailable.");
            var steamApps = steamClient.GetHandler<SteamApps>()
                ?? throw new InvalidOperationException("SteamApps handler is unavailable.");

            var manager = new CallbackManager(steamClient);
            var connectedTcs = NewTcs<SteamClient.ConnectedCallback>();
            var disconnectedTcs = NewTcs<SteamClient.DisconnectedCallback>();
            var loggedOnTcs = NewTcs<SteamUser.LoggedOnCallback>();
            var ownershipTcs = NewTcs<SteamApps.AppOwnershipTicketCallback>();

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
            manager.Subscribe<SteamApps.AppOwnershipTicketCallback>(callback =>
            {
                ownershipTicketCallbackReceived = true;
                ownershipResult = callback.Result;
                ownershipAppId = callback.AppID;
                ownershipTicketLength = callback.Ticket?.Length ?? 0;
                ownershipTcs.TrySetResult(callback);
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

            var logOnDetails = SteamPersistentLogOnDetails.Create(
                savedSession.AccountName,
                savedSession.RefreshToken);
            loginId = logOnDetails.LoginID;
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
                return BuildResult();
            }

            if (logonResult != EResult.OK)
            {
                outcome = SteamOwnershipVerificationOutcome.SessionRejected;
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
                outcome = SteamOwnershipVerificationOutcome.IdentityMismatch;
                error = "Saved session authenticated, but the returned SteamID did not match the stored identity.";
                return BuildResult();
            }

            // This is the one new Step 07 network capability.
            // The callback result and ticket length are observed; the ticket
            // bytes themselves are deliberately discarded after classification.
            steamApps.GetAppOwnershipTicket(TargetAppId);

            var ownershipWinner = await Task.WhenAny(
                    ownershipTcs.Task,
                    disconnectedTcs.Task)
                .WaitAsync(token)
                .ConfigureAwait(false);

            currentEndPoint = steamClient.CurrentEndPoint?.ToString() ?? currentEndPoint;
            if (ownershipWinner == disconnectedTcs.Task || !ownershipTicketCallbackReceived)
            {
                error = "Steam disconnected before AppOwnershipTicketCallback.";
                return BuildResult();
            }

            outcome = SteamOwnershipDecision.EvaluateTicket(
                TargetAppId,
                ownershipResult ?? EResult.Fail,
                ownershipAppId ?? 0,
                ownershipTicketLength);

            if (outcome != SteamOwnershipVerificationOutcome.Owned)
            {
                error = $"Steam did not provide sufficient ownership evidence: result={ownershipResult}, app={ownershipAppId}, ticketBytes={ownershipTicketLength}.";
            }

            return BuildResult();
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outcome = SteamOwnershipVerificationOutcome.Cancelled;
                error = "Ownership verification cancelled by user.";
            }
            else
            {
                outcome = SteamOwnershipVerificationOutcome.TimedOut;
                error = $"Ownership verification timed out after {timeout.TotalSeconds:F0}s.";
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

        SteamOwnershipVerificationResult BuildResult() => new(
            Outcome: outcome,
            TargetAppId: TargetAppId,
            SavedSessionFound: true,
            CmConnected: cmConnected,
            LoggedOnCallbackReceived: loggedOnCallbackReceived,
            LogonResult: logonResult,
            ExtendedLogonResult: extendedLogonResult,
            IdentityMatched: identityMatched,
            OwnershipTicketCallbackReceived: ownershipTicketCallbackReceived,
            OwnershipResult: ownershipResult,
            OwnershipAppId: ownershipAppId,
            OwnershipTicketLength: ownershipTicketLength,
            AccountName: savedSession.AccountName,
            SteamId64: returnedSteamId64 ?? savedSession.SteamId64,
            CurrentEndPoint: currentEndPoint,
            Elapsed: sw.Elapsed,
            Error: error,
            LoginId: loginId);
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
