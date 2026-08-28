namespace StS2Launcher.Core;

public sealed record TransformedRealStS2PrewarmJitExecutionGateResult(
    TransformedRealStS2PrewarmJitExecutionGate Gate,
    bool Passed,
    string Detail);
