namespace StS2Launcher.Core;

public sealed record RuntimeFrameworkBindingGateResult(
    RuntimeFrameworkBindingGate Gate,
    bool Passed,
    string Detail);
