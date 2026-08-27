namespace StS2Launcher.Core;

public sealed record RuntimeFrameworkBindingSummary(
    IReadOnlyList<RuntimeFrameworkBindingGateResult> Results,
    int PassedGates,
    RuntimeFrameworkBindingGate? FirstFailingGate)
{
    public bool Passed => Results.Count == 4 && FirstFailingGate is null;

    public string Summary => Passed
        ? "PREPARED RUNTIME / FRAMEWORK BINDING PASS — 4/4"
        : FirstFailingGate is null
            ? $"PREPARED RUNTIME / FRAMEWORK BINDING — {PassedGates}/4 gates passed"
            : $"PREPARED RUNTIME / FRAMEWORK BINDING FAIL — Gate {(char)('A' + (int)FirstFailingGate.Value - 1)} ({FirstFailingGate})";
}
