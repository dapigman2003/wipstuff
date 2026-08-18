namespace StS2Launcher.Core;

public enum SteamOfflineInstallPhase
{
    Locating = 0,
    ReadingReceipt = 1,
    VerifyingFiles = 2,
    Complete = 3,
}

/// <summary>
/// Step 13 local-only progress contract. No Steam/network metadata or secrets
/// are carried here; progress is derived only from the managed-install receipt
/// and local files.
/// </summary>
public sealed record SteamOfflineInstallProgress(
    SteamOfflineInstallPhase Phase,
    int CompletedFiles,
    int TotalFiles,
    ulong CompletedBytes,
    ulong TotalBytes,
    string? CurrentFile,
    string Message);
