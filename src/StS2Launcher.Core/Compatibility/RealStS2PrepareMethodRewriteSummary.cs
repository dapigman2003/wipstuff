namespace StS2Launcher.Core;

public sealed record RealStS2PrepareMethodRewriteSummary(
    IReadOnlyList<RealStS2PrepareMethodRewriteGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public RealStS2PrepareMethodRewriteGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "REAL STS2 PREPAREMETHOD REWRITE PASS — 4/4"
        : $"REAL STS2 PREPAREMETHOD REWRITE {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
