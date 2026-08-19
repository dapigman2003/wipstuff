namespace StS2Launcher.Core;

public sealed record GodotFoundationGateResult(
    GodotFoundationGate Gate,
    bool Passed,
    string Detail);
