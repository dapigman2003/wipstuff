namespace StS2Launcher.Core;

public sealed record TransformedRealStS2VeryEarlyInitializationProgress(
    TransformedRealStS2VeryEarlyInitializationGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail,
    ulong ProcessedBytes = 0,
    ulong TotalBytes = 0,
    string? Phase = null);
