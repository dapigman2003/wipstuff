namespace StS2Launcher.Core;

public sealed record RealAssemblyRewriteGateResult(
    RealAssemblyRewriteGate Gate,
    bool Passed,
    string Detail);
