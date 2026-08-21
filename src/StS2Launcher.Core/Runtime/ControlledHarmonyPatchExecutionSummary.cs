namespace StS2Launcher.Core;

public sealed record ControlledHarmonyPatchExecutionSummary(
    IReadOnlyList<ControlledHarmonyPatchExecutionGateResult> Results,
    int PassedGates,
    ControlledHarmonyPatchExecutionGate? FirstFailingGate)
{
    public bool Passed => PassedGates == 26 && FirstFailingGate is null;

    public string Summary => Passed
        ? "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY PASS — 26/26"
        : $"CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — {PassedGates}/26" +
          (FirstFailingGate is null ? string.Empty : $", first failure: {FirstFailingGate}");
}
