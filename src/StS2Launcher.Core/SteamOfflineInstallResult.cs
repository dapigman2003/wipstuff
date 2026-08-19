namespace StS2Launcher.Core;

public enum SteamOfflineInstallState
{
    Unknown = 0,
    OnlineSetupRequired = 1,
    OfflineReady = 2,
    RepairRequired = 3,
}

public enum SteamOfflineInstallOutcome
{
    Failed = 0,
    NoManagedInstall = 1,
    InvalidLocalLayout = 2,
    ReceiptMissingOrInvalid = 3,
    VerificationFailed = 4,
    OfflineReady = 5,
    Cancelled = 6,
}

/// <summary>
/// Step 13 result contract. "OfflineReady" means the previously created
/// Step 12 managed-install receipt is structurally valid for App 2868840 and
/// the exact local tree currently matches every recorded length/SHA-1.
///
/// It deliberately does NOT mean that Steam was contacted, entitlement was
/// re-proven, the installed manifest is still the newest online manifest, or
/// that the game can execute on iOS yet.
/// </summary>
public sealed record SteamOfflineInstallResult(
    SteamOfflineInstallOutcome Outcome,
    SteamOfflineInstallState State,
    uint TargetAppId,
    uint? DepotId,
    ulong? InstalledManifestId,
    string? Branch,
    bool ManagedDirectoryFound,
    bool ReceiptFound,
    bool ReceiptStructurallyValid,
    int PlannedFiles,
    ulong PlannedBytes,
    int VerifiedFiles,
    ulong VerifiedBytes,
    bool ExactManagedTreeVerified,
    bool SteamSessionConsulted,
    bool NetworkAccessAttempted,
    bool OnlineManifestFreshnessKnown,
    string? ManagedInstallRelativePath,
    TimeSpan Elapsed,
    string? Error)
{
    public bool Success => Outcome == SteamOfflineInstallOutcome.OfflineReady &&
                           State == SteamOfflineInstallState.OfflineReady &&
                           ExactManagedTreeVerified &&
                           !SteamSessionConsulted &&
                           !NetworkAccessAttempted &&
                           !OnlineManifestFreshnessKnown;

    public string Summary => Outcome switch
    {
        SteamOfflineInstallOutcome.OfflineReady =>
            $"OFFLINE READY PASS — {VerifiedFiles}/{PlannedFiles} files verified locally",
        SteamOfflineInstallOutcome.NoManagedInstall =>
            "OFFLINE STATE — online setup required",
        SteamOfflineInstallOutcome.InvalidLocalLayout =>
            "OFFLINE STATE — local install layout requires repair",
        SteamOfflineInstallOutcome.ReceiptMissingOrInvalid =>
            "OFFLINE STATE — receipt missing/invalid; repair required",
        SteamOfflineInstallOutcome.VerificationFailed =>
            "OFFLINE STATE — local files require repair",
        SteamOfflineInstallOutcome.Cancelled =>
            "OFFLINE CHECK CANCELLED",
        _ => "OFFLINE CHECK FAIL",
    };
}
