namespace StS2Launcher.Core;

public enum SteamCompatibilityInventoryPhase
{
    Locating = 0,
    VerifyingOfflineInstall = 1,
    ReadingReceipt = 2,
    ClassifyingFiles = 3,
    ScanningManagedAssemblies = 4,
    Complete = 5,
}

public sealed record SteamCompatibilityInventoryProgress(
    SteamCompatibilityInventoryPhase Phase,
    int ProcessedFiles,
    int TotalFiles,
    ulong ProcessedBytes,
    ulong TotalBytes,
    string? CurrentRelativePath,
    string Message);
