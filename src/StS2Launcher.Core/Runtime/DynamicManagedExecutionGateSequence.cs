namespace StS2Launcher.Core;

public sealed class DynamicManagedExecutionGateSequence
{
    private readonly List<DynamicManagedExecutionGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<DynamicManagedExecutionGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public DynamicManagedExecutionGateResult Record(
        DynamicManagedExecutionGate gate,
        bool passed,
        string detail)
    {
        var expected = (DynamicManagedExecutionGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected dynamic-managed-execution gate {expected}, received {gate}.");
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed dynamic-managed-execution gate.");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException("Gate detail must be non-empty.", nameof(detail));

        var result = new DynamicManagedExecutionGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        if (!passed)
            _failed = true;
        return result;
    }

    public DynamicManagedExecutionSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new DynamicManagedExecutionSummary(
            _results.ToArray(),
            Passed: _results.Count == 4 && firstFailure is null,
            FirstFailingGate: firstFailure);
    }
}
