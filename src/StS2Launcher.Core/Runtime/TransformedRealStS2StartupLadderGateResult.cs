namespace StS2Launcher.Core;

public sealed record TransformedRealStS2StartupLadderGateResult(
    int Step,
    string StepName,
    TransformedRealStS2StartupLadderGate Gate,
    bool Passed,
    string Detail);
