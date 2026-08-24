namespace StS2Launcher.Core;

public sealed class AheadOfLoadManagedTransformationGateSequence
{
    private readonly List<AheadOfLoadManagedTransformationGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<AheadOfLoadManagedTransformationGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public AheadOfLoadManagedTransformationGateResult Record(
        AheadOfLoadManagedTransformationGate gate,
        bool passed,
        string detail)
    {
        var expected = (AheadOfLoadManagedTransformationGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected ahead-of-load transformation gate {expected}, received {gate}.");
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed ahead-of-load transformation gate.");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException("Gate detail must be non-empty.", nameof(detail));

        var result = new AheadOfLoadManagedTransformationGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        if (!passed)
            _failed = true;
        return result;
    }

    public AheadOfLoadManagedTransformationSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new AheadOfLoadManagedTransformationSummary(
            _results.ToArray(),
            Passed: _results.Count == 5 && firstFailure is null,
            FirstFailingGate: firstFailure);
    }
}
