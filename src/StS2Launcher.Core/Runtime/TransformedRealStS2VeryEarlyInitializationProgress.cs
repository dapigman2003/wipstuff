namespace StS2Launcher.Core;

public sealed record TransformedRealStS2VeryEarlyInitializationProgress(
    TransformedRealStS2VeryEarlyInitializationGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
