namespace StS2Launcher.Core;

public sealed record TransformedRealStS2GameStartupFrontierGateResult(
    TransformedRealStS2GameStartupFrontierGate Gate,
    bool Passed,
    string Detail);
