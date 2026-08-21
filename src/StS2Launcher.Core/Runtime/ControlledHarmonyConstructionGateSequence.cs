namespace StS2Launcher.Core;

public sealed class ControlledHarmonyConstructionGateSequence
{
    private readonly List<ControlledHarmonyConstructionGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<ControlledHarmonyConstructionGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public ControlledHarmonyConstructionGateResult Record(ControlledHarmonyConstructionGateResult result)
    {
        if (_failed)
            throw new InvalidOperationException("A later Step 25 gate cannot run after an earlier failure.");

        var expected = (ControlledHarmonyConstructionGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 25 gate {expected}, received {result.Gate}.");

        var normalized = result with { Detail = result.Detail.Trim() };
        _results.Add(normalized);
        if (!normalized.Passed)
            _failed = true;
        return normalized;
    }

    public ControlledHarmonyConstructionSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new ControlledHarmonyConstructionSummary(
            _results.ToArray(),
            _results.Count(result => result.Passed),
            firstFailure);
    }
}
