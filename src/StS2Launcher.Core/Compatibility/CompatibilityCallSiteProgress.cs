namespace StS2Launcher.Core;

public sealed record CompatibilityCallSiteProgress(
    CompatibilityCallSiteGate Gate,
    long ProcessedItems,
    long TotalItems,
    string? CurrentPath,
    string Detail);
