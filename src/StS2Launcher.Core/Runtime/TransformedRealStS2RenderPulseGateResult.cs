namespace StS2Launcher.Core;

public sealed record TransformedRealStS2RenderPulseGateResult(
    TransformedRealStS2RenderPulseGate Gate,
    bool Passed,
    string Detail);
