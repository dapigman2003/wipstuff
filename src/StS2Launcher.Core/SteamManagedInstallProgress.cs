namespace StS2Launcher.Core;

public enum SteamManagedInstallPhase
{
    Discovering = 0,
    Inspecting = 1,
    Acquiring = 2,
    VerifyingSource = 3,
    Staging = 4,
    Committing = 5,
    Complete = 6,
}

public sealed record SteamManagedInstallProgress(
    SteamManagedInstallPhase Phase,
    int CompletedFiles,
    int TotalFiles,
    ulong CompletedBytes,
    ulong TotalBytes,
    string? CurrentFile,
    string Message);
