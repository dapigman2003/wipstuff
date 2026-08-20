namespace StS2Launcher.Core;

public sealed class ControlledManagedInitializationGateSequence
{
    private readonly List<ControlledManagedInitializationGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<ControlledManagedInitializationGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public ControlledManagedInitializationGateResult Record(ControlledManagedInitializationGateResult result)
    {
        if (_failed)
            throw new InvalidOperationException("A later Step 24 gate cannot run after an earlier failure.");

        var expected = (ControlledManagedInitializationGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 24 gate {expected}, received {result.Gate}.");

        var normalized = result with { Detail = result.Detail.Trim() };
        _results.Add(normalized);
        if (!normalized.Passed)
            _failed = true;
        return normalized;
    }

    public ControlledManagedInitializationSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new ControlledManagedInitializationSummary(
            _results.ToArray(),
            _results.Count(result => result.Passed),
            firstFailure);
    }
}
