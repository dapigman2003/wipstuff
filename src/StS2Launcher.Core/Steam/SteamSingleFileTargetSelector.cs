using SteamKit2;

namespace StS2Launcher.Core;

public sealed record SteamSingleFileDepotTarget(
    uint DepotId,
    ulong ManifestId,
    string Branch,
    string? OsList,
    string? OsArch,
    string? Language);

/// <summary>
/// Pure Step 09 selection policy. It keeps the live operation deterministic and
/// bounded: one direct depot, public branch only, and one small regular file.
/// </summary>
public static class SteamSingleFileTargetSelector
{
    public const ulong MaxTargetFileBytes = 2UL * 1024UL * 1024UL;

    public static SteamSingleFileDepotTarget? SelectDepot(
        IReadOnlyList<SteamDepotDiscovery> depots,
        uint targetAppId)
    {
        ArgumentNullException.ThrowIfNull(depots);
        if (targetAppId == 0)
            throw new ArgumentOutOfRangeException(nameof(targetAppId));

        return depots
            .Where(depot => !depot.DepotFromAppId.HasValue)
            .Select(depot => new
            {
                Depot = depot,
                Public = depot.Manifests.FirstOrDefault(manifest =>
                    string.Equals(manifest.Branch, "public", StringComparison.OrdinalIgnoreCase) &&
                    ulong.TryParse(manifest.ManifestId, out var parsed) && parsed != 0),
            })
            .Where(candidate => candidate.Public is not null)
            .Select(candidate => new
            {
                candidate.Depot,
                Manifest = candidate.Public!,
                PlatformScore = PlatformScore(candidate.Depot.OsList),
                LanguageScore = LanguageScore(candidate.Depot.Language),
            })
            .OrderBy(candidate => candidate.PlatformScore)
            .ThenBy(candidate => candidate.LanguageScore)
            .ThenBy(candidate => candidate.Depot.DepotId)
            .Select(candidate => new SteamSingleFileDepotTarget(
                candidate.Depot.DepotId,
                ulong.Parse(candidate.Manifest.ManifestId),
                "public",
                candidate.Depot.OsList,
                candidate.Depot.OsArch,
                candidate.Depot.Language))
            .FirstOrDefault();
    }

    public static DepotManifest.FileData? SelectFile(
        DepotManifest manifest,
        ulong maxBytes = MaxTargetFileBytes)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        if (maxBytes == 0)
            throw new ArgumentOutOfRangeException(nameof(maxBytes));

        return manifest.Files
            .Where(file => !file.Flags.HasFlag(EDepotFileFlag.Directory))
            .Where(file => file.TotalSize > 0 && file.TotalSize <= maxBytes)
            .Where(file => file.TotalSize <= int.MaxValue)
            .Where(file => file.Chunks.Count > 0)
            .Where(file => IsSafeRelativePath(file.FileName))
            .Where(ChunksFitFile)
            .OrderBy(file => file.TotalSize)
            .ThenBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    public static bool IsSafeRelativePath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            return false;

        var normalized = relativePath.Replace('\\', '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0 && segments.All(segment =>
            segment != "." &&
            segment != ".." &&
            segment.IndexOfAny(Path.GetInvalidFileNameChars()) < 0);
    }

    public static string NormalizeRelativePath(string relativePath)
    {
        if (!IsSafeRelativePath(relativePath))
            throw new ArgumentException("Unsafe Steam manifest path.", nameof(relativePath));

        return string.Join(Path.DirectorySeparatorChar,
            relativePath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool ChunksFitFile(DepotManifest.FileData file)
    {
        foreach (var chunk in file.Chunks)
        {
            if (chunk.UncompressedLength == 0)
                return false;

            if (chunk.Offset > file.TotalSize)
                return false;

            var end = chunk.Offset + chunk.UncompressedLength;
            if (end < chunk.Offset || end > file.TotalSize)
                return false;
        }

        return true;
    }

    private static int PlatformScore(string? osList)
    {
        if (string.IsNullOrWhiteSpace(osList))
            return 1;

        var values = osList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return values.Any(value =>
            string.Equals(value, "macos", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "macosx", StringComparison.OrdinalIgnoreCase))
            ? 0
            : 2;
    }

    private static int LanguageScore(string? language) =>
        string.IsNullOrWhiteSpace(language) ||
        string.Equals(language, "english", StringComparison.OrdinalIgnoreCase)
            ? 0
            : 1;
}
