namespace StS2Launcher.Core;

public sealed record TransformedRealStS2GameSceneAdmissionSummary(
    IReadOnlyList<TransformedRealStS2GameSceneAdmissionGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public TransformedRealStS2GameSceneAdmissionGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "STEP 37.0.1 CONTROLLED GAME-SCENE ADMISSION COMPLETE — 4/4"
        : $"STEP 37.0.1 CONTROLLED GAME-SCENE ADMISSION {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
