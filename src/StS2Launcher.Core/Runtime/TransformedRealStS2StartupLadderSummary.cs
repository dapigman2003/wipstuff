namespace StS2Launcher.Core;

public sealed record TransformedRealStS2StartupLadderSummary(
    int Step,
    string StepName,
    IReadOnlyList<TransformedRealStS2StartupLadderGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(gate => gate.Passed);

    public string Summary => Passed
        ? $"STEP {Step}.0 {StepName.ToUpperInvariant()} COMPLETE — 4/4"
        : $"STEP {Step}.0 {StepName.ToUpperInvariant()} {(Gates.Any(gate => !gate.Passed) ? "FAIL" : "INCOMPLETE")} — {Gates.Count(gate => gate.Passed)}/4";
}
