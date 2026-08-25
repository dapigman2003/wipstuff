namespace StS2Launcher.Core;

public sealed record RealStS2PrepareMethodRewriteProgress(
    RealStS2PrepareMethodRewriteGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
