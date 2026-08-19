namespace StS2Launcher.Core;

public sealed record DynamicManagedExecutionSummary(
    IReadOnlyList<DynamicManagedExecutionGateResult> Results,
    bool Passed,
    DynamicManagedExecutionGate? FirstFailingGate)
{
    public int PassedGates => Results.Count(result => result.Passed);

    public string Summary => Passed
        ? $"DYNAMIC MANAGED EXECUTION FOUNDATION PASS — {PassedGates}/4"
        : FirstFailingGate is null
            ? $"DYNAMIC MANAGED EXECUTION FOUNDATION IN PROGRESS — {PassedGates}/4"
            : $"DYNAMIC MANAGED EXECUTION FOUNDATION FAIL — Gate {(char)('A' + (int)FirstFailingGate.Value - 1)} ({FirstFailingGate})";
}
