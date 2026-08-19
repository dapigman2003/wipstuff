namespace StS2Launcher.Core;

public sealed class RealAssemblyRewriteGateSequence
{
    private readonly List<RealAssemblyRewriteGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<RealAssemblyRewriteGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public RealAssemblyRewriteGateResult Record(RealAssemblyRewriteGate gate, bool passed, string detail)
    {
        var expected = (RealAssemblyRewriteGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected real-assembly rewrite gate {expected}, received {gate}.");
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed real-assembly rewrite gate.");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException("Gate detail must be non-empty.", nameof(detail));

        var result = new RealAssemblyRewriteGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        if (!passed)
            _failed = true;
        return result;
    }

    public RealAssemblyRewriteSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new RealAssemblyRewriteSummary(
            _results.ToArray(),
            Passed: _results.Count == 4 && firstFailure is null,
            FirstFailingGate: firstFailure);
    }
}
