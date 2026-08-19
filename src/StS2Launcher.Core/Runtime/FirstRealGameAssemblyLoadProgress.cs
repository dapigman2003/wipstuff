namespace StS2Launcher.Core;

public sealed record FirstRealGameAssemblyLoadProgress(
    FirstRealGameAssemblyLoadGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
