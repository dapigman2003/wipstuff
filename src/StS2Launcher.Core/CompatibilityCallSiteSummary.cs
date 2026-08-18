namespace StS2Launcher.Core;

public sealed record CompatibilityCallSiteSummary(
    IReadOnlyList<CompatibilityCallSiteGateResult> Results,
    bool Passed,
    CompatibilityCallSiteGate? FirstFailingGate)
{
    public int PassedGates => Results.Count(result => result.Passed);

    public string Summary => Passed
        ? $"COMPATIBILITY CALL-SITE ANALYSIS PASS — {PassedGates}/4"
        : FirstFailingGate is null
            ? $"COMPATIBILITY CALL-SITE ANALYSIS IN PROGRESS — {PassedGates}/4"
            : $"COMPATIBILITY CALL-SITE ANALYSIS FAIL — Gate {(char)('A' + (int)FirstFailingGate.Value - 1)} ({FirstFailingGate})";
}
