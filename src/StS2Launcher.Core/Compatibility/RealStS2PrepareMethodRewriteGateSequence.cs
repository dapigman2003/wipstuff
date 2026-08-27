namespace StS2Launcher.Core;

public sealed class RealStS2PrepareMethodRewriteGateSequence
{
    private readonly List<RealStS2PrepareMethodRewriteGateResult> _results = [];
    public IReadOnlyList<RealStS2PrepareMethodRewriteGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(RealStS2PrepareMethodRewriteGate gate, bool passed, string detail)
    {
        if (_results.Any(result => !result.Passed))
            throw new InvalidOperationException("Step 32 cannot advance after a failed gate.");
        var expected = (RealStS2PrepareMethodRewriteGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected Step 32 gate {expected}, received {gate}.");
        _results.Add(new RealStS2PrepareMethodRewriteGateResult(gate, passed, detail));
    }

    public RealStS2PrepareMethodRewriteSummary Snapshot() => new(_results.ToArray());
}
