namespace StS2Launcher.Core;

public sealed record ManagedPreparationSummary(
    IReadOnlyList<ManagedPreparationGateResult> Results,
    bool Passed,
    ManagedPreparationGate? FirstFailingGate)
{
    public int PassedGates => Results.Count(result => result.Passed);

    public string Summary => Passed
        ? $"MANAGED PREPARATION PASS — {PassedGates}/4"
        : FirstFailingGate is null
            ? $"MANAGED PREPARATION IN PROGRESS — {PassedGates}/4"
            : $"MANAGED PREPARATION FAIL — Gate {(char)('A' + (int)FirstFailingGate.Value - 1)} ({FirstFailingGate})";
}
