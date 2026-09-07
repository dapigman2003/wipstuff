namespace StS2Launcher.Core;

public sealed record TransformedRealStS2GameLifecycleEntrySummary(
    IReadOnlyList<TransformedRealStS2GameLifecycleEntryGateResult> Gates)
{
    public bool Passed => Gates.Count == 4 && Gates.All(g => g.Passed);
    public TransformedRealStS2GameLifecycleEntryGate? FirstFailingGate => Gates.FirstOrDefault(g => !g.Passed)?.Gate;
    public string Summary => Passed
        ? "STEP 38.0 CONTROLLED NGAME _ENTERTREE ENTRY COMPLETE — 4/4"
        : $"STEP 38.0 CONTROLLED NGAME _ENTERTREE ENTRY {(FirstFailingGate is null ? "INCOMPLETE" : "FAIL")} — {Gates.Count(g => g.Passed)}/4";
}
