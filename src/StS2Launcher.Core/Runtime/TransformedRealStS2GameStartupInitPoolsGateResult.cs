namespace StS2Launcher.Core;

public sealed record TransformedRealStS2GameStartupInitPoolsGateResult(
    TransformedRealStS2GameStartupInitPoolsGate Gate,
    bool Passed,
    string Detail);
