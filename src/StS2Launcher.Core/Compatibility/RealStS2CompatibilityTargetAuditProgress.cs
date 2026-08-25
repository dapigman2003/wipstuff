namespace StS2Launcher.Core;

public sealed record RealStS2CompatibilityTargetAuditProgress(
    RealStS2CompatibilityTargetAuditGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
