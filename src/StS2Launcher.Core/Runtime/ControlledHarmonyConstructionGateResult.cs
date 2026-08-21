namespace StS2Launcher.Core;

public sealed record ControlledHarmonyConstructionGateResult(
    ControlledHarmonyConstructionGate Gate,
    bool Passed,
    string Detail);
