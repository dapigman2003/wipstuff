namespace StS2Launcher.Core;

public sealed record TransformedRealStS2VeryEarlyInitializationSummary(
    IReadOnlyList<TransformedRealStS2VeryEarlyInitializationGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public TransformedRealStS2VeryEarlyInitializationGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "STEP 35.0.19 DIAGNOSTIC LOCALIZATION COMPLETE — 4/4 — NOT STEP 35 CLOSURE"
        : $"STEP 35.0.19 DIAGNOSTIC LOCALIZATION {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
