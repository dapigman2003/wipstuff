namespace StS2Launcher.Core;

public sealed record FirstRealGameAssemblyLoadGateResult(
    FirstRealGameAssemblyLoadGate Gate,
    bool Passed,
    string Detail);
