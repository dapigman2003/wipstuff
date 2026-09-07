namespace StS2Launcher.Core;

public sealed record TransformedRealStS2GameLifecycleEntryProgress(
    TransformedRealStS2GameLifecycleEntryGate Gate,
    int Completed,
    int Total,
    string Detail);
