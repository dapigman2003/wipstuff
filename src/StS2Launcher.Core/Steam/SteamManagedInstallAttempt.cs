using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace StS2Launcher.Core;

/// <summary>
/// Step 12 boundary: manage exactly one selected direct public depot as a stable
/// local install. Current Steam metadata is discovered first. A verified Step 11
/// manifest-specific depot is used as the content source. Install/update/repair
/// always build a complete staging tree, verify it, then atomically replace the
/// managed install while preserving the prior good install until commit.
///
/// No compatibility inspection, Godot launch/runtime, Cloud, Workshop, or
/// multi-depot app composition is introduced here.
/// </summary>
public sealed class SteamManagedInstallAttempt
{
    public const uint TargetAppId = SteamOwnershipVerificationAttempt.TargetAppId;

    private readonly SteamSessionStore _sessionStore;
    private readonly string _outputRootDirectory;

    public SteamManagedInstallAttempt(SteamSessionStore sessionStore, string outputRootDirectory)
    {
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputRootDirectory));
        _outputRootDirectory = outputRootDirectory;
    }

    public async Task<SteamManagedInstallResult> RunAsync(
        TimeSpan timeout,
        IProgress<SteamManagedInstallProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        var sw = Stopwatch.StartNew();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        var token = cts.Token;

        SteamManagedInstallState stateBefore = SteamManagedInstallState.Unknown;
        SteamManagedInstallState stateAfter = SteamManagedInstallState.Unknown;
        SteamManagedInstallAction action = SteamManagedInstallAction.None;
        SteamManagedInstallOutcome outcome = SteamManagedInstallOutcome.Failed;
        uint? depotId = null;
        ulong? currentManifestId = null;
        ulong? installedManifestBefore = null;
        ulong? installedManifestAfter = null;
        string? branch = null;
        int plannedFiles = 0;
        ulong plannedBytes = 0;
        int verifiedSourceFiles = 0;
        ulong verifiedSourceBytes = 0;
        bool sourceCacheReverified = false;
        ulong sourceNewlyDownloadedBytes = 0;
        int reusedLocalFiles = 0;
        ulong reusedLocalBytes = 0;
        int replacedFiles = 0;
        ulong replacedBytes = 0;
        bool previousPreserved = true;
        bool atomicCommit = false;
        bool rollbackRestored = false;
        bool stagingAbsent = true;
        bool backupAbsent = true;
        string? managedRelative = null;
        string? cacheRelative = null;
        string? stagingPath = null;
        string? backupPath = null;
        string? error = null;

        try
        {
            progress?.Report(new SteamManagedInstallProgress(
                SteamManagedInstallPhase.Discovering, 0, 0, 0, 0, null,
                "Discovering the current public depot manifest…"));

            var discovery = await new SteamContentDiscoveryAttempt(_sessionStore)
                .RunAsync(Remaining(timeout, sw), cancellationToken: token)
                .ConfigureAwait(false);

            if (!discovery.DiscoveryProven)
            {
                outcome = SteamManagedInstallOutcome.DiscoveryFailed;
                error = discovery.Error ?? discovery.Summary;
                return FinishResult();
            }

            var target = SteamDepotDownloadPlanner.SelectDepot(discovery.Depots, TargetAppId);
            if (target is null)
            {
                outcome = SteamManagedInstallOutcome.NoSuitableDepot;
                error = "No direct public depot with a visible manifest is available.";
                return FinishResult();
            }

            depotId = target.DepotId;
            currentManifestId = target.ManifestId;
            branch = target.Branch;
            managedRelative = BuildManagedRelativePath(target.DepotId);
            var managedPath = ResolveOutputPath(managedRelative);
            stagingPath = ResolveOutputPath(BuildStagingRelativePath(target.DepotId, target.ManifestId));
            backupPath = ResolveOutputPath(BuildBackupRelativePath(target.DepotId));

            RecoverInterruptedCommit(managedPath, backupPath);

            progress?.Report(new SteamManagedInstallProgress(
                SteamManagedInstallPhase.Inspecting, 0, 0, 0, 0, null,
                "Inspecting managed-install receipt and local files…"));

            var installedReceipt = await TryReadReceiptAsync(managedPath, token).ConfigureAwait(false);
            installedManifestBefore = installedReceipt?.ManifestId;
            stateBefore = await DetermineStateAsync(
                    managedPath,
                    installedReceipt,
                    target,
                    token)
                .ConfigureAwait(false);

            if (stateBefore == SteamManagedInstallState.UpToDate)
            {
                outcome = SteamManagedInstallOutcome.UpToDate;
                stateAfter = SteamManagedInstallState.UpToDate;
                installedManifestAfter = installedReceipt?.ManifestId;
                if (installedReceipt is not null)
                {
                    plannedFiles = installedReceipt.Files.Count;
                    plannedBytes = SumBytes(installedReceipt.Files);
                }
                return FinishResult();
            }

            action = stateBefore switch
            {
                SteamManagedInstallState.NotInstalled => SteamManagedInstallAction.Install,
                SteamManagedInstallState.UpdateAvailable => SteamManagedInstallAction.Update,
                _ => SteamManagedInstallAction.Repair,
            };

            progress?.Report(new SteamManagedInstallProgress(
                SteamManagedInstallPhase.Acquiring, 0, 0, 0, 0, null,
                $"Acquiring a fully verified Step 11 source for {action.ToString().ToLowerInvariant()}…"));

            var acquisition = await AcquireVerifiedSourceAsync(
                    target,
                    Remaining(timeout, sw),
                    progress,
                    token)
                .ConfigureAwait(false);

            if (acquisition.Result is not null)
            {
                cacheRelative = acquisition.Result.OutputRelativePath;
                plannedFiles = acquisition.Result.PlannedFileCount;
                plannedBytes = acquisition.Result.PlannedBytes;
                verifiedSourceFiles = acquisition.Result.VerifiedFileCount;
                verifiedSourceBytes = acquisition.Result.ExistingFinalVerifiedAgainstManifest
                    ? acquisition.Result.PlannedBytes
                    : acquisition.Result.SatisfiedBytes;
                sourceCacheReverified = acquisition.Result.ExistingFinalVerifiedAgainstManifest;
                sourceNewlyDownloadedBytes = acquisition.Result.NewlyDownloadedBytes;
            }

            if (!acquisition.Success || acquisition.Result is null || acquisition.SourcePath is null)
            {
                outcome = acquisition.Result?.Outcome switch
                {
                    SteamResumableDepotDownloadOutcome.Cancelled => SteamManagedInstallOutcome.Cancelled,
                    SteamResumableDepotDownloadOutcome.TimedOut => SteamManagedInstallOutcome.TimedOut,
                    _ => SteamManagedInstallOutcome.AcquisitionFailed,
                };
                error = acquisition.Error;
                return FinishResult();
            }

            progress?.Report(new SteamManagedInstallProgress(
                SteamManagedInstallPhase.VerifyingSource, 0, 0, 0, 0, null,
                "Hashing the verified source depot and creating a non-secret local receipt…"));

            var sourceReceipt = await BuildReceiptAsync(
                    acquisition.SourcePath,
                    target,
                    progress,
                    token)
                .ConfigureAwait(false);

            plannedFiles = sourceReceipt.Files.Count;
            plannedBytes = SumBytes(sourceReceipt.Files);
            verifiedSourceFiles = plannedFiles;
            verifiedSourceBytes = plannedBytes;

            if (!await VerifyTreeAgainstReceiptAsync(acquisition.SourcePath, sourceReceipt, allowReceiptFile: false, token)
                    .ConfigureAwait(false))
            {
                outcome = SteamManagedInstallOutcome.SourceValidationFailed;
                error = "The Step 11 source tree changed while Step 12 was preparing it.";
                return FinishResult();
            }

            CleanupDirectory(stagingPath);
            Directory.CreateDirectory(stagingPath);

            var existingReceiptForReuse = installedReceipt is not null && Directory.Exists(managedPath)
                ? installedReceipt
                : null;
            var existingByPath = existingReceiptForReuse?.Files
                .ToDictionary(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, SteamManagedInstallFile>(StringComparer.OrdinalIgnoreCase);

            var completed = 0;
            ulong completedBytes = 0;
            foreach (var sourceFile in sourceReceipt.Files)
            {
                token.ThrowIfCancellationRequested();
                var sourceFilePath = ResolveChildPath(acquisition.SourcePath, sourceFile.RelativePath);
                var stagedFilePath = ResolveChildPath(stagingPath, sourceFile.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(stagedFilePath)!);

                var reused = false;
                if (existingByPath.TryGetValue(sourceFile.RelativePath, out var priorFile) &&
                    SameFileIdentity(priorFile, sourceFile))
                {
                    var installedFilePath = ResolveChildPath(managedPath, sourceFile.RelativePath);
                    if (await FileMatchesReceiptAsync(installedFilePath, sourceFile, token).ConfigureAwait(false))
                    {
                        File.Copy(installedFilePath, stagedFilePath, overwrite: false);
                        reusedLocalFiles++;
                        reusedLocalBytes += checked((ulong)sourceFile.Length);
                        reused = true;
                    }
                }

                if (!reused)
                {
                    File.Copy(sourceFilePath, stagedFilePath, overwrite: false);
                    replacedFiles++;
                    replacedBytes += checked((ulong)sourceFile.Length);
                }

                if (!await FileMatchesReceiptAsync(stagedFilePath, sourceFile, token).ConfigureAwait(false))
                    throw new InvalidDataException($"Staged file failed receipt verification: {sourceFile.RelativePath}");

                completed++;
                completedBytes += checked((ulong)sourceFile.Length);
                progress?.Report(new SteamManagedInstallProgress(
                    SteamManagedInstallPhase.Staging,
                    completed,
                    plannedFiles,
                    completedBytes,
                    plannedBytes,
                    sourceFile.RelativePath,
                    reused ? "Reused locally verified file" : "Staged verified source file"));
            }

            await WriteReceiptAsync(stagingPath, sourceReceipt, token).ConfigureAwait(false);
            if (!await VerifyTreeAgainstReceiptAsync(stagingPath, sourceReceipt, allowReceiptFile: true, token)
                    .ConfigureAwait(false))
            {
                outcome = SteamManagedInstallOutcome.StagingFailed;
                error = "The complete Step 12 staging tree failed final receipt verification.";
                return FinishResult();
            }

            progress?.Report(new SteamManagedInstallProgress(
                SteamManagedInstallPhase.Committing,
                plannedFiles,
                plannedFiles,
                plannedBytes,
                plannedBytes,
                null,
                "Atomically replacing the stable managed install…"));

            var hadExistingInstall = Directory.Exists(managedPath);
            if (Directory.Exists(backupPath))
                Directory.Delete(backupPath, recursive: true);

            try
            {
                if (hadExistingInstall)
                    Directory.Move(managedPath, backupPath);

                try
                {
                    Directory.Move(stagingPath, managedPath);
                    atomicCommit = true;
                }
                catch
                {
                    if (hadExistingInstall && Directory.Exists(backupPath) && !Directory.Exists(managedPath))
                    {
                        Directory.Move(backupPath, managedPath);
                        rollbackRestored = true;
                    }
                    throw;
                }

                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                outcome = SteamManagedInstallOutcome.CommitFailed;
                error = $"Managed-install replacement failed: {ex.GetType().Name}: {ex.Message}";
                return FinishResult();
            }

            installedManifestAfter = sourceReceipt.ManifestId;
            stateAfter = await DetermineStateAsync(managedPath, sourceReceipt, target, token).ConfigureAwait(false);
            if (stateAfter != SteamManagedInstallState.UpToDate)
            {
                outcome = SteamManagedInstallOutcome.CommitFailed;
                error = "The committed managed install did not verify as UpToDate.";
                return FinishResult();
            }

            outcome = action switch
            {
                SteamManagedInstallAction.Install => SteamManagedInstallOutcome.Installed,
                SteamManagedInstallAction.Update => SteamManagedInstallOutcome.Updated,
                SteamManagedInstallAction.Repair => SteamManagedInstallOutcome.Repaired,
                _ => SteamManagedInstallOutcome.UpToDate,
            };

            progress?.Report(new SteamManagedInstallProgress(
                SteamManagedInstallPhase.Complete,
                plannedFiles,
                plannedFiles,
                plannedBytes,
                plannedBytes,
                null,
                outcome.ToString()));

            return FinishResult();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            outcome = SteamManagedInstallOutcome.Cancelled;
            error = "Step 12 was cancelled; the prior managed install was preserved.";
            return FinishResult();
        }
        catch (OperationCanceledException)
        {
            outcome = SteamManagedInstallOutcome.TimedOut;
            error = "Step 12 timed out; the prior managed install was preserved.";
            return FinishResult();
        }
        catch (Exception ex)
        {
            outcome = SteamManagedInstallOutcome.Failed;
            error = $"{ex.GetType().Name}: {ex.Message}";
            return FinishResult();
        }

        SteamManagedInstallResult FinishResult()
        {
            if (stagingPath is not null && Directory.Exists(stagingPath))
            {
                try
                {
                    CleanupDirectory(stagingPath);
                }
                catch (Exception cleanupEx) when (cleanupEx is IOException or UnauthorizedAccessException)
                {
                    AppendError($"Staging cleanup failed: {cleanupEx.GetType().Name}: {cleanupEx.Message}");
                }
            }

            if (backupPath is not null && Directory.Exists(backupPath) && managedRelative is not null)
            {
                try
                {
                    var managedPath = ResolveOutputPath(managedRelative);
                    if (!Directory.Exists(managedPath))
                    {
                        Directory.Move(backupPath, managedPath);
                        rollbackRestored = true;
                    }
                    else
                    {
                        CleanupDirectory(backupPath);
                    }
                }
                catch (Exception cleanupEx) when (cleanupEx is IOException or UnauthorizedAccessException)
                {
                    AppendError($"Backup cleanup/restore failed: {cleanupEx.GetType().Name}: {cleanupEx.Message}");
                }
            }

            stagingAbsent = stagingPath is null || !Directory.Exists(stagingPath);
            backupAbsent = backupPath is null || !Directory.Exists(backupPath);
            return BuildResult();
        }

        void AppendError(string message)
        {
            error = string.IsNullOrWhiteSpace(error) ? message : $"{error} | {message}";
        }

        SteamManagedInstallResult BuildResult()
        {
            if (stateAfter == SteamManagedInstallState.Unknown && managedRelative is not null && depotId is not null && currentManifestId is not null)
            {
                var managedPath = ResolveOutputPath(managedRelative);
                var receipt = TryReadReceipt(managedPath);
                installedManifestAfter ??= receipt?.ManifestId;
                stateAfter = receipt is null || !Directory.Exists(managedPath)
                    ? SteamManagedInstallState.NotInstalled
                    : receipt.ManifestId == currentManifestId
                        ? SteamManagedInstallState.RepairNeeded
                        : SteamManagedInstallState.UpdateAvailable;
            }

            return new SteamManagedInstallResult(
                outcome,
                stateBefore,
                stateAfter,
                action,
                TargetAppId,
                depotId,
                currentManifestId,
                installedManifestBefore,
                installedManifestAfter,
                branch,
                plannedFiles,
                plannedBytes,
                verifiedSourceFiles,
                verifiedSourceBytes,
                sourceCacheReverified,
                sourceNewlyDownloadedBytes,
                reusedLocalFiles,
                reusedLocalBytes,
                replacedFiles,
                replacedBytes,
                previousPreserved,
                atomicCommit,
                rollbackRestored,
                stagingAbsent,
                backupAbsent,
                managedRelative,
                cacheRelative,
                sw.Elapsed,
                error);
        }
    }

    public async Task<string> PrepareRepairTestAsync(CancellationToken cancellationToken = default)
    {
        var root = ResolveOutputPath("Step12-ManagedInstall");
        if (!Directory.Exists(root))
            throw new InvalidOperationException("No Step 12 managed install exists yet.");

        foreach (var depotDirectory in Directory.EnumerateDirectories(root, "Depot-*", SearchOption.TopDirectoryOnly))
        {
            var receipt = await TryReadReceiptAsync(depotDirectory, cancellationToken).ConfigureAwait(false);
            if (receipt is null)
                continue;

            var target = receipt.Files.FirstOrDefault(file => file.Length > 0) ?? receipt.Files.FirstOrDefault();
            if (target is null)
                throw new InvalidOperationException("Managed install contains no regular files to mutate.");

            var path = ResolveChildPath(depotDirectory, target.RelativePath);
            if (target.Length > 0)
            {
                await using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                var original = stream.ReadByte();
                if (original < 0)
                    throw new IOException("Could not read repair-test byte.");
                stream.Position = 0;
                stream.WriteByte((byte)(original ^ 0x5A));
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await File.WriteAllBytesAsync(path, [0x5A], cancellationToken).ConfigureAwait(false);
            }

            return target.RelativePath;
        }

        throw new InvalidOperationException("No readable Step 12 managed-install receipt was found.");
    }

    public async Task<ulong> PrepareUpdateStateTestAsync(CancellationToken cancellationToken = default)
    {
        var root = ResolveOutputPath("Step12-ManagedInstall");
        if (!Directory.Exists(root))
            throw new InvalidOperationException("No Step 12 managed install exists yet.");

        foreach (var depotDirectory in Directory.EnumerateDirectories(root, "Depot-*", SearchOption.TopDirectoryOnly))
        {
            var receipt = await TryReadReceiptAsync(depotDirectory, cancellationToken).ConfigureAwait(false);
            if (receipt is null)
                continue;

            var altered = CreateSyntheticUpdateReceipt(receipt);
            await WriteReceiptAsync(depotDirectory, altered, cancellationToken).ConfigureAwait(false);
            return altered.ManifestId;
        }

        throw new InvalidOperationException("No readable Step 12 managed-install receipt was found.");
    }

    public static SteamManagedInstallReceipt CreateSyntheticUpdateReceipt(SteamManagedInstallReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        if (receipt.Files.Count == 0)
            throw new InvalidOperationException("Managed install contains no regular files for the update-state test.");

        var simulatedManifest = receipt.ManifestId == ulong.MaxValue ? receipt.ManifestId - 1 : receipt.ManifestId + 1;
        var files = receipt.Files.ToArray();
        var index = files
            .Select((file, index) => (file, index))
            .Where(item => item.file.Length > 0)
            .OrderBy(item => item.file.Length)
            .Select(item => item.index)
            .DefaultIfEmpty(0)
            .First();

        var original = files[index];
        var syntheticSha1 = string.Equals(original.Sha1Hex, new string('0', 40), StringComparison.OrdinalIgnoreCase)
            ? new string('1', 40)
            : new string('0', 40);
        files[index] = original with { Sha1Hex = syntheticSha1 };

        return receipt with
        {
            ManifestId = simulatedManifest,
            CreatedUtc = DateTimeOffset.UtcNow,
            Files = files,
        };
    }

    public static SteamManagedInstallState DetermineStateFromReceipt(
        bool directoryExists,
        SteamManagedInstallReceipt? receipt,
        uint depotId,
        ulong currentManifestId,
        bool filesVerify)
    {
        if (!directoryExists)
            return SteamManagedInstallState.NotInstalled;
        if (!IsReceiptStructurallyValid(receipt) ||
            receipt!.SchemaVersion != SteamManagedInstallReceipt.CurrentSchemaVersion ||
            receipt.AppId != TargetAppId ||
            receipt.DepotId != depotId)
        {
            return SteamManagedInstallState.RepairNeeded;
        }
        if (receipt.ManifestId != currentManifestId)
            return SteamManagedInstallState.UpdateAvailable;
        return filesVerify ? SteamManagedInstallState.UpToDate : SteamManagedInstallState.RepairNeeded;
    }

    private async Task<SteamManagedInstallState> DetermineStateAsync(
        string managedPath,
        SteamManagedInstallReceipt? receipt,
        SteamSingleFileDepotTarget current,
        CancellationToken token)
    {
        var exists = Directory.Exists(managedPath);
        if (!exists)
            return SteamManagedInstallState.NotInstalled;
        if (!IsReceiptStructurallyValid(receipt) ||
            receipt!.SchemaVersion != SteamManagedInstallReceipt.CurrentSchemaVersion ||
            receipt.AppId != TargetAppId ||
            receipt.DepotId != current.DepotId ||
            !string.Equals(receipt.Branch, current.Branch, StringComparison.OrdinalIgnoreCase))
        {
            return SteamManagedInstallState.RepairNeeded;
        }

        // A mismatched manifest is already sufficient to prove UpdateAvailable.
        // Do not spend another full-tree SHA-1 pass proving the old/stale receipt
        // before entering the update path. Current source and staged output are
        // independently verified later against the real current manifest.
        if (receipt.ManifestId != current.ManifestId)
            return SteamManagedInstallState.UpdateAvailable;

        var filesVerify = await VerifyTreeAgainstReceiptAsync(
                managedPath,
                receipt,
                allowReceiptFile: true,
                token)
            .ConfigureAwait(false);
        return filesVerify ? SteamManagedInstallState.UpToDate : SteamManagedInstallState.RepairNeeded;
    }

    private async Task<(bool Success, SteamResumableDepotDownloadResult? Result, string? SourcePath, string? Error)> AcquireVerifiedSourceAsync(
        SteamSingleFileDepotTarget target,
        TimeSpan timeout,
        IProgress<SteamManagedInstallProgress>? progress,
        CancellationToken token)
    {
        var downloader = new SteamResumableDepotDownloadAttempt(_sessionStore, _outputRootDirectory);
        IProgress<SteamDepotDownloadProgress>? sourceProgress = progress is null
            ? null
            : new Progress<SteamDepotDownloadProgress>(value => progress.Report(new SteamManagedInstallProgress(
                SteamManagedInstallPhase.Acquiring,
                value.CompletedFiles,
                value.TotalFiles,
                value.CompletedBytes,
                value.TotalBytes,
                value.CurrentFile,
                value.Phase == SteamDepotDownloadPhase.Verifying
                    ? "Revalidating the existing Step 11 cache against the current Steam manifest…"
                    : $"Step 11 source acquisition: {value.Phase.ToString().ToLowerInvariant()}…")));

        var result = await downloader.RunAsync(timeout, sourceProgress, token).ConfigureAwait(false);

        if (result.SelectedDepotId is not null &&
            (result.SelectedDepotId != target.DepotId || result.SelectedManifestId != target.ManifestId))
        {
            return (false, result, null, "Steam public-depot metadata changed between Step 12 discovery and Step 11 acquisition; retry the manager so it can rediscover a consistent target.");
        }

        if (result.Outcome == SteamResumableDepotDownloadOutcome.Downloaded && result.OutputRelativePath is not null)
            return (true, result, ResolveOutputPath(result.OutputRelativePath), null);

        if (result.Outcome != SteamResumableDepotDownloadOutcome.OutputAlreadyExists || result.OutputRelativePath is null)
            return (false, result, null, result.Error ?? result.Summary);

        var existingPath = ResolveOutputPath(result.OutputRelativePath);
        if (result.ExistingFinalVerifiedAgainstManifest)
            return (true, result, existingPath, null);

        // Step 12.3 no longer requires the Step 12 install receipt to vouch for a
        // completed Step 11 cache. Step 11 has already downloaded the current Steam
        // manifest and directly re-hashed the exact final tree against it. Only a
        // cache that fails that independent manifest check is discarded/reacquired.
        CleanupDirectory(existingPath);
        result = await downloader.RunAsync(timeout, sourceProgress, token).ConfigureAwait(false);
        if (result.Outcome == SteamResumableDepotDownloadOutcome.Downloaded && result.OutputRelativePath is not null)
        {
            if (result.SelectedDepotId != target.DepotId || result.SelectedManifestId != target.ManifestId)
                return (false, result, null, "Steam public-depot metadata changed while the Step 12 source was being reacquired; retry the manager.");
            return (true, result, ResolveOutputPath(result.OutputRelativePath), null);
        }

        return (false, result, null, result.Error ?? result.Summary);
    }

    private static async Task<SteamManagedInstallReceipt> BuildReceiptAsync(
        string sourceRoot,
        SteamSingleFileDepotTarget target,
        IProgress<SteamManagedInstallProgress>? progress,
        CancellationToken token)
    {
        var sourcePaths = Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        ulong totalBytes = 0;
        foreach (var path in sourcePaths)
            totalBytes += checked((ulong)new FileInfo(path).Length);

        var files = new List<SteamManagedInstallFile>(sourcePaths.Length);
        var completed = 0;
        ulong completedBytes = 0;
        foreach (var path in sourcePaths)
        {
            token.ThrowIfCancellationRequested();
            var relative = Path.GetRelativePath(sourceRoot, path).Replace('\\', '/');
            if (!SteamSingleFileTargetSelector.IsSafeRelativePath(relative))
                throw new InvalidDataException($"Unsafe source-cache path: {relative}");
            if (string.Equals(relative, SteamManagedInstallReceipt.FileName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Source cache unexpectedly contains a Step 12 receipt.");

            var info = new FileInfo(path);
            var sha1 = await ComputeSha1HexAsync(path, token).ConfigureAwait(false);
            files.Add(new SteamManagedInstallFile(relative, info.Length, sha1));
            completed++;
            completedBytes += checked((ulong)info.Length);
            progress?.Report(new SteamManagedInstallProgress(
                SteamManagedInstallPhase.VerifyingSource,
                completed,
                sourcePaths.Length,
                completedBytes,
                totalBytes,
                relative,
                "Hashing verified Step 11 source for the managed-install receipt…"));
        }

        return new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            TargetAppId,
            target.DepotId,
            target.ManifestId,
            target.Branch,
            DateTimeOffset.UtcNow,
            files);
    }

    private static async Task<bool> VerifyTreeAgainstReceiptAsync(
        string root,
        SteamManagedInstallReceipt receipt,
        bool allowReceiptFile,
        CancellationToken token)
    {
        if (!Directory.Exists(root))
            return false;

        var expected = new Dictionary<string, SteamManagedInstallFile>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in receipt.Files)
        {
            if (!SteamSingleFileTargetSelector.IsSafeRelativePath(file.RelativePath) || !expected.TryAdd(file.RelativePath, file))
                return false;
        }

        var actual = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            token.ThrowIfCancellationRequested();
            var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
            if (allowReceiptFile && string.Equals(relative, SteamManagedInstallReceipt.FileName, StringComparison.OrdinalIgnoreCase))
                continue;
            if (!actual.Add(relative) || !expected.TryGetValue(relative, out var expectedFile))
                return false;
            if (!await FileMatchesReceiptAsync(path, expectedFile, token).ConfigureAwait(false))
                return false;
        }

        return actual.Count == expected.Count;
    }

    private static async Task<bool> FileMatchesReceiptAsync(
        string path,
        SteamManagedInstallFile expected,
        CancellationToken token)
    {
        if (!File.Exists(path))
            return false;
        var info = new FileInfo(path);
        if (info.Length != expected.Length)
            return false;
        var actual = await ComputeSha1HexAsync(path, token).ConfigureAwait(false);
        return string.Equals(actual, expected.Sha1Hex, StringComparison.OrdinalIgnoreCase);
    }

    private static bool SameFileIdentity(SteamManagedInstallFile a, SteamManagedInstallFile b) =>
        a.Length == b.Length && string.Equals(a.Sha1Hex, b.Sha1Hex, StringComparison.OrdinalIgnoreCase);

    private static async Task<string> ComputeSha1HexAsync(string path, CancellationToken token)
    {
        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 128 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha1 = SHA1.Create();
        var hash = await sha1.ComputeHashAsync(stream, token).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }

    private static async Task<SteamManagedInstallReceipt?> TryReadReceiptAsync(string managedPath, CancellationToken token)
    {
        var receiptPath = Path.Combine(managedPath, SteamManagedInstallReceipt.FileName);
        if (!File.Exists(receiptPath))
            return null;
        try
        {
            await using var stream = new FileStream(receiptPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var receipt = await JsonSerializer.DeserializeAsync(
                    stream,
                    SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt,
                    token)
                .ConfigureAwait(false);
            return IsReceiptStructurallyValid(receipt) ? receipt : null;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return null;
        }
    }

    private static SteamManagedInstallReceipt? TryReadReceipt(string managedPath)
    {
        var receiptPath = Path.Combine(managedPath, SteamManagedInstallReceipt.FileName);
        if (!File.Exists(receiptPath))
            return null;
        try
        {
            var receipt = JsonSerializer.Deserialize(
                File.ReadAllText(receiptPath),
                SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
            return IsReceiptStructurallyValid(receipt) ? receipt : null;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return null;
        }
    }

    private static async Task WriteReceiptAsync(string root, SteamManagedInstallReceipt receipt, CancellationToken token)
    {
        if (!IsReceiptStructurallyValid(receipt))
            throw new InvalidDataException("Refusing to write an invalid managed-install receipt.");

        var finalPath = Path.Combine(root, SteamManagedInstallReceipt.FileName);
        var tempPath = finalPath + ".tmp";
        TryDeleteFile(tempPath);

        try
        {
            await using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(
                        stream,
                        receipt,
                        SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt,
                        token)
                    .ConfigureAwait(false);
                await stream.FlushAsync(token).ConfigureAwait(false);
            }

            File.Move(tempPath, finalPath, overwrite: true);
        }
        finally
        {
            // A cancelled/failed write must never leave a receipt-adjacent temp file
            // that would later make the managed tree look corrupt.
            TryDeleteFile(tempPath);
        }
    }

    private static bool IsReceiptStructurallyValid(SteamManagedInstallReceipt? receipt)
    {
        if (receipt is null ||
            receipt.AppId == 0 ||
            receipt.DepotId == 0 ||
            receipt.ManifestId == 0 ||
            string.IsNullOrWhiteSpace(receipt.Branch) ||
            receipt.Files is null)
        {
            return false;
        }

        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in receipt.Files)
        {
            if (file is null ||
                file.Length < 0 ||
                !SteamSingleFileTargetSelector.IsSafeRelativePath(file.RelativePath) ||
                !paths.Add(file.RelativePath) ||
                !IsSha1Hex(file.Sha1Hex))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSha1Hex(string? value) =>
        value is { Length: 40 } && value.All(Uri.IsHexDigit);

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static ulong SumBytes(IEnumerable<SteamManagedInstallFile> files)
    {
        ulong total = 0;
        foreach (var file in files)
            checked { total += (ulong)file.Length; }
        return total;
    }

    private static void RecoverInterruptedCommit(string managedPath, string backupPath)
    {
        if (Directory.Exists(managedPath) && Directory.Exists(backupPath))
        {
            Directory.Delete(backupPath, recursive: true);
            return;
        }
        if (!Directory.Exists(managedPath) && Directory.Exists(backupPath))
            Directory.Move(backupPath, managedPath);
    }

    private static void CleanupDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;
        Directory.Delete(path, recursive: true);
    }

    private static TimeSpan Remaining(TimeSpan timeout, Stopwatch sw)
    {
        var remaining = timeout - sw.Elapsed;
        return remaining > TimeSpan.FromSeconds(1) ? remaining : TimeSpan.FromSeconds(1);
    }

    private static string BuildManagedRelativePath(uint depotId) =>
        Path.Combine("Step12-ManagedInstall", $"Depot-{depotId}");

    private static string BuildStagingRelativePath(uint depotId, ulong manifestId) =>
        Path.Combine("Step12-ManagedInstall", $".staging-depot-{depotId}-manifest-{manifestId}");

    private static string BuildBackupRelativePath(uint depotId) =>
        Path.Combine("Step12-ManagedInstall", $".backup-depot-{depotId}");

    private string ResolveOutputPath(string relative)
    {
        Directory.CreateDirectory(_outputRootDirectory);
        return ResolveChildPath(_outputRootDirectory, relative);
    }

    private static string ResolveChildPath(string root, string relative)
    {
        var normalized = relative.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(root, normalized));
        if (!candidate.StartsWith(fullRoot, StringComparison.Ordinal))
            throw new InvalidDataException($"Path escapes managed root: {relative}");
        return candidate;
    }
}
