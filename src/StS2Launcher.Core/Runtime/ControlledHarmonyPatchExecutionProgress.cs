namespace StS2Launcher.Core;

public sealed record ControlledHarmonyPatchExecutionProgress(
    ControlledHarmonyPatchExecutionGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
