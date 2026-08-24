namespace StS2Launcher.Core;

public sealed record AheadOfLoadManagedTransformationProgress(
    AheadOfLoadManagedTransformationGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
