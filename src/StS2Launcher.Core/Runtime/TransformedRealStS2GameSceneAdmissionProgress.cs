namespace StS2Launcher.Core;

public sealed record TransformedRealStS2GameSceneAdmissionProgress(
    TransformedRealStS2GameSceneAdmissionGate Gate,
    int ProcessedItems,
    int TotalItems,
    string? CurrentPath,
    string Detail);
