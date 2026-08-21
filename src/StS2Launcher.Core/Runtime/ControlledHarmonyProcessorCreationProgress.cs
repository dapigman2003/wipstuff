namespace StS2Launcher.Core;

public sealed record ControlledHarmonyProcessorCreationProgress(
    ControlledHarmonyProcessorCreationGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
