namespace StS2Launcher.Core;

public sealed class CompatibilityCallSiteGateSequence
{
    private readonly List<CompatibilityCallSiteGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<CompatibilityCallSiteGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public CompatibilityCallSiteGateResult Record(CompatibilityCallSiteGate gate, bool passed, string detail)
    {
        var expected = (CompatibilityCallSiteGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected compatibility call-site gate {expected}, received {gate}.");
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed compatibility call-site gate.");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException("Gate detail must be non-empty.", nameof(detail));

        var result = new CompatibilityCallSiteGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        if (!passed)
            _failed = true;
        return result;
    }

    public CompatibilityCallSiteSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new CompatibilityCallSiteSummary(
            _results.ToArray(),
            Passed: _results.Count == 4 && firstFailure is null,
            FirstFailingGate: firstFailure);
    }
}
