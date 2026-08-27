namespace StS2Launcher.Core;

public sealed record RealStS2PrepareMethodRewriteGateResult(
    RealStS2PrepareMethodRewriteGate Gate,
    bool Passed,
    string Detail);
