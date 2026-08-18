using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using SteamKit2;
using SteamKit2.CDN;

namespace StS2Launcher.Core;

/// <summary>
/// Step 09 boundary: re-prove the saved Steam session, ownership and Step 08
/// PICS metadata, then retrieve exactly one public depot manifest and download
/// exactly one small regular file (<= 2 MiB). The manifest, depot key, request
/// code, CDN auth token and downloaded bytes remain in memory only. Only the
/// final SHA-1-verified file is persisted.
///
/// This is intentionally not a depot downloader: no multi-file queue, resume,
/// update, repair, install state, manifest persistence or chunk cache exists.
/// </summary>
public sealed class SteamSingleFileDownloadAttempt
{
    public const uint TargetAppId = SteamOwnershipVerificationAttempt.TargetAppId;
    public const int MaxCdnServersToTry = 8;

    private readonly SteamSessionStore _sessionStore;
    private readonly string _outputRootDirectory;

    public SteamSingleFileDownloadAttempt(
        SteamSessionStore sessionStore,
        string outputRootDirectory)
    {
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputRootDirectory));

        _outputRootDirectory = outputRootDirectory;
    }

    public async Task<SteamSingleFileDownloadResult> RunAsync(
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
                SteamSingleFileDownloadOutcome.InvalidLocalSession,
                savedSessionFound: true,
                sw.Elapsed,
                $"Saved session could not be read: {ex.GetType().Name}: {ex.Message}");
        }

        if (savedSession is null)
        {
            return EmptyResult(
                SteamSingleFileDownloadOutcome.NoSavedSession,
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
        IReadOnlyList<SteamDepotDiscovery> depots = Array.Empty<SteamDepotDiscovery>();
        SteamSingleFileDepotTarget? selectedDepot = null;
        var depotKeyRequested = false;
        EResult? depotKeyResult = null;
        var depotKeyReceived = false;
        var manifestRequestCodeRequested = false;
        var manifestRequestCodeReceived = false;
        var eligibleCdnServerCount = 0;
        var manifestDownloaded = false;
        string? selectedFileName = null;
        ulong selectedFileBytes = 0;
        var selectedFileChunkCount = 0;
        var chunksDownloaded = 0;
        ulong downloadedUncompressedBytes = 0;
        var cdnAuthTokenRequested = false;
        var cdnAuthTokenReceived = false;
        var fileHashMatched = false;
        var fileWritten = false;
        string? outputRelativePath = null;
        string? currentEndPoint = null;
        string? returnedSteamId64 = null;
        var outcome = SteamSingleFileDownloadOutcome.Failed;
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
            var steamContent = steamClient.GetHandler<SteamContent>()
                ?? throw new InvalidOperationException("SteamContent handler is unavailable.");

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
                    depots = SteamContentDiscoveryParser.Parse(appInfo.KeyValues);
                }

                if (!callback.ResponsePending)
                    picsProductDoneTcs.TrySetResult(true);
            });

            callbackPump = Task.Run(() => PumpCallbacks(manager, pumpCts.Token));
            steamClient.Connect();

            var connectWinner = await Task.WhenAny(connectedTcs.Task, disconnectedTcs.Task)
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

            var logonWinner = await Task.WhenAny(loggedOnTcs.Task, disconnectedTcs.Task)
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
                outcome = SteamSingleFileDownloadOutcome.SessionRejected;
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
                outcome = SteamSingleFileDownloadOutcome.IdentityMismatch;
                error = "Saved session authenticated, but the returned SteamID did not match the stored identity.";
                return BuildResult();
            }

            // Step 07 regression gate.
            steamApps.GetAppOwnershipTicket(TargetAppId);
            var ownershipWinner = await Task.WhenAny(ownershipTcs.Task, disconnectedTcs.Task)
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
                outcome = SteamSingleFileDownloadOutcome.OwnershipNotProven;
                error = $"Step 07 ownership gate failed: result={ownershipResult}, app={ownershipAppId}, ticketBytes={ownershipTicketLength}.";
                return BuildResult();
            }

            // Step 08 regression gate: retrieve app metadata and select exactly
            // one direct public depot. The PICS access-token value stays local.
            steamApps.PICSGetAccessTokens(TargetAppId, package: null);
            var tokensWinner = await Task.WhenAny(picsTokensTcs.Task, disconnectedTcs.Task)
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
                outcome = SteamSingleFileDownloadOutcome.PicsAccessTokenDenied;
                error = $"Steam denied the PICS access token for App ID {TargetAppId}.";
                return BuildResult();
            }

            var picsAccessToken = 0UL;
            if (tokenCallback.AppTokens.TryGetValue(TargetAppId, out var returnedAccessToken))
            {
                picsAccessToken = returnedAccessToken;
                picsAccessTokenReceived = true;
            }

            steamApps.PICSGetProductInfo(
                app: new SteamApps.PICSRequest(TargetAppId, picsAccessToken),
                package: null,
                metaDataOnly: false);

            var productWinner = await Task.WhenAny(picsProductDoneTcs.Task, disconnectedTcs.Task)
                .WaitAsync(token)
                .ConfigureAwait(false);

            if (productWinner == disconnectedTcs.Task || !picsProductInfoCallbackReceived)
            {
                error = "Steam disconnected before the final PICSProductInfoCallback.";
                return BuildResult();
            }

            if (!picsAppInfoFound)
            {
                outcome = SteamSingleFileDownloadOutcome.ProductInfoUnavailable;
                error = $"PICS did not return app info for App ID {TargetAppId}.";
                return BuildResult();
            }

            if (picsMissingToken)
            {
                outcome = SteamSingleFileDownloadOutcome.MissingPicsToken;
                error = $"PICS reported that App ID {TargetAppId} still requires an access token.";
                return BuildResult();
            }

            selectedDepot = SteamSingleFileTargetSelector.SelectDepot(depots, TargetAppId);
            if (selectedDepot is null)
            {
                outcome = SteamSingleFileDownloadOutcome.NoSuitableDepot;
                error = "No direct depot with a visible public manifest was available for the controlled Step 09 test.";
                return BuildResult();
            }

            // First new content-access boundary: one depot key, kept in memory.
            depotKeyRequested = true;
            var depotKey = await Task.Run(async () =>
                    await steamApps.GetDepotDecryptionKey(selectedDepot.DepotId, TargetAppId))
                .WaitAsync(token)
                .ConfigureAwait(false);
            depotKeyResult = depotKey.Result;
            depotKeyReceived = depotKey.Result == EResult.OK &&
                               depotKey.DepotID == selectedDepot.DepotId &&
                               depotKey.DepotKey is { Length: > 0 };

            if (!depotKeyReceived)
            {
                outcome = SteamSingleFileDownloadOutcome.DepotKeyDenied;
                error = $"Steam did not return a usable key for depot {selectedDepot.DepotId}: {depotKey.Result}.";
                return BuildResult();
            }

            // One short-lived manifest request code, also kept in memory.
            manifestRequestCodeRequested = true;
            var manifestRequestCode = await Task.Run(async () =>
                    await steamContent.GetManifestRequestCode(
                        selectedDepot.DepotId,
                        TargetAppId,
                        selectedDepot.ManifestId,
                        selectedDepot.Branch))
                .WaitAsync(token)
                .ConfigureAwait(false);
            manifestRequestCodeReceived = manifestRequestCode != 0;

            if (!manifestRequestCodeReceived)
            {
                outcome = SteamSingleFileDownloadOutcome.ManifestRequestCodeUnavailable;
                error = $"Steam returned no manifest request code for depot {selectedDepot.DepotId}, manifest {selectedDepot.ManifestId}.";
                return BuildResult();
            }

            var allServers = await Task.Run(async () =>
                    await steamContent.GetServersForSteamPipe())
                .WaitAsync(token)
                .ConfigureAwait(false);

            var proxyServer = allServers.FirstOrDefault(server => server.UseAsProxy);
            var eligibleServers = allServers
                .Where(server =>
                    (server.AllowedAppIds.Length == 0 || server.AllowedAppIds.Contains(TargetAppId)) &&
                    (string.Equals(server.Type, "SteamCache", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(server.Type, "CDN", StringComparison.OrdinalIgnoreCase)))
                .OrderBy(server => server.WeightedLoad)
                .Take(MaxCdnServersToTry)
                .ToArray();
            eligibleCdnServerCount = eligibleServers.Length;

            if (eligibleServers.Length == 0)
            {
                outcome = SteamSingleFileDownloadOutcome.NoCdnServers;
                error = "Steam returned no eligible CDN/SteamCache servers for App ID 2868840.";
                return BuildResult();
            }

            var cdnClient = new Client(steamClient);
            var cdnTokensByHost = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var manifest = await DownloadManifestFromAnyServerAsync(
                    steamContent,
                    cdnClient,
                    eligibleServers,
                    proxyServer,
                    selectedDepot,
                    depotKey.DepotKey,
                    manifestRequestCode,
                    cdnTokensByHost,
                    token,
                    onAuthRequest: () => cdnAuthTokenRequested = true,
                    onAuthSuccess: () => cdnAuthTokenReceived = true)
                .ConfigureAwait(false);

            if (manifest is null)
            {
                outcome = SteamSingleFileDownloadOutcome.ManifestDownloadFailed;
                error = $"Could not download manifest {selectedDepot.ManifestId} for depot {selectedDepot.DepotId} from the bounded CDN server set.";
                return BuildResult();
            }
            manifestDownloaded = true;

            var selectedFile = SteamSingleFileTargetSelector.SelectFile(manifest);
            if (selectedFile is null)
            {
                outcome = SteamSingleFileDownloadOutcome.NoSmallFile;
                error = $"Manifest {selectedDepot.ManifestId} contained no safe non-empty regular file <= {SteamSingleFileTargetSelector.MaxTargetFileBytes} bytes.";
                return BuildResult();
            }

            selectedFileName = selectedFile.FileName;
            selectedFileBytes = selectedFile.TotalSize;
            selectedFileChunkCount = selectedFile.Chunks.Count;

            var fileBytes = new byte[(int)selectedFile.TotalSize];
            foreach (var chunk in selectedFile.Chunks.OrderBy(chunk => chunk.Offset))
            {
                token.ThrowIfCancellationRequested();
                var chunkBuffer = new byte[checked((int)chunk.UncompressedLength)];

                var written = await DownloadChunkFromAnyServerAsync(
                        steamContent,
                        cdnClient,
                        eligibleServers,
                        proxyServer,
                        selectedDepot.DepotId,
                        depotKey.DepotKey,
                        chunk,
                        chunkBuffer,
                        cdnTokensByHost,
                        token,
                        onAuthRequest: () => cdnAuthTokenRequested = true,
                        onAuthSuccess: () => cdnAuthTokenReceived = true)
                    .ConfigureAwait(false);

                if (written <= 0 || written != chunk.UncompressedLength)
                {
                    outcome = SteamSingleFileDownloadOutcome.ChunkDownloadFailed;
                    error = $"Chunk {chunksDownloaded + 1}/{selectedFileChunkCount} did not return its expected uncompressed byte count.";
                    return BuildResult();
                }

                Buffer.BlockCopy(
                    chunkBuffer,
                    0,
                    fileBytes,
                    checked((int)chunk.Offset),
                    written);
                chunksDownloaded++;
                downloadedUncompressedBytes += (ulong)written;
            }

            fileHashMatched = selectedFile.FileHash is { Length: > 0 } &&
                              CryptographicOperations.FixedTimeEquals(
                                  SHA1.HashData(fileBytes),
                                  selectedFile.FileHash);
            if (!fileHashMatched)
            {
                outcome = SteamSingleFileDownloadOutcome.FileHashMismatch;
                error = "The assembled file SHA-1 did not match the Steam manifest. Nothing was written.";
                return BuildResult();
            }

            try
            {
                outputRelativePath = BuildOutputRelativePath(selectedDepot, selectedFile.FileName);
                var finalPath = ResolveOutputPath(outputRelativePath);
                var parent = Path.GetDirectoryName(finalPath)
                    ?? throw new InvalidOperationException("Could not resolve the Step 09 output directory.");
                Directory.CreateDirectory(parent);

                var tempPath = finalPath + ".step09.tmp";
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                    await File.WriteAllBytesAsync(tempPath, fileBytes, token).ConfigureAwait(false);
                    File.Move(tempPath, finalPath, overwrite: true);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }

                fileWritten = File.Exists(finalPath) && new FileInfo(finalPath).Length == (long)selectedFile.TotalSize;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                outcome = SteamSingleFileDownloadOutcome.FileWriteFailed;
                error = $"Verified file could not be written: {ex.GetType().Name}: {ex.Message}";
                return BuildResult();
            }

            if (!fileWritten)
            {
                outcome = SteamSingleFileDownloadOutcome.FileWriteFailed;
                error = "The final file was not present at its exact expected size after the atomic write.";
                return BuildResult();
            }

            outcome = SteamSingleFileDownloadOutcome.Downloaded;
            return BuildResult();
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outcome = SteamSingleFileDownloadOutcome.Cancelled;
                error = "Single-file download cancelled by user.";
            }
            else
            {
                outcome = SteamSingleFileDownloadOutcome.TimedOut;
                error = $"Single-file download timed out after {timeout.TotalSeconds:F0}s.";
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

        SteamSingleFileDownloadResult BuildResult() => new(
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
            SelectedDepotId: selectedDepot?.DepotId,
            SelectedManifestId: selectedDepot?.ManifestId,
            SelectedBranch: selectedDepot?.Branch,
            SelectedDepotOsList: selectedDepot?.OsList,
            DepotKeyRequested: depotKeyRequested,
            DepotKeyResult: depotKeyResult,
            DepotKeyReceived: depotKeyReceived,
            ManifestRequestCodeRequested: manifestRequestCodeRequested,
            ManifestRequestCodeReceived: manifestRequestCodeReceived,
            EligibleCdnServerCount: eligibleCdnServerCount,
            ManifestDownloaded: manifestDownloaded,
            SelectedFileName: selectedFileName,
            SelectedFileBytes: selectedFileBytes,
            SelectedFileChunkCount: selectedFileChunkCount,
            ChunksDownloaded: chunksDownloaded,
            DownloadedUncompressedBytes: downloadedUncompressedBytes,
            CdnAuthTokenRequested: cdnAuthTokenRequested,
            CdnAuthTokenReceived: cdnAuthTokenReceived,
            FileHashMatched: fileHashMatched,
            FileWritten: fileWritten,
            OutputRelativePath: outputRelativePath,
            AccountName: savedSession.AccountName,
            SteamId64: returnedSteamId64 ?? savedSession.SteamId64,
            CurrentEndPoint: currentEndPoint,
            Elapsed: sw.Elapsed,
            Error: error,
            LoginId: loginId);
    }

    private static async Task<DepotManifest?> DownloadManifestFromAnyServerAsync(
        SteamContent steamContent,
        Client cdnClient,
        IReadOnlyList<Server> servers,
        Server? proxyServer,
        SteamSingleFileDepotTarget target,
        byte[] depotKey,
        ulong manifestRequestCode,
        IDictionary<string, string> cdnTokensByHost,
        CancellationToken cancellationToken,
        Action onAuthRequest,
        Action onAuthSuccess)
    {
        foreach (var server in servers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            cdnTokensByHost.TryGetValue(server.Host, out var cdnToken);

            try
            {
                return await cdnClient.DownloadManifestAsync(
                        target.DepotId,
                        target.ManifestId,
                        manifestRequestCode,
                        server,
                        depotKey,
                        proxyServer,
                        cdnToken)
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (SteamKitWebRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden && cdnToken is null)
            {
                var tokenValue = await RequestCdnTokenAsync(
                        steamContent,
                        target.DepotId,
                        server.Host,
                        cancellationToken,
                        onAuthRequest,
                        onAuthSuccess)
                    .ConfigureAwait(false);
                if (tokenValue is null)
                    continue;

                cdnTokensByHost[server.Host] = tokenValue;
                try
                {
                    return await cdnClient.DownloadManifestAsync(
                            target.DepotId,
                            target.ManifestId,
                            manifestRequestCode,
                            server,
                            depotKey,
                            proxyServer,
                            tokenValue)
                        .WaitAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (TimeoutException) when (!cancellationToken.IsCancellationRequested)
                {
                    // iOS can surface SteamKit's bounded CDN cancellation as TimeoutException.
                    // Treat it as a per-endpoint transport failure and try another server.
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Per-server timeout inside SteamKit: try another server.
                }
                catch (SteamKitWebRequestException)
                {
                    // SteamKit-specific HTTP failure: try the next bounded server.
                }
                catch (HttpRequestException)
                {
                    // Transport failure on this endpoint: try another bounded server.
                }
                catch (IOException)
                {
                    // Stream failure on this endpoint: try another bounded server.
                }
            }
            catch (SteamKitWebRequestException)
            {
                // Try the next bounded server.
            }
            catch (TimeoutException) when (!cancellationToken.IsCancellationRequested)
            {
                // iOS can surface SteamKit's bounded CDN cancellation as TimeoutException.
                // Treat it as a recoverable per-endpoint timeout.
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Per-server timeout inside SteamKit: try another server.
            }
            catch (HttpRequestException)
            {
                // Transport failure on this endpoint: try another bounded server.
            }
            catch (IOException)
            {
                // Stream failure on this endpoint: try another bounded server.
            }
        }

        return null;
    }

    private static async Task<int> DownloadChunkFromAnyServerAsync(
        SteamContent steamContent,
        Client cdnClient,
        IReadOnlyList<Server> servers,
        Server? proxyServer,
        uint depotId,
        byte[] depotKey,
        DepotManifest.ChunkData chunk,
        byte[] destination,
        IDictionary<string, string> cdnTokensByHost,
        CancellationToken cancellationToken,
        Action onAuthRequest,
        Action onAuthSuccess)
    {
        foreach (var server in servers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            cdnTokensByHost.TryGetValue(server.Host, out var cdnToken);

            try
            {
                return await cdnClient.DownloadDepotChunkAsync(
                        depotId,
                        chunk,
                        server,
                        destination,
                        depotKey,
                        proxyServer,
                        cdnToken)
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (SteamKitWebRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden && cdnToken is null)
            {
                var tokenValue = await RequestCdnTokenAsync(
                        steamContent,
                        depotId,
                        server.Host,
                        cancellationToken,
                        onAuthRequest,
                        onAuthSuccess)
                    .ConfigureAwait(false);
                if (tokenValue is null)
                    continue;

                cdnTokensByHost[server.Host] = tokenValue;
                try
                {
                    return await cdnClient.DownloadDepotChunkAsync(
                            depotId,
                            chunk,
                            server,
                            destination,
                            depotKey,
                            proxyServer,
                            tokenValue)
                        .WaitAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (TimeoutException) when (!cancellationToken.IsCancellationRequested)
                {
                    // iOS can surface SteamKit's bounded CDN cancellation as TimeoutException.
                    // Treat it as a per-endpoint transport failure and try another server.
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Per-server timeout inside SteamKit: try another server.
                }
                catch (SteamKitWebRequestException)
                {
                    // SteamKit-specific HTTP failure: try another server.
                }
                catch (HttpRequestException)
                {
                    // Transport failure on this endpoint: try another bounded server.
                }
                catch (IOException)
                {
                    // Stream failure on this endpoint: try another bounded server.
                }
            }
            catch (SteamKitWebRequestException)
            {
                // Try another server.
            }
            catch (TimeoutException) when (!cancellationToken.IsCancellationRequested)
            {
                // iOS can surface SteamKit's bounded CDN cancellation as TimeoutException.
                // Treat it as a recoverable per-endpoint timeout.
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Per-server timeout inside SteamKit: try another server.
            }
            catch (HttpRequestException)
            {
                // Transport failure on this endpoint: try another bounded server.
            }
            catch (IOException)
            {
                // Stream failure on this endpoint: try another bounded server.
            }
        }

        return 0;
    }

    private static async Task<string?> RequestCdnTokenAsync(
        SteamContent steamContent,
        uint depotId,
        string host,
        CancellationToken cancellationToken,
        Action onAuthRequest,
        Action onAuthSuccess)
    {
        onAuthRequest();
        var auth = await Task.Run(async () =>
                await steamContent.GetCDNAuthToken(TargetAppId, depotId, host))
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        if (auth.Result != EResult.OK || string.IsNullOrWhiteSpace(auth.Token))
            return null;

        onAuthSuccess();
        return auth.Token;
    }

    private string BuildOutputRelativePath(
        SteamSingleFileDepotTarget target,
        string manifestFileName)
    {
        var safeManifestPath = SteamSingleFileTargetSelector.NormalizeRelativePath(manifestFileName);
        return Path.Combine(
            "Step09-SingleFile",
            target.DepotId.ToString(),
            target.ManifestId.ToString(),
            safeManifestPath);
    }

    private string ResolveOutputPath(string outputRelativePath)
    {
        var root = Path.GetFullPath(_outputRootDirectory);
        var candidate = Path.GetFullPath(Path.Combine(root, outputRelativePath));
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(rootPrefix, StringComparison.Ordinal))
            throw new InvalidOperationException("Resolved Step 09 output escaped the configured app data directory.");

        return candidate;
    }

    private static SteamSingleFileDownloadResult EmptyResult(
        SteamSingleFileDownloadOutcome outcome,
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
            SelectedDepotId: null,
            SelectedManifestId: null,
            SelectedBranch: null,
            SelectedDepotOsList: null,
            DepotKeyRequested: false,
            DepotKeyResult: null,
            DepotKeyReceived: false,
            ManifestRequestCodeRequested: false,
            ManifestRequestCodeReceived: false,
            EligibleCdnServerCount: 0,
            ManifestDownloaded: false,
            SelectedFileName: null,
            SelectedFileBytes: 0,
            SelectedFileChunkCount: 0,
            ChunksDownloaded: 0,
            DownloadedUncompressedBytes: 0,
            CdnAuthTokenRequested: false,
            CdnAuthTokenReceived: false,
            FileHashMatched: false,
            FileWritten: false,
            OutputRelativePath: null,
            AccountName: null,
            SteamId64: null,
            CurrentEndPoint: null,
            Elapsed: elapsed,
            Error: error,
            LoginId: null);

    private static TaskCompletionSource<T> NewTcs<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static void PumpCallbacks(CallbackManager manager, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
            manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(100));
    }
}
