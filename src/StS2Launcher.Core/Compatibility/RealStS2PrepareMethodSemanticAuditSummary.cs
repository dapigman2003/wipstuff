namespace StS2Launcher.Core;

public sealed record RealStS2PrepareMethodSemanticAuditSummary(
    IReadOnlyList<RealStS2PrepareMethodSemanticAuditGateResult> Gates,
    bool Passed,
    RealStS2PrepareMethodSemanticAuditGate? FirstFailingGate)
{
    public string Summary => Passed
        ? "PREPAREMETHOD SEMANTIC CONTEXT AUDIT PASS — 4/4"
        : $"PREPAREMETHOD SEMANTIC CONTEXT AUDIT FAIL — {Gates.Count(g => g.Passed)}/4";
}
