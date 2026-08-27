namespace StS2Launcher.Core;

public sealed record ControlledManagedInitializationProgress(
    ControlledManagedInitializationGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
