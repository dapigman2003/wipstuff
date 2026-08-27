namespace StS2Launcher.Core;

public sealed record ControlledManagedInitializationSummary(
    IReadOnlyList<ControlledManagedInitializationGateResult> Results,
    int PassedGates,
    ControlledManagedInitializationGate? FirstFailingGate)
{
    public bool Passed => PassedGates == 4 && FirstFailingGate is null;

    public string Summary => Passed
        ? "CONTROLLED MANAGED INITIALIZATION BOUNDARY PASS — 4/4"
        : FirstFailingGate is null
            ? $"CONTROLLED MANAGED INITIALIZATION BOUNDARY — {PassedGates}/4"
            : $"CONTROLLED MANAGED INITIALIZATION BOUNDARY FAIL — {PassedGates}/4, first failure: {FirstFailingGate}";
}
