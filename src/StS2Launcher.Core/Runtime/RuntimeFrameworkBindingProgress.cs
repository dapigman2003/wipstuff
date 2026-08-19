namespace StS2Launcher.Core;

public sealed record RuntimeFrameworkBindingProgress(
    RuntimeFrameworkBindingGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
