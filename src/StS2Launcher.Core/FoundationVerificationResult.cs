namespace StS2Launcher.Core;

/// <summary>
/// Compact final verification state for the launcher foundation completed by
/// Steps 01-05. UIKit startup/lifecycle are device gates; Core and credential
/// behavior are also covered by host unit tests; Steam remains a physical-device
/// integration gate.
/// </summary>
public sealed record FoundationVerificationResult(
    bool UiStartupPassed,
    bool LifecycleActive,
    CoreSelfTestResult Core,
    CredentialStoreVerificationResult CredentialStore,
    SteamConnectionProbeResult Steam)
{
    public const int TotalGates = 5;

    public int PassedGates =>
        (UiStartupPassed ? 1 : 0) +
        (LifecycleActive ? 1 : 0) +
        (Core.Passed ? 1 : 0) +
        (CredentialStore.Passed ? 1 : 0) +
        (Steam.Passed ? 1 : 0);

    public bool Passed => PassedGates == TotalGates;

    public string Summary => Passed
        ? $"FOUNDATION PASS — {PassedGates}/{TotalGates}"
        : $"FOUNDATION FAIL — {PassedGates}/{TotalGates}";
}
