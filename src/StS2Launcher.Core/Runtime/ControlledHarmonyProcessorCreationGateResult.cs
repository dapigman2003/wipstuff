namespace StS2Launcher.Core;

public sealed record ControlledHarmonyProcessorCreationGateResult(
    ControlledHarmonyProcessorCreationGate Gate,
    bool Passed,
    string Detail);
