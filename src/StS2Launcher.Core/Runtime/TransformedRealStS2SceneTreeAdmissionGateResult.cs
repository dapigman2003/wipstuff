namespace StS2Launcher.Core;

public sealed record TransformedRealStS2SceneTreeAdmissionGateResult(
    TransformedRealStS2SceneTreeAdmissionGate Gate,
    bool Passed,
    string Detail);
