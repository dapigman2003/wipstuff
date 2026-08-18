namespace StS2Launcher.Core;

public sealed class ExpressionInterpreterCompatibilityGateSequence
{
    private readonly List<ExpressionInterpreterCompatibilityGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<ExpressionInterpreterCompatibilityGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public ExpressionInterpreterCompatibilityGateResult Record(
        ExpressionInterpreterCompatibilityGate gate,
        bool passed,
        string detail)
    {
        var expected = (ExpressionInterpreterCompatibilityGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected expression-interpreter compatibility gate {expected}, received {gate}.");
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed expression-interpreter compatibility gate.");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException("Gate detail must be non-empty.", nameof(detail));

        var result = new ExpressionInterpreterCompatibilityGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        if (!passed)
            _failed = true;
        return result;
    }

    public ExpressionInterpreterCompatibilitySummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new ExpressionInterpreterCompatibilitySummary(
            _results.ToArray(),
            Passed: _results.Count == 4 && firstFailure is null,
            FirstFailingGate: firstFailure);
    }
}
