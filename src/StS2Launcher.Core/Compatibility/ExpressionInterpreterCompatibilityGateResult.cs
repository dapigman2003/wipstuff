namespace StS2Launcher.Core;

public sealed record ExpressionInterpreterCompatibilityGateResult(
    ExpressionInterpreterCompatibilityGate Gate,
    bool Passed,
    string Detail);
