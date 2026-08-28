namespace StS2Launcher.Core;

public sealed record TransformedRealStS2PrewarmJitExecutionSummary(
    IReadOnlyList<TransformedRealStS2PrewarmJitExecutionGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public TransformedRealStS2PrewarmJitExecutionGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION PASS — 4/4"
        : $"TRANSFORMED REAL STS2 PREWARMJIT EXECUTION {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
