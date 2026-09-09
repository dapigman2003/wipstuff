namespace StS2Launcher.Core;

public sealed record TransformedRealStS2GameStartupFrontierSummary(
    IReadOnlyList<TransformedRealStS2GameStartupFrontierGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public TransformedRealStS2GameStartupFrontierGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "STEP 41.0 GAMESTARTUP ASYNC FRONTIER MAP COMPLETE — 4/4"
        : $"STEP 41.0 GAMESTARTUP ASYNC FRONTIER MAP {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
