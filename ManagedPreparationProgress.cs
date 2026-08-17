namespace StS2Launcher.Core;

public sealed record ManagedPreparationProgress(
    ManagedPreparationGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
