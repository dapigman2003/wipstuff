using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace StS2Launcher.Core;

/// <summary>
/// Step 13 boundary: determine whether the already-managed Step 12 depot can be
/// trusted as an offline-ready local launcher state without consulting Steam,
/// a saved Steam session, PICS, CDN, sockets, or any other network service.
///
/// The proof is intentionally local and conservative: one managed depot,
/// source-generated receipt JSON, safe unique paths, exact file set, exact
/// lengths, and SHA-1 equality for every receipt entry.
///
/// This does not prove online manifest freshness and does not launch the game.
/// </summary>
public sealed class SteamOfflineInstallInspection
{
    public const uint TargetAppId = SteamManagedInstallAttempt.TargetAppId;
    public const string ManagedRootRelativePath = "Step12-ManagedInstall";

    private readonly string _outputRootDirectory;

    public SteamOfflineInstallInspection(string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputRootDirectory));

        _outputRootDirectory = Path.GetFullPath(outputRootDirectory);
    }

    public async Task<SteamOfflineInstallResult> RunAsync(
        IProgress<SteamOfflineInstallProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var outcome = SteamOfflineInstallOutcome.Failed;
        var state = SteamOfflineInstallState.Unknown;
        uint? depotId = null;
        ulong? manifestId = null;
        string? branch = null;
        var managedDirectoryFound = false;
        var receiptFound = false;
        var receiptStructurallyValid = false;
        var plannedFiles = 0;
        ulong plannedBytes = 0;
        var verifiedFiles = 0;
        ulong verifiedBytes = 0;
        var exactTreeVerified = false;
        string? managedRelativePath = null;
        string? error = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new SteamOfflineInstallProgress(
                SteamOfflineInstallPhase.Locating,
                0,
                0,
                0,
                0,
                null,
                "Locating the existing Step 12 managed install using local storage only…"));

            var managedRoot = ResolveChildPath(_outputRootDirectory, ManagedRootRelativePath);
            if (!Directory.Exists(managedRoot))
            {
                outcome = SteamOfflineInstallOutcome.NoManagedInstall;
                state = SteamOfflineInstallState.OnlineSetupRequired;
                return BuildResult();
            }

            var depotDirectories = Directory
                .EnumerateDirectories(managedRoot, "Depot-*", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (depotDirectories.Length == 0)
            {
                outcome = SteamOfflineInstallOutcome.NoManagedInstall;
                state = SteamOfflineInstallState.OnlineSetupRequired;
                return BuildResult();
            }

            if (depotDirectories.Length != 1)
            {
                outcome = SteamOfflineInstallOutcome.InvalidLocalLayout;
                state = SteamOfflineInstallState.RepairRequired;
                error = $"Expected exactly one managed Depot-* directory for the current one-depot boundary; found {depotDirectories.Length}.";
                return BuildResult();
            }

            var managedPath = depotDirectories[0];
            managedDirectoryFound = true;
            managedRelativePath = Path.GetRelativePath(_outputRootDirectory, managedPath).Replace('\\', '/');

            var directoryName = Path.GetFileName(managedPath);
            if (!directoryName.StartsWith("Depot-", StringComparison.Ordinal) ||
                !uint.TryParse(directoryName.AsSpan("Depot-".Length), out var parsedDepotId) ||
                parsedDepotId == 0)
            {
                outcome = SteamOfflineInstallOutcome.InvalidLocalLayout;
                state = SteamOfflineInstallState.RepairRequired;
                error = $"Managed depot directory name is invalid: {directoryName}";
                return BuildResult();
            }
            depotId = parsedDepotId;

            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new SteamOfflineInstallProgress(
                SteamOfflineInstallPhase.ReadingReceipt,
                0,
                0,
                0,
                0,
                SteamManagedInstallReceipt.FileName,
                "Reading the non-secret Step 12 receipt locally; Steam session and network are not consulted…"));

            var receiptPath = Path.Combine(managedPath, SteamManagedInstallReceipt.FileName);
            receiptFound = File.Exists(receiptPath);
            if (!receiptFound)
            {
                outcome = SteamOfflineInstallOutcome.ReceiptMissingOrInvalid;
                state = SteamOfflineInstallState.RepairRequired;
                error = "The managed install directory exists, but its Step 12 receipt is missing.";
                return BuildResult();
            }

            SteamManagedInstallReceipt? receipt;
            try
            {
                await using var stream = new FileStream(
                    receiptPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 16 * 1024,
                    options: FileOptions.Asynchronous | FileOptions.SequentialScan);
                receipt = await JsonSerializer.DeserializeAsync(
                        stream,
                        SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or NotSupportedException)
            {
                outcome = SteamOfflineInstallOutcome.ReceiptMissingOrInvalid;
                state = SteamOfflineInstallState.RepairRequired;
                error = $"Managed-install receipt could not be read safely: {ex.GetType().Name}: {ex.Message}";
                return BuildResult();
            }

            if (!IsReceiptStructurallyValid(receipt, parsedDepotId))
            {
                outcome = SteamOfflineInstallOutcome.ReceiptMissingOrInvalid;
                state = SteamOfflineInstallState.RepairRequired;
                error = "Managed-install receipt is malformed, foreign, empty, or inconsistent with its depot directory.";
                return BuildResult();
            }

            receiptStructurallyValid = true;
            manifestId = receipt!.ManifestId;
            branch = receipt.Branch;
            plannedFiles = receipt.Files.Count;
            try
            {
                foreach (var file in receipt.Files)
                    checked { plannedBytes += (ulong)file.Length; }
            }
            catch (OverflowException)
            {
                outcome = SteamOfflineInstallOutcome.ReceiptMissingOrInvalid;
                state = SteamOfflineInstallState.RepairRequired;
                error = "Managed-install receipt byte count overflowed.";
                return BuildResult();
            }

            var expected = new Dictionary<string, SteamManagedInstallFile>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in receipt.Files)
                expected.Add(file.RelativePath, file);

            var actual = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in Directory.EnumerateFiles(managedPath, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var relative = Path.GetRelativePath(managedPath, path).Replace('\\', '/');
                if (string.Equals(relative, SteamManagedInstallReceipt.FileName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!SteamSingleFileTargetSelector.IsSafeRelativePath(relative) ||
                    !actual.Add(relative) ||
                    !expected.ContainsKey(relative))
                {
                    outcome = SteamOfflineInstallOutcome.VerificationFailed;
                    state = SteamOfflineInstallState.RepairRequired;
                    error = $"Managed tree contains an unexpected or unsafe file: {relative}";
                    return BuildResult();
                }
            }

            if (actual.Count != expected.Count)
            {
                outcome = SteamOfflineInstallOutcome.VerificationFailed;
                state = SteamOfflineInstallState.RepairRequired;
                error = $"Managed tree file count does not match receipt: actual={actual.Count}, expected={expected.Count}.";
                return BuildResult();
            }

            progress?.Report(new SteamOfflineInstallProgress(
                SteamOfflineInstallPhase.VerifyingFiles,
                0,
                plannedFiles,
                0,
                plannedBytes,
                null,
                "Hash-verifying the exact managed tree locally…"));

            foreach (var file in receipt.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var path = ResolveChildPath(managedPath, file.RelativePath);
                if (!File.Exists(path))
                {
                    outcome = SteamOfflineInstallOutcome.VerificationFailed;
                    state = SteamOfflineInstallState.RepairRequired;
                    error = $"Managed file is missing: {file.RelativePath}";
                    return BuildResult();
                }

                FileInfo info;
                try
                {
                    info = new FileInfo(path);
                    if (info.Length != file.Length)
                    {
                        outcome = SteamOfflineInstallOutcome.VerificationFailed;
                        state = SteamOfflineInstallState.RepairRequired;
                        error = $"Managed file length mismatch: {file.RelativePath}";
                        return BuildResult();
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    outcome = SteamOfflineInstallOutcome.VerificationFailed;
                    state = SteamOfflineInstallState.RepairRequired;
                    error = $"Managed file could not be inspected: {file.RelativePath}: {ex.GetType().Name}: {ex.Message}";
                    return BuildResult();
                }

                var actualSha1 = await ComputeSha1HexAsync(path, cancellationToken).ConfigureAwait(false);
                if (!string.Equals(actualSha1, file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                {
                    outcome = SteamOfflineInstallOutcome.VerificationFailed;
                    state = SteamOfflineInstallState.RepairRequired;
                    error = $"Managed file SHA-1 mismatch: {file.RelativePath}";
                    return BuildResult();
                }

                verifiedFiles++;
                checked { verifiedBytes += (ulong)file.Length; }
                progress?.Report(new SteamOfflineInstallProgress(
                    SteamOfflineInstallPhase.VerifyingFiles,
                    verifiedFiles,
                    plannedFiles,
                    verifiedBytes,
                    plannedBytes,
                    file.RelativePath,
                    "Local SHA-1 verified; no Steam request was made."));
            }

            exactTreeVerified = verifiedFiles == plannedFiles && verifiedBytes == plannedBytes;
            if (!exactTreeVerified)
            {
                outcome = SteamOfflineInstallOutcome.VerificationFailed;
                state = SteamOfflineInstallState.RepairRequired;
                error = "Managed tree verification ended without satisfying the full receipt.";
                return BuildResult();
            }

            outcome = SteamOfflineInstallOutcome.OfflineReady;
            state = SteamOfflineInstallState.OfflineReady;
            progress?.Report(new SteamOfflineInstallProgress(
                SteamOfflineInstallPhase.Complete,
                verifiedFiles,
                plannedFiles,
                verifiedBytes,
                plannedBytes,
                null,
                "Offline-ready local state proven. Online manifest freshness remains intentionally unknown."));
            return BuildResult();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            outcome = SteamOfflineInstallOutcome.Cancelled;
            state = SteamOfflineInstallState.Unknown;
            error = "Step 13 local verification was cancelled. No managed files were changed.";
            return BuildResult();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            outcome = managedDirectoryFound
                ? SteamOfflineInstallOutcome.VerificationFailed
                : SteamOfflineInstallOutcome.Failed;
            state = managedDirectoryFound
                ? SteamOfflineInstallState.RepairRequired
                : SteamOfflineInstallState.Unknown;
            error = $"{ex.GetType().Name}: {ex.Message}";
            return BuildResult();
        }
        catch (Exception ex)
        {
            outcome = SteamOfflineInstallOutcome.Failed;
            state = SteamOfflineInstallState.Unknown;
            error = $"{ex.GetType().Name}: {ex.Message}";
            return BuildResult();
        }

        SteamOfflineInstallResult BuildResult() => new(
            outcome,
            state,
            TargetAppId,
            depotId,
            manifestId,
            branch,
            managedDirectoryFound,
            receiptFound,
            receiptStructurallyValid,
            plannedFiles,
            plannedBytes,
            verifiedFiles,
            verifiedBytes,
            exactTreeVerified,
            SteamSessionConsulted: false,
            NetworkAccessAttempted: false,
            OnlineManifestFreshnessKnown: false,
            ManagedInstallRelativePath: managedRelativePath,
            Elapsed: sw.Elapsed,
            Error: error);
    }

    private static bool IsReceiptStructurallyValid(SteamManagedInstallReceipt? receipt, uint directoryDepotId)
    {
        if (receipt is null ||
            receipt.SchemaVersion != SteamManagedInstallReceipt.CurrentSchemaVersion ||
            receipt.AppId != TargetAppId ||
            receipt.DepotId == 0 ||
            receipt.DepotId != directoryDepotId ||
            receipt.ManifestId == 0 ||
            string.IsNullOrWhiteSpace(receipt.Branch) ||
            receipt.Files is null ||
            receipt.Files.Count == 0)
        {
            return false;
        }

        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in receipt.Files)
        {
            if (file is null ||
                file.Length < 0 ||
                !SteamSingleFileTargetSelector.IsSafeRelativePath(file.RelativePath) ||
                string.Equals(file.RelativePath, SteamManagedInstallReceipt.FileName, StringComparison.OrdinalIgnoreCase) ||
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

    private static async Task<string> ComputeSha1HexAsync(string path, CancellationToken token)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 128 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha1 = SHA1.Create();
        var hash = await sha1.ComputeHashAsync(stream, token).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }

    private static string ResolveChildPath(string root, string relative)
    {
        var normalized = relative.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(root, normalized));
        if (!candidate.StartsWith(fullRoot, StringComparison.Ordinal))
            throw new InvalidDataException($"Path escapes local managed root: {relative}");
        return candidate;
    }
}
