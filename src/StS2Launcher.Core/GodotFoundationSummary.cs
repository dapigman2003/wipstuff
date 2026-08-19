namespace StS2Launcher.Core;

public sealed record GodotFoundationSummary(
    IReadOnlyList<GodotFoundationGateResult> Results,
    bool Passed,
    GodotFoundationGate? FirstFailingGate)
{
    public int PassedGates => Results.Count(value => value.Passed);

    public string Summary => Passed
        ? $"GODOT FOUNDATION PASS — {PassedGates}/4"
        : FirstFailingGate is null
            ? $"GODOT FOUNDATION IN PROGRESS — {PassedGates}/4"
            : $"GODOT FOUNDATION FAIL — Gate {(char)('A' + (int)FirstFailingGate.Value - 1)} ({FirstFailingGate})";
}
