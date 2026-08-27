namespace StS2Launcher.Core;

public sealed record RealAssemblyRewriteSummary(
    IReadOnlyList<RealAssemblyRewriteGateResult> Results,
    bool Passed,
    RealAssemblyRewriteGate? FirstFailingGate)
{
    public int PassedGates => Results.Count(result => result.Passed);

    public string Summary => Passed
        ? $"REAL ASSEMBLY REWRITE WORKSPACE PASS — {PassedGates}/4"
        : FirstFailingGate is null
            ? $"REAL ASSEMBLY REWRITE WORKSPACE IN PROGRESS — {PassedGates}/4"
            : $"REAL ASSEMBLY REWRITE WORKSPACE FAIL — Gate {(char)('A' + (int)FirstFailingGate.Value - 1)} ({FirstFailingGate})";
}
