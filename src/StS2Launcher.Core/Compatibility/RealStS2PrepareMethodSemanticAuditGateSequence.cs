namespace StS2Launcher.Core;

public sealed class RealStS2PrepareMethodSemanticAuditGateSequence
{
    private readonly List<RealStS2PrepareMethodSemanticAuditGateResult> _results = [];
    public IReadOnlyList<RealStS2PrepareMethodSemanticAuditGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public RealStS2PrepareMethodSemanticAuditGateResult Record(
        RealStS2PrepareMethodSemanticAuditGate gate,
        bool passed,
        string detail)
    {
        if (_results.Any(result => !result.Passed))
            throw new InvalidOperationException("Step 31 cannot advance after a failed gate.");
        var expected = (RealStS2PrepareMethodSemanticAuditGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected Step 31 gate {expected}, received {gate}.");
        var result = new RealStS2PrepareMethodSemanticAuditGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        return result;
    }

    public RealStS2PrepareMethodSemanticAuditSummary Snapshot()
    {
        var firstFail = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new RealStS2PrepareMethodSemanticAuditSummary(
            _results.ToArray(),
            _results.Count == 4 && _results.All(result => result.Passed),
            firstFail);
    }
}
