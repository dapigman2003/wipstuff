namespace StS2Launcher.Core;

public sealed class ControlledHarmonyProcessorCreationGateSequence
{
    private readonly List<ControlledHarmonyProcessorCreationGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<ControlledHarmonyProcessorCreationGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public ControlledHarmonyProcessorCreationGateResult Record(ControlledHarmonyProcessorCreationGateResult result)
    {
        if (_failed)
            throw new InvalidOperationException("A later Step 26 gate cannot run after an earlier failure.");

        var expected = (ControlledHarmonyProcessorCreationGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 26 gate {expected}, received {result.Gate}.");

        var normalized = result with { Detail = result.Detail.Trim() };
        _results.Add(normalized);
        if (!normalized.Passed)
            _failed = true;
        return normalized;
    }

    public ControlledHarmonyProcessorCreationSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new ControlledHarmonyProcessorCreationSummary(
            _results.ToArray(),
            _results.Count(result => result.Passed),
            firstFailure);
    }
}
