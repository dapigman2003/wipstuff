namespace StS2Launcher.Core;

public sealed record TransformedRealStS2VeryEarlyInitializationGateResult(
    TransformedRealStS2VeryEarlyInitializationGate Gate,
    bool Passed,
    string Detail);
