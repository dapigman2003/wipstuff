namespace StS2Launcher.Core;

public sealed record ExpressionInterpreterCompatibilityProgress(
    ExpressionInterpreterCompatibilityGate Gate,
    long ProcessedItems,
    long TotalItems,
    string? CurrentPath,
    string Detail);
