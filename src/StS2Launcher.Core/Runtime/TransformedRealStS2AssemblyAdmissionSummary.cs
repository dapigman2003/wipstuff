namespace StS2Launcher.Core;

public sealed record TransformedRealStS2AssemblyAdmissionSummary(
    IReadOnlyList<TransformedRealStS2AssemblyAdmissionGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public TransformedRealStS2AssemblyAdmissionGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "TRANSFORMED REAL STS2 CLR ADMISSION PASS — 4/4"
        : $"TRANSFORMED REAL STS2 CLR ADMISSION {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
