namespace StS2Launcher.Core;

public sealed record ControlledManagedInitializationGateResult(
    ControlledManagedInitializationGate Gate,
    bool Passed,
    string Detail);
