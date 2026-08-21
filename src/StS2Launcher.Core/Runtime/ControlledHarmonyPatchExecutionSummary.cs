namespace StS2Launcher.Core;

public sealed record ControlledHarmonyPatchExecutionSummary(
    IReadOnlyList<ControlledHarmonyPatchExecutionGateResult> Results,
    int PassedGates,
    ControlledHarmonyPatchExecutionGate? FirstFailingGate)
{
    public bool Passed => PassedGates == 25 && FirstFailingGate is null;

    public string Summary => Passed
        ? "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY PASS — 25/25"
        : $"CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — {PassedGates}/25" +
          (FirstFailingGate is null ? string.Empty : $", first failure: {FirstFailingGate}");
}
