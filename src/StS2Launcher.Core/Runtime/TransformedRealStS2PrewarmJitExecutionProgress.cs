namespace StS2Launcher.Core;

public sealed record TransformedRealStS2PrewarmJitExecutionProgress(
    TransformedRealStS2PrewarmJitExecutionGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
