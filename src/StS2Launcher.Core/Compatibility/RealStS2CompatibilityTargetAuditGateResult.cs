namespace StS2Launcher.Core;

public sealed record RealStS2CompatibilityTargetAuditGateResult(
    RealStS2CompatibilityTargetAuditGate Gate,
    bool Passed,
    string Detail);
