namespace StS2Launcher.Core;

public sealed record RealStS2PrepareMethodSemanticAuditProgress(
    RealStS2PrepareMethodSemanticAuditGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
