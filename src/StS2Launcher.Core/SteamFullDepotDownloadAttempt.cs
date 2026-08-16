using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using SteamKit2;
using SteamKit2.CDN;

namespace StS2Launcher.Core;

/// <summary>
/// Step 10 boundary: re-prove the saved Steam session, ownership and Step 08
/// PICS metadata, then download exactly one selected direct public depot. Every
/// regular file is queued into an isolated staging directory, reconstructed from
/// Steam chunks, SHA-1 verified against the manifest, and only after the entire
/// queue succeeds is the staging directory atomically renamed to its final
/// manifest-specific location.
///
/// Step 10 intentionally has no resume, update/repair/install orchestration,
/// multi-depot app-install or later runtime/service integration behavior.
/// </summary>
public sealed class SteamFullDepotDownloadAttempt
{
    public const uint TargetAppId = SteamOwnershipVerificationAttempt.TargetAppId;
    public const int MaxCdnServersToTry = 8;

    private readonly SteamSessionStore _sessionStore;
    private readonly string _outputRootDirectory;

    public SteamFullDepotDownloadAttempt(
        SteamSessionStore sessionStore,
        string outputRootDirectory)
    {
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputRootDirectory));

        _outputRootDirectory = outputRootDirectory;
    }

    public async Task<SteamFullDepotDownloadResult> RunAsync(
        TimeSpan timeout,
        IProgress<SteamDepotDownloadProgress>? progress = null,
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
                SteamFullDepotDownloadOutcome.InvalidLocalSession,
                savedSessionFound: true,
                sw.Elapsed,
                $"Saved session could not be read: {ex.GetType().Name}: {ex.Message}");
        }

        if (savedSession is null)
        {
            return EmptyResult(
                SteamFullDepotDownloadOutcome.NoSavedSession,
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
        var plannedFileCount = 0;
        var plannedChunkCount = 0;
        ulong plannedBytes = 0;
        var completedFileCount = 0;
        var verifiedFileCount = 0;
        var downloadedChunkCount = 0;
        ulong downloadedUncompressedBytes = 0;
        var cdnAuthTokenRequested = false;
        var cdnAuthTokenReceived = false;
        var stagingDirectoryCreated = false;
        var stagingDirectoryCleaned = false;
        var finalDirectoryCommitted = false;
        string? stagingPath = null;
        string? outputRelativePath = null;
        string? currentEndPoint = null;
        string? returnedSteamId64 = null;
        var outcome = SteamFullDepotDownloadOutcome.Failed;
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
                outcome = SteamFullDepotDownloadOutcome.SessionRejected;
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
                outcome = SteamFullDepotDownloadOutcome.IdentityMismatch;
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
                outcome = SteamFullDepotDownloadOutcome.OwnershipNotProven;
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
                outcome = SteamFullDepotDownloadOutcome.PicsAccessTokenDenied;
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
                outcome = SteamFullDepotDownloadOutcome.ProductInfoUnavailable;
                error = $"PICS did not return app info for App ID {TargetAppId}.";
                return BuildResult();
            }

            if (picsMissingToken)
            {
                outcome = SteamFullDepotDownloadOutcome.MissingPicsToken;
                error = $"PICS reported that App ID {TargetAppId} still requires an access token.";
                return BuildResult();
            }

            selectedDepot = SteamDepotDownloadPlanner.SelectDepot(depots, TargetAppId);
            if (selectedDepot is null)
            {
                outcome = SteamFullDepotDownloadOutcome.NoSuitableDepot;
                error = "No direct depot with a visible public manifest was available for the minimal Step 10 depot download.";
                return BuildResult();
            }

            // Retain the proven Step 09 depot-key boundary; the key stays in memory only.
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
                outcome = SteamFullDepotDownloadOutcome.DepotKeyDenied;
                error = $"Steam did not return a usable key for depot {selectedDepot.DepotId}: {depotKey.Result}.";
                return BuildResult();
            }

            // Retain the proven Step 09 manifest request-code boundary; it stays in memory only.
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
                outcome = SteamFullDepotDownloadOutcome.ManifestRequestCodeUnavailable;
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
                outcome = SteamFullDepotDownloadOutcome.NoCdnServers;
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
                outcome = SteamFullDepotDownloadOutcome.ManifestDownloadFailed;
                error = $"Could not download manifest {selectedDepot.ManifestId} for depot {selectedDepot.DepotId} from the bounded CDN server set.";
                return BuildResult();
            }
            manifestDownloaded = true;

            SteamDepotDownloadPlan plan;
            try
            {
                plan = SteamDepotDownloadPlanner.Create(selectedDepot, manifest);
            }
            catch (Exception ex) when (ex is InvalidDataException or OverflowException)
            {
                outcome = SteamFullDepotDownloadOutcome.InvalidManifest;
                error = $"Selected Steam manifest cannot be safely materialized in Step 10: {ex.Message}";
                return BuildResult();
            }

            plannedFileCount = plan.TotalFileCount;
            plannedChunkCount = plan.TotalChunkCount;
            plannedBytes = plan.TotalBytes;

            progress?.Report(new SteamDepotDownloadProgress(
                SteamDepotDownloadPhase.Preparing,
                CompletedFiles: 0,
                TotalFiles: plannedFileCount,
                CompletedChunks: 0,
                TotalChunks: plannedChunkCount,
                CompletedBytes: 0,
                TotalBytes: plannedBytes,
                CurrentFile: null));

            outputRelativePath = BuildOutputRelativePath(selectedDepot);
            var finalPath = ResolveOutputPath(outputRelativePath);
            if (Directory.Exists(finalPath) || File.Exists(finalPath))
            {
                outcome = SteamFullDepotDownloadOutcome.OutputAlreadyExists;
                error = "The manifest-specific Step 10 final directory already exists. Step 10 does not implement update/repair/overwrite behavior.";
                return BuildResult();
            }

            var stagingRelativePath = Path.Combine(
                "Step10-FullDepot",
                ".staging",
                $"{selectedDepot.DepotId}-{selectedDepot.ManifestId}-{Guid.NewGuid():N}");
            stagingPath = ResolveOutputPath(stagingRelativePath);

            try
            {
                Directory.CreateDirectory(stagingPath);
                stagingDirectoryCreated = Directory.Exists(stagingPath);
                if (!stagingDirectoryCreated)
                    throw new IOException("The Step 10 staging directory was not created.");

                foreach (var directory in plan.Directories)
                {
                    token.ThrowIfCancellationRequested();
                    Directory.CreateDirectory(ResolveChildPath(stagingPath, directory));
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                outcome = SteamFullDepotDownloadOutcome.FileWriteFailed;
                error = $"Could not prepare the isolated Step 10 staging tree: {ex.GetType().Name}: {ex.Message}";
                return FinishResult();
            }

            foreach (var file in plan.Files)
            {
                token.ThrowIfCancellationRequested();
                var normalized = SteamSingleFileTargetSelector.NormalizeRelativePath(file.FileName);
                var stagedFilePath = ResolveChildPath(stagingPath, normalized);
                var stagedParent = Path.GetDirectoryName(stagedFilePath)
                    ?? throw new InvalidOperationException("Could not resolve staged file parent directory.");
                Directory.CreateDirectory(stagedParent);

                var partPath = stagedFilePath + ".step10.part";
                try
                {
                    if (File.Exists(partPath))
                        File.Delete(partPath);
                    if (File.Exists(stagedFilePath))
                        throw new IOException("A staged file unexpectedly already exists.");

                    progress?.Report(new SteamDepotDownloadProgress(
                        SteamDepotDownloadPhase.Downloading,
                        completedFileCount,
                        plannedFileCount,
                        downloadedChunkCount,
                        plannedChunkCount,
                        downloadedUncompressedBytes,
                        plannedBytes,
                        file.FileName));

                    await using (var output = new FileStream(
                        partPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 128 * 1024,
                        options: FileOptions.Asynchronous))
                    {
                        output.SetLength(checked((long)file.TotalSize));

                        foreach (var chunk in file.Chunks.OrderBy(chunk => chunk.Offset))
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
                                outcome = SteamFullDepotDownloadOutcome.FileDownloadFailed;
                                error = $"Chunk {downloadedChunkCount + 1}/{plannedChunkCount} for '{file.FileName}' did not return its expected uncompressed byte count.";
                                return FinishResult();
                            }

                            output.Position = checked((long)chunk.Offset);
                            await output.WriteAsync(chunkBuffer.AsMemory(0, written), token)
                                .ConfigureAwait(false);

                            downloadedChunkCount++;
                            downloadedUncompressedBytes += (ulong)written;

                            progress?.Report(new SteamDepotDownloadProgress(
                                SteamDepotDownloadPhase.Downloading,
                                completedFileCount,
                                plannedFileCount,
                                downloadedChunkCount,
                                plannedChunkCount,
                                downloadedUncompressedBytes,
                                plannedBytes,
                                file.FileName));
                        }

                        await output.FlushAsync(token).ConfigureAwait(false);
                    }

                    if (new FileInfo(partPath).Length != checked((long)file.TotalSize))
                    {
                        outcome = SteamFullDepotDownloadOutcome.FileWriteFailed;
                        error = $"Staged file size did not match the Steam manifest: {file.FileName}";
                        return FinishResult();
                    }

                    progress?.Report(new SteamDepotDownloadProgress(
                        SteamDepotDownloadPhase.Verifying,
                        completedFileCount,
                        plannedFileCount,
                        downloadedChunkCount,
                        plannedChunkCount,
                        downloadedUncompressedBytes,
                        plannedBytes,
                        file.FileName));

                    byte[] actualHash;
                    await using (var input = new FileStream(
                        partPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 128 * 1024,
                        options: FileOptions.Asynchronous | FileOptions.SequentialScan))
                    using (var sha1 = SHA1.Create())
                    {
                        actualHash = await sha1.ComputeHashAsync(input, token).ConfigureAwait(false);
                    }

                    if (!CryptographicOperations.FixedTimeEquals(actualHash, file.FileHash))
                    {
                        outcome = SteamFullDepotDownloadOutcome.FileHashMismatch;
                        error = $"SHA-1 mismatch for staged file '{file.FileName}'. The entire staging tree will be removed.";
                        return FinishResult();
                    }

                    File.Move(partPath, stagedFilePath, overwrite: false);
                    verifiedFileCount++;
                    completedFileCount++;

                    progress?.Report(new SteamDepotDownloadProgress(
                        SteamDepotDownloadPhase.Downloading,
                        completedFileCount,
                        plannedFileCount,
                        downloadedChunkCount,
                        plannedChunkCount,
                        downloadedUncompressedBytes,
                        plannedBytes,
                        file.FileName));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    outcome = SteamFullDepotDownloadOutcome.FileWriteFailed;
                    error = $"Could not materialize staged file '{file.FileName}': {ex.GetType().Name}: {ex.Message}";
                    return FinishResult();
                }
            }

            if (completedFileCount != plannedFileCount ||
                verifiedFileCount != plannedFileCount ||
                downloadedChunkCount != plannedChunkCount ||
                downloadedUncompressedBytes != plannedBytes)
            {
                outcome = SteamFullDepotDownloadOutcome.Failed;
                error = "The Step 10 queue finished with inconsistent file/chunk/byte totals; final directory was not committed.";
                return FinishResult();
            }

            progress?.Report(new SteamDepotDownloadProgress(
                SteamDepotDownloadPhase.Committing,
                completedFileCount,
                plannedFileCount,
                downloadedChunkCount,
                plannedChunkCount,
                downloadedUncompressedBytes,
                plannedBytes,
                CurrentFile: null));

            try
            {
                var finalParent = Path.GetDirectoryName(finalPath)
                    ?? throw new InvalidOperationException("Could not resolve Step 10 final directory parent.");
                Directory.CreateDirectory(finalParent);

                if (Directory.Exists(finalPath) || File.Exists(finalPath))
                    throw new IOException("The Step 10 final manifest directory appeared before commit.");

                Directory.Move(stagingPath, finalPath);
                finalDirectoryCommitted = Directory.Exists(finalPath) && !Directory.Exists(stagingPath);
                stagingDirectoryCleaned = !Directory.Exists(stagingPath);
                if (!finalDirectoryCommitted)
                    throw new IOException("Atomic staging-directory rename did not produce the final directory.");
                stagingPath = null;
            }
            catch (Exception ex)
            {
                outcome = SteamFullDepotDownloadOutcome.CommitFailed;
                error = $"Verified depot could not be atomically committed: {ex.GetType().Name}: {ex.Message}";
                return FinishResult();
            }

            outcome = SteamFullDepotDownloadOutcome.Downloaded;
            progress?.Report(new SteamDepotDownloadProgress(
                SteamDepotDownloadPhase.Complete,
                completedFileCount,
                plannedFileCount,
                downloadedChunkCount,
                plannedChunkCount,
                downloadedUncompressedBytes,
                plannedBytes,
                CurrentFile: null));
            return BuildResult();
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outcome = SteamFullDepotDownloadOutcome.Cancelled;
                error = "Full-depot download cancelled by user. Any Step 10 staging tree is removed.";
            }
            else
            {
                outcome = SteamFullDepotDownloadOutcome.TimedOut;
                error = $"Full-depot download timed out after {timeout.TotalMinutes:F0} minutes. Any Step 10 staging tree is removed.";
            }

            return FinishResult();
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return FinishResult();
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

            if (!finalDirectoryCommitted)
                CleanupStaging();

            sw.Stop();
        }

        SteamFullDepotDownloadResult FinishResult()
        {
            CleanupStaging();
            return BuildResult();
        }

        void CleanupStaging()
        {
            if (string.IsNullOrWhiteSpace(stagingPath))
                return;

            try
            {
                if (Directory.Exists(stagingPath))
                    Directory.Delete(stagingPath, recursive: true);
                stagingDirectoryCleaned = !Directory.Exists(stagingPath);
            }
            catch (Exception cleanupEx)
            {
                stagingDirectoryCleaned = false;
                error = string.IsNullOrWhiteSpace(error)
                    ? $"Staging cleanup failed: {cleanupEx.GetType().Name}: {cleanupEx.Message}"
                    : $"{error} Staging cleanup also failed: {cleanupEx.GetType().Name}: {cleanupEx.Message}";
            }
        }

        SteamFullDepotDownloadResult BuildResult() => new(
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
            PlannedFileCount: plannedFileCount,
            PlannedChunkCount: plannedChunkCount,
            PlannedBytes: plannedBytes,
            CompletedFileCount: completedFileCount,
            VerifiedFileCount: verifiedFileCount,
            DownloadedChunkCount: downloadedChunkCount,
            DownloadedUncompressedBytes: downloadedUncompressedBytes,
            CdnAuthTokenRequested: cdnAuthTokenRequested,
            CdnAuthTokenReceived: cdnAuthTokenReceived,
            StagingDirectoryCreated: stagingDirectoryCreated,
            StagingDirectoryCleaned: stagingDirectoryCleaned,
            FinalDirectoryCommitted: finalDirectoryCommitted,
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
                catch (SteamKitWebRequestException)
                {
                    // Try the next bounded server.
                }
            }
            catch (SteamKitWebRequestException)
            {
                // Try the next bounded server.
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
                catch (SteamKitWebRequestException)
                {
                    // Try another server.
                }
            }
            catch (SteamKitWebRequestException)
            {
                // Try another server.
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

    private static string BuildOutputRelativePath(SteamSingleFileDepotTarget target) =>
        Path.Combine(
            "Step10-FullDepot",
            target.DepotId.ToString(),
            target.ManifestId.ToString());

    private string ResolveOutputPath(string outputRelativePath)
    {
        var root = Path.GetFullPath(_outputRootDirectory);
        var candidate = Path.GetFullPath(Path.Combine(root, outputRelativePath));
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(rootPrefix, StringComparison.Ordinal))
            throw new InvalidOperationException("Resolved Step 10 output escaped the configured app data directory.");

        return candidate;
    }

    private static string ResolveChildPath(string rootDirectory, string safeRelativePath)
    {
        if (!SteamSingleFileTargetSelector.IsSafeRelativePath(safeRelativePath))
            throw new InvalidDataException("Unsafe Steam manifest child path.");

        var root = Path.GetFullPath(rootDirectory);
        var candidate = Path.GetFullPath(Path.Combine(root, safeRelativePath));
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(rootPrefix, StringComparison.Ordinal))
            throw new InvalidDataException("Resolved Steam manifest path escaped the Step 10 staging directory.");

        return candidate;
    }

    private static SteamFullDepotDownloadResult EmptyResult(
        SteamFullDepotDownloadOutcome outcome,
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
            PlannedFileCount: 0,
            PlannedChunkCount: 0,
            PlannedBytes: 0,
            CompletedFileCount: 0,
            VerifiedFileCount: 0,
            DownloadedChunkCount: 0,
            DownloadedUncompressedBytes: 0,
            CdnAuthTokenRequested: false,
            CdnAuthTokenReceived: false,
            StagingDirectoryCreated: false,
            StagingDirectoryCleaned: false,
            FinalDirectoryCommitted: false,
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
