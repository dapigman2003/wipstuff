namespace StS2Launcher.Core;

public sealed record DynamicManagedExecutionProgress(
    DynamicManagedExecutionGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
