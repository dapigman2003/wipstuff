namespace StS2Launcher.Core;

public sealed class GodotFoundationGateSequence
{
    private readonly List<GodotFoundationGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<GodotFoundationGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public GodotFoundationGateResult Record(GodotFoundationGate gate, bool passed, string detail)
    {
        var expected = (GodotFoundationGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected Godot foundation gate {expected}, received {gate}.");
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed Godot foundation gate.");
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException("Gate detail must be non-empty.", nameof(detail));

        var result = new GodotFoundationGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        if (!passed)
            _failed = true;
        return result;
    }

    public GodotFoundationSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(value => !value.Passed)?.Gate;
        return new GodotFoundationSummary(
            _results.ToArray(),
            Passed: _results.Count == 4 && firstFailure is null,
            FirstFailingGate: firstFailure);
    }
}
