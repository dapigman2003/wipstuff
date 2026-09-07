namespace StS2Launcher.Core;

public sealed record TransformedRealStS2GameLifecycleEntryGateResult(
    TransformedRealStS2GameLifecycleEntryGate Gate,
    bool Passed,
    string Detail);
