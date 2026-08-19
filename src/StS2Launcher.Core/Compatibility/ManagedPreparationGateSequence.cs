namespace StS2Launcher.Core;

public sealed class ManagedPreparationGateSequence
{
    private readonly List<ManagedPreparationGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<ManagedPreparationGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public ManagedPreparationGateResult Record(ManagedPreparationGate gate, bool passed, string detail)
    {
        var expected = (ManagedPreparationGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected managed-preparation gate {expected}, received {gate}.");
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed managed-preparation gate.");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException("Gate detail must be non-empty.", nameof(detail));

        var result = new ManagedPreparationGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        if (!passed)
            _failed = true;
        return result;
    }

    public ManagedPreparationSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new ManagedPreparationSummary(
            _results.ToArray(),
            Passed: _results.Count == 4 && firstFailure is null,
            FirstFailingGate: firstFailure);
    }
}
