namespace StS2Launcher.Core;

public sealed record RealStS2CompatibilityTargetAuditSummary(
    IReadOnlyList<RealStS2CompatibilityTargetAuditGateResult> Gates,
    bool Passed,
    RealStS2CompatibilityTargetAuditGate? FirstFailingGate)
{
    public string Summary => Passed
        ? "REAL STS2 COMPATIBILITY TARGET AUDIT PASS — 4/4"
        : $"REAL STS2 COMPATIBILITY TARGET AUDIT FAIL — {Gates.Count}/4" +
          (FirstFailingGate is null ? string.Empty : $", first failure: {FirstFailingGate}");
}
