namespace StS2Launcher.Core;

public sealed record TransformedRealStS2RenderPulseSummary(
    IReadOnlyList<TransformedRealStS2RenderPulseGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public TransformedRealStS2RenderPulseGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "STEP 40.1 CONTROLLED REAL-GAME RENDER PULSE COMPLETE — 4/4"
        : $"STEP 40.1 CONTROLLED REAL-GAME RENDER PULSE {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
