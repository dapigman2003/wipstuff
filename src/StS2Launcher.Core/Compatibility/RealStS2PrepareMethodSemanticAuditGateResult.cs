namespace StS2Launcher.Core;

public sealed record RealStS2PrepareMethodSemanticAuditGateResult(
    RealStS2PrepareMethodSemanticAuditGate Gate,
    bool Passed,
    string Detail);
