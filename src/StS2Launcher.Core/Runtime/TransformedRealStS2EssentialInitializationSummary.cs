namespace StS2Launcher.Core;

public sealed record TransformedRealStS2EssentialInitializationSummary(
    IReadOnlyList<TransformedRealStS2EssentialInitializationGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public TransformedRealStS2EssentialInitializationGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "STEP 36.0 ESSENTIAL INITIALIZATION COMPLETE — 4/4"
        : $"STEP 36.0 ESSENTIAL INITIALIZATION {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
