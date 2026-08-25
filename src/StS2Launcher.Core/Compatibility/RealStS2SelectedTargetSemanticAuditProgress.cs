namespace StS2Launcher.Core;

public sealed record RealStS2SelectedTargetSemanticAuditProgress(
    RealStS2SelectedTargetSemanticAuditGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
