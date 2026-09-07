namespace StS2Launcher.Core;

public sealed record TransformedRealStS2GameSceneAdmissionGateResult(
    TransformedRealStS2GameSceneAdmissionGate Gate,
    bool Passed,
    string Detail);
