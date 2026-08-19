namespace StS2Launcher.Core;

public sealed record CompatibilityCallSiteGateResult(
    CompatibilityCallSiteGate Gate,
    bool Passed,
    string Detail);
