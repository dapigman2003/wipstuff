namespace StS2Launcher.Core;

public sealed record RealStS2SelectedTargetSemanticAuditGateResult(
    RealStS2SelectedTargetSemanticAuditGate Gate,
    bool Passed,
    string Detail);
