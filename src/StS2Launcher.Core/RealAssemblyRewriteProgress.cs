namespace StS2Launcher.Core;

public sealed record RealAssemblyRewriteProgress(
    RealAssemblyRewriteGate Gate,
    long ProcessedItems,
    long TotalItems,
    string? CurrentPath,
    string Detail);
