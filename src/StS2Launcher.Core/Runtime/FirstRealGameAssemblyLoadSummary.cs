namespace StS2Launcher.Core;

public sealed record FirstRealGameAssemblyLoadSummary(
    IReadOnlyList<FirstRealGameAssemblyLoadGateResult> Results,
    int PassedGates,
    FirstRealGameAssemblyLoadGate? FirstFailingGate)
{
    public bool Passed => Results.Count == 4 && PassedGates == 4 && FirstFailingGate is null;

    public string Summary => Passed
        ? "FIRST REAL STS2 CLR LOAD BOUNDARY PASS — 4/4"
        : FirstFailingGate is { } failed
            ? $"FIRST REAL STS2 CLR LOAD BOUNDARY FAIL — Gate {(char)('A' + (int)failed - 1)} ({failed})"
            : $"FIRST REAL STS2 CLR LOAD BOUNDARY — {PassedGates}/4 passed";
}
