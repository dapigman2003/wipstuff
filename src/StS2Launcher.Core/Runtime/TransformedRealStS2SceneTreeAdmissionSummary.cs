namespace StS2Launcher.Core;

public sealed record TransformedRealStS2SceneTreeAdmissionSummary(
    IReadOnlyList<TransformedRealStS2SceneTreeAdmissionGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public TransformedRealStS2SceneTreeAdmissionGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "STEP 39.0 REAL SCENETREE ADMISSION COMPLETE — 4/4"
        : $"STEP 39.0 REAL SCENETREE ADMISSION {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
