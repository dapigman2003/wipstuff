namespace StS2Launcher.Core;

public sealed class RealStS2CompatibilityTargetAuditGateSequence
{
    private readonly List<RealStS2CompatibilityTargetAuditGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<RealStS2CompatibilityTargetAuditGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public RealStS2CompatibilityTargetAuditGateResult Record(
        RealStS2CompatibilityTargetAuditGate gate,
        bool passed,
        string detail)
    {
        var expected = (RealStS2CompatibilityTargetAuditGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected real-StS2 target-audit gate {expected}, received {gate}.");
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed real-StS2 target-audit gate.");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException("Gate detail must be non-empty.", nameof(detail));

        var result = new RealStS2CompatibilityTargetAuditGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        if (!passed)
            _failed = true;
        return result;
    }

    public RealStS2CompatibilityTargetAuditSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new RealStS2CompatibilityTargetAuditSummary(
            _results.ToArray(),
            Passed: _results.Count == 4 && firstFailure is null,
            FirstFailingGate: firstFailure);
    }
}
