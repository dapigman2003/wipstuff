namespace StS2Launcher.Core;

public sealed record ControlledHarmonyProcessorCreationSummary(
    IReadOnlyList<ControlledHarmonyProcessorCreationGateResult> Results,
    int PassedGates,
    ControlledHarmonyProcessorCreationGate? FirstFailingGate)
{
    public bool Passed => PassedGates == 14 && FirstFailingGate is null;

    public string Summary => Passed
        ? "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY PASS — 14/14"
        : $"CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY FAIL — {PassedGates}/14" +
          (FirstFailingGate is null ? string.Empty : $", first failure: {FirstFailingGate}");
}
