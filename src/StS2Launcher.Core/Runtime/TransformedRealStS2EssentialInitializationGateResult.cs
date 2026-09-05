namespace StS2Launcher.Core;

public sealed record TransformedRealStS2EssentialInitializationGateResult(
    TransformedRealStS2EssentialInitializationGate Gate,
    bool Passed,
    string Detail);
