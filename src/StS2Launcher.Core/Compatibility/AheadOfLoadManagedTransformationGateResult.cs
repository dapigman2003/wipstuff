namespace StS2Launcher.Core;

public sealed record AheadOfLoadManagedTransformationGateResult(
    AheadOfLoadManagedTransformationGate Gate,
    bool Passed,
    string Detail);
