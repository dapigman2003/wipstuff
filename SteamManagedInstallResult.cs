namespace StS2Launcher.Core;

public enum SteamManagedInstallState
{
    Unknown = 0,
    NotInstalled = 1,
    UpToDate = 2,
    UpdateAvailable = 3,
    RepairNeeded = 4,
}

public enum SteamManagedInstallAction
{
    None = 0,
    Install = 1,
    Update = 2,
    Repair = 3,
}

public enum SteamManagedInstallOutcome
{
    Failed = 0,
    DiscoveryFailed = 1,
    NoSuitableDepot = 2,
    AcquisitionFailed = 3,
    SourceValidationFailed = 4,
    StagingFailed = 5,
    CommitFailed = 6,
    UpToDate = 7,
    Installed = 8,
    Updated = 9,
    Repaired = 10,
    Cancelled = 11,
    TimedOut = 12,
}

public sealed record SteamManagedInstallResult(
    SteamManagedInstallOutcome Outcome,
    SteamManagedInstallState StateBefore,
    SteamManagedInstallState StateAfter,
    SteamManagedInstallAction ActionTaken,
    uint TargetAppId,
    uint? DepotId,
    ulong? CurrentManifestId,
    ulong? InstalledManifestIdBefore,
    ulong? InstalledManifestIdAfter,
    string? Branch,
    int PlannedFiles,
    ulong PlannedBytes,
    int VerifiedSourceFiles,
    ulong VerifiedSourceBytes,
    bool SourceCacheReverifiedAgainstCurrentManifest,
    ulong SourceNewlyDownloadedBytes,
    int ReusedLocalFiles,
    ulong ReusedLocalBytes,
    int ReplacedFiles,
    ulong ReplacedBytes,
    bool ExistingInstallPreservedUntilCommit,
    bool AtomicCommitCompleted,
    bool RollbackRestoredPreviousInstall,
    bool StagingAbsentAfterResult,
    bool BackupAbsentAfterResult,
    string? ManagedInstallRelativePath,
    string? SourceCacheRelativePath,
    TimeSpan Elapsed,
    string? Error)
{
    public bool Success => Outcome is SteamManagedInstallOutcome.UpToDate
        or SteamManagedInstallOutcome.Installed
        or SteamManagedInstallOutcome.Updated
        or SteamManagedInstallOutcome.Repaired;

    public string Summary => Outcome switch
    {
        SteamManagedInstallOutcome.UpToDate => $"INSTALL MANAGER PASS — up to date ({PlannedFiles} files)",
        SteamManagedInstallOutcome.Installed => $"INSTALL PASS — {PlannedFiles} files ({PlannedBytes} bytes)",
        SteamManagedInstallOutcome.Updated => $"UPDATE PASS — manifest {InstalledManifestIdAfter}",
        SteamManagedInstallOutcome.Repaired => $"REPAIR PASS — {ReplacedFiles} files restored",
        SteamManagedInstallOutcome.Cancelled => "INSTALL MANAGER CANCELLED — previous install preserved",
        SteamManagedInstallOutcome.TimedOut => "INSTALL MANAGER TIMEOUT — previous install preserved",
        SteamManagedInstallOutcome.NoSuitableDepot => "INSTALL MANAGER FAIL — no suitable public depot",
        SteamManagedInstallOutcome.AcquisitionFailed => "INSTALL MANAGER FAIL — verified source unavailable",
        SteamManagedInstallOutcome.SourceValidationFailed => "INSTALL MANAGER FAIL — source cache validation failed",
        SteamManagedInstallOutcome.CommitFailed => "INSTALL MANAGER FAIL — atomic replacement failed",
        _ => "INSTALL MANAGER FAIL",
    };
}
