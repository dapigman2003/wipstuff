namespace StS2Launcher.Core;

public sealed record ExpressionInterpreterCompatibilitySummary(
    IReadOnlyList<ExpressionInterpreterCompatibilityGateResult> Results,
    bool Passed,
    ExpressionInterpreterCompatibilityGate? FirstFailingGate)
{
    public int PassedGates => Results.Count(result => result.Passed);

    public string Summary => Passed
        ? $"EXPRESSION INTERPRETER COMPATIBILITY PASS — {PassedGates}/4"
        : FirstFailingGate is null
            ? $"EXPRESSION INTERPRETER COMPATIBILITY IN PROGRESS — {PassedGates}/4"
            : $"EXPRESSION INTERPRETER COMPATIBILITY FAIL — Gate {(char)('A' + (int)FirstFailingGate.Value - 1)} ({FirstFailingGate})";
}
