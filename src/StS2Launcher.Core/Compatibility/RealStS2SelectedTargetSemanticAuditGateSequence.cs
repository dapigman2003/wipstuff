namespace StS2Launcher.Core;

public sealed class RealStS2SelectedTargetSemanticAuditGateSequence
{
    private readonly List<RealStS2SelectedTargetSemanticAuditGateResult> _results = [];
    public IReadOnlyList<RealStS2SelectedTargetSemanticAuditGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public RealStS2SelectedTargetSemanticAuditGateResult Record(
        RealStS2SelectedTargetSemanticAuditGate gate,
        bool passed,
        string detail)
    {
        if (_results.Any(result => !result.Passed))
            throw new InvalidOperationException("Step 30 cannot advance after a failed gate.");
        var expected = (RealStS2SelectedTargetSemanticAuditGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected Step 30 gate {expected}, received {gate}.");
        var result = new RealStS2SelectedTargetSemanticAuditGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        return result;
    }

    public RealStS2SelectedTargetSemanticAuditSummary Snapshot()
    {
        var firstFail = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new RealStS2SelectedTargetSemanticAuditSummary(
            _results.ToArray(),
            _results.Count == 4 && _results.All(result => result.Passed),
            firstFail);
    }
}
