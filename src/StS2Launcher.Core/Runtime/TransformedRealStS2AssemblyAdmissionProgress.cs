namespace StS2Launcher.Core;

public sealed record TransformedRealStS2AssemblyAdmissionProgress(
    TransformedRealStS2AssemblyAdmissionGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
