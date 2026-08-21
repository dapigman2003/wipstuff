namespace StS2Launcher.Core;

public sealed record ControlledHarmonyConstructionProgress(
    ControlledHarmonyConstructionGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
