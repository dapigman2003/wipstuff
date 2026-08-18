namespace StS2Launcher.Core;

public enum SteamCompatibilityInventoryOutcome
{
    Failed = 0,
    LocalInstallNotReady = 1,
    Complete = 2,
    Cancelled = 3,
}

/// <summary>
/// Step 14 result contract. This is a read-only inventory of the already-verified
/// Step 12 managed depot. Evidence lists are diagnostic indicators, not proof
/// that a referenced API/dependency is actually reached at runtime.
/// </summary>
public sealed record SteamCompatibilityInventoryResult(
    SteamCompatibilityInventoryOutcome Outcome,
    uint TargetAppId,
    uint? DepotId,
    ulong? InstalledManifestId,
    string? Branch,
    bool OfflineReadyPreconditionVerified,
    int TotalFiles,
    ulong TotalBytes,
    int AssetFiles,
    ulong AssetBytes,
    int GodotContentFiles,
    int ManagedAssemblyFiles,
    ulong ManagedAssemblyBytes,
    int ManagedAssembliesScanned,
    int NativeBinaryFiles,
    ulong NativeBinaryBytes,
    int GodotSharpIndicatorFiles,
    int FmodIndicatorFiles,
    int SpineIndicatorFiles,
    int ReflectionIndicatorFiles,
    int DynamicCodeIndicatorFiles,
    int PlatformSpecificFiles,
    int OtherFiles,
    IReadOnlyList<string> ManagedAssemblyEvidence,
    IReadOnlyList<string> NativeBinaryEvidence,
    IReadOnlyList<string> GodotSharpEvidence,
    IReadOnlyList<string> FmodEvidence,
    IReadOnlyList<string> SpineEvidence,
    IReadOnlyList<string> ReflectionEvidence,
    IReadOnlyList<string> DynamicCodeEvidence,
    IReadOnlyList<string> PlatformSpecificEvidence,
    IReadOnlyList<string> PotentialIosBlockerSignals,
    IReadOnlyList<string> DependencyNotes,
    bool SteamSessionConsulted,
    bool NetworkAccessAttempted,
    bool ManagedInstallModified,
    bool GameLaunchAttempted,
    TimeSpan Elapsed,
    string? Error)
{
    public bool Success => Outcome == SteamCompatibilityInventoryOutcome.Complete &&
                           OfflineReadyPreconditionVerified &&
                           TotalFiles > 0 &&
                           !SteamSessionConsulted &&
                           !NetworkAccessAttempted &&
                           !ManagedInstallModified &&
                           !GameLaunchAttempted;

    public string Summary => Outcome switch
    {
        SteamCompatibilityInventoryOutcome.Complete =>
            $"COMPATIBILITY INVENTORY PASS — {TotalFiles} files classified; {PotentialIosBlockerSignals.Count} potential iOS blocker signal(s)",
        SteamCompatibilityInventoryOutcome.LocalInstallNotReady =>
            "COMPATIBILITY INVENTORY BLOCKED — local managed install is not OfflineReady",
        SteamCompatibilityInventoryOutcome.Cancelled =>
            "COMPATIBILITY INVENTORY CANCELLED",
        _ => "COMPATIBILITY INVENTORY FAIL",
    };
}
