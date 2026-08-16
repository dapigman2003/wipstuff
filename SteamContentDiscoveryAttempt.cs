using System.Diagnostics;
using System.Net.Http;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 08 boundary: reuse the proven saved Steam session and ownership gate,
/// then request PICS access metadata and product info for App ID 2868840 only.
/// The returned app-info KeyValues are inspected only to enumerate depot IDs
/// and visible branch manifest IDs.
///
/// No depot decryption key, manifest body, CDN server/token, chunk, or file is
/// requested in this step.
/// </summary>
public sealed class SteamContentDiscoveryAttempt
{
    public const uint TargetAppId = SteamOwnershipVerificationAttempt.TargetAppId;

    private readonly SteamSessionStore _sessionStore;

    public SteamContentDiscoveryAttempt(SteamSessionStore sessionStore)
    {
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
    }

    public async Task<SteamContentDiscoveryResult> RunAsync(
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
            return EmptyResult(
                SteamContentDiscoveryOutcome.InvalidLocalSession,
                savedSessionFound: true,
                sw.Elapsed,
                $"Saved session could not be read: {ex.GetType().Name}: {ex.Message}");
        }

        if (savedSession is null)
        {
            return EmptyResult(
                SteamContentDiscoveryOutcome.NoSavedSession,
                savedSessionFound: false,
                sw.Elapsed,
                error: null);
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
        var ownershipProven = false;
        var picsAccessTokenCallbackReceived = false;
        var picsAccessTokenReceived = false;
        var picsProductInfoCallbackReceived = false;
        var picsAppInfoFound = false;
        var picsMissingToken = false;
        uint? picsChangeNumber = null;
        IReadOnlyList<SteamDepotDiscovery> depots = Array.Empty<SteamDepotDiscovery>();
        string? currentEndPoint = null;
        string? returnedSteamId64 = null;
        var outcome = SteamContentDiscoveryOutcome.Failed;
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
            var picsTokensTcs = NewTcs<SteamApps.PICSTokensCallback>();
            var picsProductDoneTcs = NewTcs<bool>();

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
            manager.Subscribe<SteamApps.PICSTokensCallback>(callback =>
            {
                picsAccessTokenCallbackReceived = true;
                picsTokensTcs.TrySetResult(callback);
            });
            manager.Subscribe<SteamApps.PICSProductInfoCallback>(callback =>
            {
                picsProductInfoCallbackReceived = true;

                if (callback.Apps.TryGetValue(TargetAppId, out var appInfo))
                {
                    picsAppInfoFound = true;
                    picsMissingToken = appInfo.MissingToken;
                    picsChangeNumber = appInfo.ChangeNumber;
                    depots = SteamContentDiscoveryParser.Parse(appInfo.KeyValues);
                }

                if (!callback.ResponsePending)
                    picsProductDoneTcs.TrySetResult(true);
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
                outcome = SteamContentDiscoveryOutcome.SessionRejected;
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
                outcome = SteamContentDiscoveryOutcome.IdentityMismatch;
                error = "Saved session authenticated, but the returned SteamID did not match the stored identity.";
                return BuildResult();
            }

            // Reuse the already-proven Step 07 ownership gate before any new
            // Step 08 product-info request is made.
            steamApps.GetAppOwnershipTicket(TargetAppId);

            var ownershipWinner = await Task.WhenAny(
                    ownershipTcs.Task,
                    disconnectedTcs.Task)
                .WaitAsync(token)
                .ConfigureAwait(false);

            if (ownershipWinner == disconnectedTcs.Task || !ownershipTicketCallbackReceived)
            {
                error = "Steam disconnected before AppOwnershipTicketCallback.";
                return BuildResult();
            }

            ownershipProven = SteamOwnershipDecision.EvaluateTicket(
                TargetAppId,
                ownershipResult ?? EResult.Fail,
                ownershipAppId ?? 0,
                ownershipTicketLength) == SteamOwnershipVerificationOutcome.Owned;

            if (!ownershipProven)
            {
                outcome = SteamContentDiscoveryOutcome.OwnershipNotProven;
                error = $"Step 07 ownership gate failed: result={ownershipResult}, app={ownershipAppId}, ticketBytes={ownershipTicketLength}.";
                return BuildResult();
            }

            // First new Step 08 request: obtain the access token needed for
            // authoritative PICS product info. The token value is kept only in
            // this local variable and is never exposed in the result/UI/logs.
            steamApps.PICSGetAccessTokens(TargetAppId, package: null);

            var tokensWinner = await Task.WhenAny(
                    picsTokensTcs.Task,
                    disconnectedTcs.Task)
                .WaitAsync(token)
                .ConfigureAwait(false);

            if (tokensWinner == disconnectedTcs.Task || !picsAccessTokenCallbackReceived)
            {
                error = "Steam disconnected before PICSTokensCallback.";
                return BuildResult();
            }

            var tokenCallback = await picsTokensTcs.Task.ConfigureAwait(false);
            if (tokenCallback.AppTokensDenied.Contains(TargetAppId))
            {
                outcome = SteamContentDiscoveryOutcome.PicsAccessTokenDenied;
                error = $"Steam denied the PICS access token for App ID {TargetAppId}.";
                return BuildResult();
            }

            var accessToken = 0UL;
            if (tokenCallback.AppTokens.TryGetValue(TargetAppId, out var returnedAccessToken))
            {
                accessToken = returnedAccessToken;
                picsAccessTokenReceived = true;
            }

            // Second and final new Step 08 request: retrieve product metadata
            // for this one app. No packages are requested.
            steamApps.PICSGetProductInfo(
                app: new SteamApps.PICSRequest(TargetAppId, accessToken),
                package: null,
                metaDataOnly: false);

            var productWinner = await Task.WhenAny(
                    picsProductDoneTcs.Task,
                    disconnectedTcs.Task)
                .WaitAsync(token)
                .ConfigureAwait(false);

            currentEndPoint = steamClient.CurrentEndPoint?.ToString() ?? currentEndPoint;
            if (productWinner == disconnectedTcs.Task || !picsProductInfoCallbackReceived)
            {
                error = "Steam disconnected before the final PICSProductInfoCallback.";
                return BuildResult();
            }

            if (!picsAppInfoFound)
            {
                outcome = SteamContentDiscoveryOutcome.ProductInfoUnavailable;
                error = $"PICS did not return app info for App ID {TargetAppId}.";
                return BuildResult();
            }

            if (picsMissingToken)
            {
                outcome = SteamContentDiscoveryOutcome.MissingPicsToken;
                error = $"PICS reported that App ID {TargetAppId} still requires an access token.";
                return BuildResult();
            }

            if (depots.Count == 0)
            {
                outcome = SteamContentDiscoveryOutcome.NoDepots;
                error = "PICS app info contained no numeric depot entries.";
                return BuildResult();
            }

            if (depots.Sum(depot => depot.Manifests.Count) == 0)
            {
                outcome = SteamContentDiscoveryOutcome.NoVisibleManifests;
                error = "PICS app info contained depots but no visible branch manifest IDs.";
                return BuildResult();
            }

            outcome = SteamContentDiscoveryOutcome.Discovered;
            return BuildResult();
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outcome = SteamContentDiscoveryOutcome.Cancelled;
                error = "Content discovery cancelled by user.";
            }
            else
            {
                outcome = SteamContentDiscoveryOutcome.TimedOut;
                error = $"Content discovery timed out after {timeout.TotalSeconds:F0}s.";
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

        SteamContentDiscoveryResult BuildResult() => new(
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
            OwnershipTicketLength: ownershipTicketLength,
            OwnershipProven: ownershipProven,
            PicsAccessTokenCallbackReceived: picsAccessTokenCallbackReceived,
            PicsAccessTokenReceived: picsAccessTokenReceived,
            PicsProductInfoCallbackReceived: picsProductInfoCallbackReceived,
            PicsAppInfoFound: picsAppInfoFound,
            PicsMissingToken: picsMissingToken,
            PicsChangeNumber: picsChangeNumber,
            Depots: depots,
            AccountName: savedSession.AccountName,
            SteamId64: returnedSteamId64 ?? savedSession.SteamId64,
            CurrentEndPoint: currentEndPoint,
            Elapsed: sw.Elapsed,
            Error: error,
            LoginId: loginId);
    }

    private static SteamContentDiscoveryResult EmptyResult(
        SteamContentDiscoveryOutcome outcome,
        bool savedSessionFound,
        TimeSpan elapsed,
        string? error) => new(
            Outcome: outcome,
            TargetAppId: TargetAppId,
            SavedSessionFound: savedSessionFound,
            CmConnected: false,
            LoggedOnCallbackReceived: false,
            LogonResult: null,
            ExtendedLogonResult: null,
            IdentityMatched: false,
            OwnershipTicketCallbackReceived: false,
            OwnershipResult: null,
            OwnershipTicketLength: 0,
            OwnershipProven: false,
            PicsAccessTokenCallbackReceived: false,
            PicsAccessTokenReceived: false,
            PicsProductInfoCallbackReceived: false,
            PicsAppInfoFound: false,
            PicsMissingToken: false,
            PicsChangeNumber: null,
            Depots: Array.Empty<SteamDepotDiscovery>(),
            AccountName: null,
            SteamId64: null,
            CurrentEndPoint: null,
            Elapsed: elapsed,
            Error: error,
            LoginId: null);

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
