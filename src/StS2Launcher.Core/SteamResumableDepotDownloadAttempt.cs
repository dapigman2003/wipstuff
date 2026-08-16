using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using SteamKit2;
using SteamKit2.CDN;

namespace StS2Launcher.Core;

/// <summary>
/// Step 11 boundary: re-prove the saved Steam session, ownership and Step 08
/// PICS metadata, then resume exactly one selected direct public depot after an
/// interrupted prior attempt. Staging is deterministic per depot/manifest. On
/// relaunch, complete staged files are revalidated by SHA-1 and partial files are
/// scanned chunk-by-chunk using the manifest's Adler-32 checksum; only missing or
/// corrupt chunks are downloaded again. The final directory remains invisible
/// until the complete depot is SHA-1 verified and atomically committed.
///
/// Step 11 intentionally has no update/repair/install orchestration, manifest
/// migration/delta-update logic, multi-depot app-install, Godot, Cloud or Workshop.
/// </summary>
public sealed class SteamResumableDepotDownloadAttempt
{
    public const uint TargetAppId = SteamOwnershipVerificationAttempt.TargetAppId;
    public const int MaxCdnServersToTry = 8;

    private readonly SteamSessionStore _sessionStore;
    private readonly string _outputRootDirectory;

    public SteamResumableDepotDownloadAttempt(
        SteamSessionStore sessionStore,
        string outputRootDirectory)
    {
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputRootDirectory));

        _outputRootDirectory = outputRootDirectory;
    }

    public async Task<SteamResumableDepotDownloadResult> RunAsync(
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
                SteamResumableDepotDownloadOutcome.InvalidLocalSession,
                savedSessionFound: true,
                sw.Elapsed,
                $"Saved session could not be read: {ex.GetType().Name}: {ex.Message}");
        }

        if (savedSession is null)
        {
            return EmptyResult(
                SteamResumableDepotDownloadOutcome.NoSavedSession,
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
        var satisfiedChunkCount = 0;
        ulong satisfiedBytes = 0;
        var reusedVerifiedFileCount = 0;
        var reusedChunkCount = 0;
        ulong reusedBytes = 0;
        var newlyDownloadedChunkCount = 0;
        ulong newlyDownloadedBytes = 0;
        var invalidResumeFileCount = 0;
        var invalidResumeChunkCount = 0;
        var cdnAuthTokenRequested = false;
        var cdnAuthTokenReceived = false;
        var resumeStagingFoundAtStart = false;
        var resumeStagingCreated = false;
        var resumeDataPreserved = false;
        var finalDirectoryCommitted = false;
        string? stagingPath = null;
        string? resumeRelativePath = null;
        string? outputRelativePath = null;
        string? currentEndPoint = null;
        string? returnedSteamId64 = null;
        var outcome = SteamResumableDepotDownloadOutcome.Failed;
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
                outcome = SteamResumableDepotDownloadOutcome.SessionRejected;
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
                outcome = SteamResumableDepotDownloadOutcome.IdentityMismatch;
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
                outcome = SteamResumableDepotDownloadOutcome.OwnershipNotProven;
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
                outcome = SteamResumableDepotDownloadOutcome.PicsAccessTokenDenied;
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
                outcome = SteamResumableDepotDownloadOutcome.ProductInfoUnavailable;
                error = $"PICS did not return app info for App ID {TargetAppId}.";
                return BuildResult();
            }

            if (picsMissingToken)
            {
                outcome = SteamResumableDepotDownloadOutcome.MissingPicsToken;
                error = $"PICS reported that App ID {TargetAppId} still requires an access token.";
                return BuildResult();
            }

            selectedDepot = SteamDepotDownloadPlanner.SelectDepot(depots, TargetAppId);
            if (selectedDepot is null)
            {
                outcome = SteamResumableDepotDownloadOutcome.NoSuitableDepot;
                error = "No direct depot with a visible public manifest was available for the minimal Step 11 depot download.";
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
                outcome = SteamResumableDepotDownloadOutcome.DepotKeyDenied;
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
                outcome = SteamResumableDepotDownloadOutcome.ManifestRequestCodeUnavailable;
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
                outcome = SteamResumableDepotDownloadOutcome.NoCdnServers;
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
                outcome = SteamResumableDepotDownloadOutcome.ManifestDownloadFailed;
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
                outcome = SteamResumableDepotDownloadOutcome.InvalidManifest;
                error = $"Selected Steam manifest cannot be safely materialized in Step 11: {ex.Message}";
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
                outcome = SteamResumableDepotDownloadOutcome.OutputAlreadyExists;
                error = "The manifest-specific Step 11 final directory already exists. Step 11 does not implement update/repair/overwrite behavior.";
                return BuildResult();
            }

            resumeRelativePath = BuildResumeRelativePath(selectedDepot);
            stagingPath = ResolveOutputPath(resumeRelativePath);
            resumeStagingFoundAtStart = Directory.Exists(stagingPath);

            try
            {
                Directory.CreateDirectory(stagingPath);
                resumeStagingCreated = Directory.Exists(stagingPath);
                if (!resumeStagingCreated)
                    throw new IOException("The Step 11 deterministic resume staging directory was not created.");

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
                outcome = SteamResumableDepotDownloadOutcome.FileWriteFailed;
                error = $"Could not prepare the Step 11 resume staging tree: {ex.GetType().Name}: {ex.Message}";
                return PreserveResult();
            }

            foreach (var file in plan.Files)
            {
                token.ThrowIfCancellationRequested();
                var normalized = SteamSingleFileTargetSelector.NormalizeRelativePath(file.FileName);
                var stagedFilePath = ResolveChildPath(stagingPath, normalized);
                var stagedParent = Path.GetDirectoryName(stagedFilePath)
                    ?? throw new InvalidOperationException("Could not resolve staged file parent directory.");
                Directory.CreateDirectory(stagedParent);

                var partPath = stagedFilePath + ".step11.part";

                // A fully staged file from an interrupted run is reusable only after
                // re-proving its complete Steam manifest SHA-1.
                if (File.Exists(stagedFilePath))
                {
                    if (await FileMatchesManifestAsync(stagedFilePath, file, token).ConfigureAwait(false))
                    {
                        if (File.Exists(partPath))
                            File.Delete(partPath);

                        completedFileCount++;
                        verifiedFileCount++;
                        reusedVerifiedFileCount++;
                        reusedChunkCount += file.Chunks.Count;
                        reusedBytes += file.TotalSize;
                        satisfiedChunkCount += file.Chunks.Count;
                        satisfiedBytes += file.TotalSize;

                        progress?.Report(new SteamDepotDownloadProgress(
                            SteamDepotDownloadPhase.Resuming,
                            completedFileCount,
                            plannedFileCount,
                            satisfiedChunkCount,
                            plannedChunkCount,
                            satisfiedBytes,
                            plannedBytes,
                            file.FileName));
                        continue;
                    }

                    invalidResumeFileCount++;
                    File.Delete(stagedFilePath);
                }

                var validChunkOffsets = new HashSet<ulong>();
                if (File.Exists(partPath))
                {
                    try
                    {
                        var info = new FileInfo(partPath);
                        if (info.Length != checked((long)file.TotalSize))
                        {
                            invalidResumeFileCount++;
                            File.Delete(partPath);
                        }
                        else if (file.TotalSize > 0)
                        {
                            await using var existingPart = new FileStream(
                                partPath,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.Read,
                                bufferSize: 128 * 1024,
                                options: FileOptions.Asynchronous | FileOptions.RandomAccess);

                            foreach (var chunk in file.Chunks.OrderBy(chunk => chunk.Offset))
                            {
                                token.ThrowIfCancellationRequested();
                                if (await ChunkMatchesManifestAsync(existingPart, chunk, token).ConfigureAwait(false))
                                {
                                    validChunkOffsets.Add(chunk.Offset);
                                    reusedChunkCount++;
                                    reusedBytes += chunk.UncompressedLength;
                                    satisfiedChunkCount++;
                                    satisfiedBytes += chunk.UncompressedLength;
                                }
                                else
                                {
                                    invalidResumeChunkCount++;
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OverflowException)
                    {
                        invalidResumeFileCount++;
                        try { File.Delete(partPath); } catch { }
                        validChunkOffsets.Clear();
                    }
                }

                progress?.Report(new SteamDepotDownloadProgress(
                    SteamDepotDownloadPhase.Resuming,
                    completedFileCount,
                    plannedFileCount,
                    satisfiedChunkCount,
                    plannedChunkCount,
                    satisfiedBytes,
                    plannedBytes,
                    file.FileName));

                try
                {
                    if (!File.Exists(partPath))
                    {
                        await using var freshPart = new FileStream(
                            partPath,
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.None,
                            bufferSize: 128 * 1024,
                            options: FileOptions.Asynchronous);
                        freshPart.SetLength(checked((long)file.TotalSize));
                        await freshPart.FlushAsync(token).ConfigureAwait(false);
                    }

                    await using (var output = new FileStream(
                        partPath,
                        FileMode.Open,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 128 * 1024,
                        options: FileOptions.Asynchronous | FileOptions.RandomAccess))
                    {
                        foreach (var chunk in file.Chunks.OrderBy(chunk => chunk.Offset))
                        {
                            token.ThrowIfCancellationRequested();
                            if (validChunkOffsets.Contains(chunk.Offset))
                                continue;

                            var chunkBuffer = ArrayPool<byte>.Shared.Rent(checked((int)chunk.UncompressedLength));
                            try
                            {
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
                                    outcome = SteamResumableDepotDownloadOutcome.FileDownloadFailed;
                                    error = $"Chunk for '{file.FileName}' did not return its expected uncompressed byte count. Resume staging is preserved.";
                                    return PreserveResult();
                                }

                                output.Position = checked((long)chunk.Offset);
                                await output.WriteAsync(chunkBuffer.AsMemory(0, written), token)
                                    .ConfigureAwait(false);
                                // Flush every completed chunk so an abrupt process termination
                                // has the best chance of leaving checksum-valid resume data.
                                await output.FlushAsync(token).ConfigureAwait(false);

                                newlyDownloadedChunkCount++;
                                newlyDownloadedBytes += (ulong)written;
                                satisfiedChunkCount++;
                                satisfiedBytes += (ulong)written;

                                progress?.Report(new SteamDepotDownloadProgress(
                                    SteamDepotDownloadPhase.Downloading,
                                    completedFileCount,
                                    plannedFileCount,
                                    satisfiedChunkCount,
                                    plannedChunkCount,
                                    satisfiedBytes,
                                    plannedBytes,
                                    file.FileName));
                            }
                            finally
                            {
                                ArrayPool<byte>.Shared.Return(chunkBuffer, clearArray: true);
                            }
                        }

                        await output.FlushAsync(token).ConfigureAwait(false);
                    }

                    if (new FileInfo(partPath).Length != checked((long)file.TotalSize))
                    {
                        outcome = SteamResumableDepotDownloadOutcome.FileWriteFailed;
                        error = $"Resumable staged file size did not match the Steam manifest: {file.FileName}";
                        return PreserveResult();
                    }

                    progress?.Report(new SteamDepotDownloadProgress(
                        SteamDepotDownloadPhase.Verifying,
                        completedFileCount,
                        plannedFileCount,
                        satisfiedChunkCount,
                        plannedChunkCount,
                        satisfiedBytes,
                        plannedBytes,
                        file.FileName));

                    if (!await FileMatchesManifestAsync(partPath, file, token).ConfigureAwait(false))
                    {
                        try { File.Delete(partPath); } catch { }
                        outcome = SteamResumableDepotDownloadOutcome.FileHashMismatch;
                        error = $"SHA-1 mismatch for resumable staged file '{file.FileName}'. That partial file was discarded; other verified resume data remains.";
                        return PreserveResult();
                    }

                    File.Move(partPath, stagedFilePath, overwrite: false);
                    verifiedFileCount++;
                    completedFileCount++;

                    progress?.Report(new SteamDepotDownloadProgress(
                        SteamDepotDownloadPhase.Downloading,
                        completedFileCount,
                        plannedFileCount,
                        satisfiedChunkCount,
                        plannedChunkCount,
                        satisfiedBytes,
                        plannedBytes,
                        file.FileName));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    outcome = SteamResumableDepotDownloadOutcome.FileWriteFailed;
                    error = $"Could not materialize resumable staged file '{file.FileName}': {ex.GetType().Name}: {ex.Message}";
                    return PreserveResult();
                }
            }

            if (completedFileCount != plannedFileCount ||
                verifiedFileCount != plannedFileCount ||
                satisfiedChunkCount != plannedChunkCount ||
                satisfiedBytes != plannedBytes)
            {
                outcome = SteamResumableDepotDownloadOutcome.Failed;
                error = "The Step 11 queue finished with inconsistent file/chunk/byte totals; resume staging was preserved and no final directory was committed.";
                return PreserveResult();
            }

            try
            {
                ValidateCommitTree(stagingPath, plan);
            }
            catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
            {
                outcome = SteamResumableDepotDownloadOutcome.FileWriteFailed;
                error = $"Resume staging contained unexpected data before commit: {ex.Message}";
                return PreserveResult();
            }

            progress?.Report(new SteamDepotDownloadProgress(
                SteamDepotDownloadPhase.Committing,
                completedFileCount,
                plannedFileCount,
                satisfiedChunkCount,
                plannedChunkCount,
                satisfiedBytes,
                plannedBytes,
                CurrentFile: null));

            try
            {
                var finalParent = Path.GetDirectoryName(finalPath)
                    ?? throw new InvalidOperationException("Could not resolve Step 11 final directory parent.");
                Directory.CreateDirectory(finalParent);

                if (Directory.Exists(finalPath) || File.Exists(finalPath))
                    throw new IOException("The Step 11 final manifest directory appeared before commit.");

                Directory.Move(stagingPath, finalPath);
                finalDirectoryCommitted = Directory.Exists(finalPath) && !Directory.Exists(stagingPath);
                if (!finalDirectoryCommitted)
                    throw new IOException("Atomic resume-staging rename did not produce the final directory.");
                stagingPath = null;
                resumeDataPreserved = false;
            }
            catch (Exception ex)
            {
                outcome = SteamResumableDepotDownloadOutcome.CommitFailed;
                error = $"Verified resumed depot could not be atomically committed: {ex.GetType().Name}: {ex.Message}";
                return PreserveResult();
            }

            outcome = SteamResumableDepotDownloadOutcome.Downloaded;
            progress?.Report(new SteamDepotDownloadProgress(
                SteamDepotDownloadPhase.Complete,
                completedFileCount,
                plannedFileCount,
                satisfiedChunkCount,
                plannedChunkCount,
                satisfiedBytes,
                plannedBytes,
                CurrentFile: null));
            return BuildResult();
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outcome = SteamResumableDepotDownloadOutcome.Cancelled;
                error = "Step 11 download interrupted by user. Checksum-valid resume staging is deliberately preserved for the next run.";
            }
            else
            {
                outcome = SteamResumableDepotDownloadOutcome.TimedOut;
                error = $"Step 11 download timed out after {timeout.TotalMinutes:F0} minutes. Checksum-valid resume staging is deliberately preserved for the next run.";
            }

            return PreserveResult();
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return PreserveResult();
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

            if (!finalDirectoryCommitted && !string.IsNullOrWhiteSpace(stagingPath))
                resumeDataPreserved = Directory.Exists(stagingPath);

            sw.Stop();
        }

        SteamResumableDepotDownloadResult PreserveResult()
        {
            resumeDataPreserved = !finalDirectoryCommitted &&
                                  !string.IsNullOrWhiteSpace(stagingPath) &&
                                  Directory.Exists(stagingPath);
            return BuildResult();
        }

        SteamResumableDepotDownloadResult BuildResult() => new(
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
            SatisfiedChunkCount: satisfiedChunkCount,
            SatisfiedBytes: satisfiedBytes,
            ReusedVerifiedFileCount: reusedVerifiedFileCount,
            ReusedChunkCount: reusedChunkCount,
            ReusedBytes: reusedBytes,
            NewlyDownloadedChunkCount: newlyDownloadedChunkCount,
            NewlyDownloadedBytes: newlyDownloadedBytes,
            InvalidResumeFileCount: invalidResumeFileCount,
            InvalidResumeChunkCount: invalidResumeChunkCount,
            CdnAuthTokenRequested: cdnAuthTokenRequested,
            CdnAuthTokenReceived: cdnAuthTokenReceived,
            ResumeStagingFoundAtStart: resumeStagingFoundAtStart,
            ResumeStagingCreated: resumeStagingCreated,
            ResumeDataPreserved: resumeDataPreserved,
            FinalDirectoryCommitted: finalDirectoryCommitted,
            ResumeRelativePath: resumeRelativePath,
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
                    // iOS NSUrlSessionHandler can surface SteamKit's bounded response-body cancellation
                    // as TimeoutException instead of TaskCanceledException. Treat it as an endpoint
                    // timeout and fail over just like the other transient CDN transport failures.
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
                catch (SteamKitWebRequestException)
                {
                    // Try the next bounded server.
                }
            }
            catch (SteamKitWebRequestException)
            {
                // Try the next bounded server.
            }
            catch (TimeoutException) when (!cancellationToken.IsCancellationRequested)
            {
                // On iOS, NSUrlSessionHandler may translate SteamKit's request/body cancellation
                // into TimeoutException. This is a recoverable per-endpoint CDN timeout, not a
                // reason to abandon the resumable depot attempt.
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
                    // iOS NSUrlSessionHandler can surface SteamKit's bounded response-body cancellation
                    // as TimeoutException instead of TaskCanceledException. Treat it as an endpoint
                    // timeout and fail over just like the other transient CDN transport failures.
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
                catch (SteamKitWebRequestException)
                {
                    // Try another server.
                }
            }
            catch (SteamKitWebRequestException)
            {
                // Try another server.
            }
            catch (TimeoutException) when (!cancellationToken.IsCancellationRequested)
            {
                // On iOS, NSUrlSessionHandler may translate SteamKit's request/body cancellation
                // into TimeoutException. This is a recoverable per-endpoint CDN timeout, not a
                // reason to abandon the resumable depot attempt.
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

    private static async Task<bool> FileMatchesManifestAsync(
        string path,
        DepotManifest.FileData file,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            return false;

        var info = new FileInfo(path);
        if (info.Length != checked((long)file.TotalSize))
            return false;

        byte[] actualHash;
        await using (var input = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 128 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan))
        using (var sha1 = SHA1.Create())
        {
            actualHash = await sha1.ComputeHashAsync(input, cancellationToken).ConfigureAwait(false);
        }

        return CryptographicOperations.FixedTimeEquals(actualHash, file.FileHash);
    }

    private static async Task<bool> ChunkMatchesManifestAsync(
        FileStream input,
        DepotManifest.ChunkData chunk,
        CancellationToken cancellationToken)
    {
        if (chunk.UncompressedLength > int.MaxValue || chunk.Offset > long.MaxValue)
            return false;

        var end = checked(chunk.Offset + chunk.UncompressedLength);
        if (end > (ulong)input.Length)
            return false;

        input.Position = checked((long)chunk.Offset);
        var checksum = await SteamDepotResumeValidation.ComputeAdler32Async(
                input,
                checked((int)chunk.UncompressedLength),
                cancellationToken)
            .ConfigureAwait(false);
        return checksum == chunk.Checksum;
    }

    private static void ValidateCommitTree(
        string stagingRoot,
        SteamDepotDownloadPlan plan)
    {
        var root = Path.GetFullPath(stagingRoot);
        var expectedFiles = new HashSet<string>(StringComparer.Ordinal);
        var expectedDirectories = new HashSet<string>(StringComparer.Ordinal) { root };

        foreach (var directory in plan.Directories)
        {
            var full = ResolveChildPath(root, directory);
            AddDirectoryAndParents(full, root, expectedDirectories);
        }

        foreach (var file in plan.Files)
        {
            var normalized = SteamSingleFileTargetSelector.NormalizeRelativePath(file.FileName);
            var full = ResolveChildPath(root, normalized);
            expectedFiles.Add(full);
            var parent = Path.GetDirectoryName(full);
            if (!string.IsNullOrWhiteSpace(parent))
                AddDirectoryAndParents(parent, root, expectedDirectories);
        }

        foreach (var actualFile in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var full = Path.GetFullPath(actualFile);
            if (!expectedFiles.Contains(full))
                throw new InvalidDataException($"Unexpected resume-staging file: {Path.GetRelativePath(root, full)}");
        }

        foreach (var expectedFile in expectedFiles)
        {
            if (!File.Exists(expectedFile))
                throw new InvalidDataException($"Expected verified staged file is missing: {Path.GetRelativePath(root, expectedFile)}");
        }

        foreach (var actualDirectory in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
        {
            var full = Path.GetFullPath(actualDirectory);
            if (!expectedDirectories.Contains(full))
                throw new InvalidDataException($"Unexpected resume-staging directory: {Path.GetRelativePath(root, full)}");
        }
    }

    private static void AddDirectoryAndParents(
        string directory,
        string root,
        ISet<string> destinations)
    {
        var current = Path.GetFullPath(directory);
        while (true)
        {
            destinations.Add(current);
            if (string.Equals(current, root, StringComparison.Ordinal))
                break;

            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrWhiteSpace(parent) || !parent.StartsWith(root, StringComparison.Ordinal))
                throw new InvalidDataException("Resume directory parent escaped the staging root.");
            current = parent;
        }
    }

    private static string BuildResumeRelativePath(SteamSingleFileDepotTarget target) =>
        Path.Combine(
            "Step11-ResumableDepot",
            ".resume",
            $"{target.DepotId}-{target.ManifestId}");

    private static string BuildOutputRelativePath(SteamSingleFileDepotTarget target) =>
        Path.Combine(
            "Step11-ResumableDepot",
            "complete",
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
            throw new InvalidOperationException("Resolved Step 11 output escaped the configured app data directory.");

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
            throw new InvalidDataException("Resolved Steam manifest path escaped the Step 11 staging directory.");

        return candidate;
    }

    private static SteamResumableDepotDownloadResult EmptyResult(
        SteamResumableDepotDownloadOutcome outcome,
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
            SatisfiedChunkCount: 0,
            SatisfiedBytes: 0,
            ReusedVerifiedFileCount: 0,
            ReusedChunkCount: 0,
            ReusedBytes: 0,
            NewlyDownloadedChunkCount: 0,
            NewlyDownloadedBytes: 0,
            InvalidResumeFileCount: 0,
            InvalidResumeChunkCount: 0,
            CdnAuthTokenRequested: false,
            CdnAuthTokenReceived: false,
            ResumeStagingFoundAtStart: false,
            ResumeStagingCreated: false,
            ResumeDataPreserved: false,
            FinalDirectoryCommitted: false,
            ResumeRelativePath: null,
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
