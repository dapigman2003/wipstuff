namespace StS2Launcher.Core;

public sealed record AheadOfLoadManagedTransformationSummary(
    IReadOnlyList<AheadOfLoadManagedTransformationGateResult> Gates,
    bool Passed,
    AheadOfLoadManagedTransformationGate? FirstFailingGate)
{
    public string Summary => Passed
        ? "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY PASS — 5/5"
        : $"AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY FAIL — {Gates.Count}/5" +
          (FirstFailingGate is null ? string.Empty : $", first failure: {FirstFailingGate}");
}
