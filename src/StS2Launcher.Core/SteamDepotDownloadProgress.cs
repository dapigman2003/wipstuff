namespace StS2Launcher.Core;

public enum SteamDepotDownloadPhase
{
    Preparing = 0,
    Downloading = 1,
    Verifying = 2,
    Committing = 3,
    Complete = 4,
    Resuming = 5,
}

/// <summary>
/// Non-secret Step 10/11 progress telemetry suitable for direct UI display.
/// </summary>
public sealed record SteamDepotDownloadProgress(
    SteamDepotDownloadPhase Phase,
    int CompletedFiles,
    int TotalFiles,
    int CompletedChunks,
    int TotalChunks,
    ulong CompletedBytes,
    ulong TotalBytes,
    string? CurrentFile)
{
    public double Fraction => TotalBytes == 0
        ? (TotalFiles == 0 || CompletedFiles >= TotalFiles ? 1d : 0d)
        : Math.Clamp((double)CompletedBytes / TotalBytes, 0d, 1d);

    public int Percent => (int)Math.Round(Fraction * 100d, MidpointRounding.AwayFromZero);

    public string Summary =>
        $"DEPOT {Phase.ToString().ToUpperInvariant()} — {Percent}% • {CompletedFiles}/{TotalFiles} files • {CompletedBytes}/{TotalBytes} bytes";
}
