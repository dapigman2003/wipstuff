namespace StS2Launcher.Core;

public sealed record RealStS2SelectedTargetSemanticAuditSummary(
    IReadOnlyList<RealStS2SelectedTargetSemanticAuditGateResult> Gates,
    bool Passed,
    RealStS2SelectedTargetSemanticAuditGate? FirstFailingGate)
{
    public string Summary => Passed
        ? "SELECTED TARGET SEMANTIC CONTEXT AUDIT PASS — 4/4"
        : $"SELECTED TARGET SEMANTIC CONTEXT AUDIT FAIL — {Gates.Count(g => g.Passed)}/4";
}
