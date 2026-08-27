namespace StS2Launcher.Core;

public enum HostFrameworkClosureGate
{
    RootedHostAvailability = 1,
    BindingClosureRecompute = 2,
    HostOnlyFrameworkPreparedSet = 3,
    IsolationAudit = 4,
}

public sealed record HostFrameworkClosureGateResult(HostFrameworkClosureGate Gate, bool Passed, string Detail);

public sealed record HostFrameworkClosureGateSnapshot(int PassedCount, HostFrameworkClosureGate? FirstFailingGate, string Summary);

public sealed class HostFrameworkClosureGateSequence
{
    private readonly List<HostFrameworkClosureGateResult> _results = new();
    public IReadOnlyList<HostFrameworkClosureGateResult> Results => _results;
    public void Reset() => _results.Clear();
    public void Record(HostFrameworkClosureGateResult result) => _results.Add(result);
    public HostFrameworkClosureGateSnapshot Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        var passed = _results.Count(result => result.Passed);
        var summary = firstFailure is null && passed == 4
            ? "HOST FRAMEWORK CLOSURE FOUNDATION PASS — 4/4"
            : $"HOST FRAMEWORK CLOSURE FOUNDATION — {passed}/4" + (firstFailure is null ? string.Empty : $" — FIRST FAIL: {firstFailure}");
        return new HostFrameworkClosureGateSnapshot(passed, firstFailure, summary);
    }
}
