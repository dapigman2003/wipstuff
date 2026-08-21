namespace StS2Launcher.Core;

public sealed record ControlledHarmonyConstructionSummary(
    IReadOnlyList<ControlledHarmonyConstructionGateResult> Results,
    int PassedGates,
    ControlledHarmonyConstructionGate? FirstFailingGate)
{
    public bool Passed => PassedGates == 9 && FirstFailingGate is null;

    public string Summary => Passed
        ? "CONTROLLED HARMONY CONSTRUCTION BOUNDARY PASS — 9/9"
        : $"CONTROLLED HARMONY CONSTRUCTION BOUNDARY FAIL — {PassedGates}/9" +
          (FirstFailingGate is null ? string.Empty : $", first failure: {FirstFailingGate}");
}
