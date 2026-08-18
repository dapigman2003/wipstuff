using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 10's deterministic, in-memory plan for one selected Steam depot.
/// It contains manifest metadata only; no depot keys, request codes, CDN tokens,
/// chunk payloads or downloaded file bytes are retained here.
/// </summary>
public sealed record SteamDepotDownloadPlan(
    SteamSingleFileDepotTarget Target,
    IReadOnlyList<string> Directories,
    IReadOnlyList<DepotManifest.FileData> Files,
    int TotalFileCount,
    int TotalChunkCount,
    ulong TotalBytes);

/// <summary>
/// Pure validation/planning policy for the minimal Step 10 full-depot downloader.
/// A plan is accepted only when every manifest path is sandbox-safe, every
/// regular file has a Steam SHA-1, and its chunks exactly cover the file without
/// gaps or overlaps. Symlink entries are deliberately rejected in this boundary.
/// </summary>
public static class SteamDepotDownloadPlanner
{
    public static SteamSingleFileDepotTarget? SelectDepot(
        IReadOnlyList<SteamDepotDiscovery> depots,
        uint targetAppId) =>
        SteamSingleFileTargetSelector.SelectDepot(depots, targetAppId);

    public static SteamDepotDownloadPlan Create(
        SteamSingleFileDepotTarget target,
        DepotManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(manifest);

        var directories = new List<string>();
        var files = new List<DepotManifest.FileData>();
        var pathKinds = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var totalChunks = 0;
        ulong totalBytes = 0;

        foreach (var entry in manifest.Files.OrderBy(file => file.FileName, StringComparer.OrdinalIgnoreCase))
        {
            if (!SteamSingleFileTargetSelector.IsSafeRelativePath(entry.FileName))
                throw new InvalidDataException($"Steam manifest contains an unsafe relative path: {entry.FileName}");

            var normalized = SteamSingleFileTargetSelector.NormalizeRelativePath(entry.FileName);
            var isDirectory = entry.Flags.HasFlag(EDepotFileFlag.Directory);

            if (!pathKinds.TryAdd(normalized, isDirectory))
                throw new InvalidDataException($"Steam manifest contains a duplicate/case-colliding path: {entry.FileName}");

            if (isDirectory)
            {
                directories.Add(normalized);
                continue;
            }

            if (LooksLikeSymbolicLink(entry))
                throw new InvalidDataException($"Step 10 does not materialize symbolic-link entries: {entry.FileName}");

            if (entry.TotalSize > long.MaxValue)
                throw new InvalidDataException($"Manifest file is too large for the local filesystem API: {entry.FileName}");

            if (entry.FileHash is not { Length: > 0 })
                throw new InvalidDataException($"Manifest file has no Steam SHA-1: {entry.FileName}");

            ValidateChunkCoverage(entry);

            files.Add(entry);
            checked
            {
                totalBytes += entry.TotalSize;
                totalChunks += entry.Chunks.Count;
            }
        }

        ValidateNoFileDirectoryCollisions(pathKinds);

        return new SteamDepotDownloadPlan(
            target,
            directories,
            files,
            files.Count,
            totalChunks,
            totalBytes);
    }

    private static void ValidateChunkCoverage(DepotManifest.FileData file)
    {
        if (file.TotalSize == 0)
        {
            if (file.Chunks.Count != 0)
                throw new InvalidDataException($"Zero-byte file unexpectedly contains chunks: {file.FileName}");
            return;
        }

        if (file.Chunks.Count == 0)
            throw new InvalidDataException($"Non-empty file contains no chunks: {file.FileName}");

        ulong expectedOffset = 0;
        foreach (var chunk in file.Chunks.OrderBy(chunk => chunk.Offset))
        {
            if (chunk.UncompressedLength == 0)
                throw new InvalidDataException($"Manifest contains a zero-length chunk: {file.FileName}");

            if (chunk.Offset != expectedOffset)
                throw new InvalidDataException($"Manifest chunks do not exactly cover file: {file.FileName}");

            checked
            {
                expectedOffset += chunk.UncompressedLength;
            }

            if (expectedOffset > file.TotalSize)
                throw new InvalidDataException($"Manifest chunk exceeds file size: {file.FileName}");
        }

        if (expectedOffset != file.TotalSize)
            throw new InvalidDataException($"Manifest chunks do not cover the full file: {file.FileName}");
    }

    private static bool LooksLikeSymbolicLink(DepotManifest.FileData file) =>
        !string.IsNullOrWhiteSpace(file.LinkTarget) ||
        file.Flags.ToString().Contains("Symlink", StringComparison.OrdinalIgnoreCase) ||
        file.Flags.ToString().Contains("Symbolic", StringComparison.OrdinalIgnoreCase);

    private static void ValidateNoFileDirectoryCollisions(
        IReadOnlyDictionary<string, bool> pathKinds)
    {
        foreach (var pair in pathKinds)
        {
            var parent = Path.GetDirectoryName(pair.Key);
            while (!string.IsNullOrEmpty(parent))
            {
                if (pathKinds.TryGetValue(parent, out var parentIsDirectory) && !parentIsDirectory)
                    throw new InvalidDataException($"Manifest file path collides with another file: {pair.Key}");
                parent = Path.GetDirectoryName(parent);
            }
        }
    }
}
