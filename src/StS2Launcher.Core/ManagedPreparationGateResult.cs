namespace StS2Launcher.Core;

public sealed record ManagedPreparationGateResult(
    ManagedPreparationGate Gate,
    bool Passed,
    string Detail);
