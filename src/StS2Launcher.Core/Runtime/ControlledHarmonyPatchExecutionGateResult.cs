namespace StS2Launcher.Core;

public sealed record ControlledHarmonyPatchExecutionGateResult(
    ControlledHarmonyPatchExecutionGate Gate,
    bool Passed,
    string Detail);
