namespace StS2Launcher.Core;

public sealed record DynamicManagedExecutionGateResult(
    DynamicManagedExecutionGate Gate,
    bool Passed,
    string Detail);
