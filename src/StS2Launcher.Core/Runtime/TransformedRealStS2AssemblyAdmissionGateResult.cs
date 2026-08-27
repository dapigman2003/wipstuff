namespace StS2Launcher.Core;

public sealed record TransformedRealStS2AssemblyAdmissionGateResult(
    TransformedRealStS2AssemblyAdmissionGate Gate,
    bool Passed,
    string Detail);
