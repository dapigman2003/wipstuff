namespace StS2Launcher.Core;

/// <summary>
/// Post-Step-12 maintenance helper for the project-owned Step 11 content cache.
/// This is intentionally scoped only to the project-owned Step 11 source cache.
/// </summary>
public sealed class SteamDownloadCacheMaintenance
{
    public const string CacheRelativePath = "Step11-ResumableDepot";

    private readonly string _outputRootDirectory;

    public SteamDownloadCacheMaintenance(string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputRootDirectory));
        _outputRootDirectory = outputRootDirectory;
    }

    public SteamDownloadCacheClearResult Clear()
    {
        var cachePath = ResolveCachePath();
        if (!Directory.Exists(cachePath))
            return new SteamDownloadCacheClearResult(false, true, CacheRelativePath);

        Directory.Delete(cachePath, recursive: true);
        if (Directory.Exists(cachePath))
            throw new IOException("The Step 11 download-cache directory still exists after deletion.");

        return new SteamDownloadCacheClearResult(true, true, CacheRelativePath);
    }

    public bool Exists() => Directory.Exists(ResolveCachePath());

    private string ResolveCachePath()
    {
        Directory.CreateDirectory(_outputRootDirectory);
        var root = Path.GetFullPath(_outputRootDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(_outputRootDirectory, CacheRelativePath));
        if (!candidate.StartsWith(root, StringComparison.Ordinal))
            throw new InvalidOperationException("Resolved Step 11 cache path escaped the configured app-data directory.");
        return candidate;
    }
}

public sealed record SteamDownloadCacheClearResult(
    bool CacheExisted,
    bool CacheAbsentAfterClear,
    string CacheRelativePath);
