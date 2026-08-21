namespace StS2Launcher.Core;

public sealed class ControlledHarmonyPatchExecutionGateSequence
{
    private readonly List<ControlledHarmonyPatchExecutionGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<ControlledHarmonyPatchExecutionGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public ControlledHarmonyPatchExecutionGateResult Record(ControlledHarmonyPatchExecutionGateResult result)
    {
        if (_failed)
            throw new InvalidOperationException("A later Step 27 gate cannot run after an earlier failure.");

        var expected = (ControlledHarmonyPatchExecutionGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 27 gate {expected}, received {result.Gate}.");

        var normalized = result with { Detail = result.Detail.Trim() };
        _results.Add(normalized);
        if (!normalized.Passed)
            _failed = true;
        return normalized;
    }

    public ControlledHarmonyPatchExecutionSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new ControlledHarmonyPatchExecutionSummary(
            _results.ToArray(),
            _results.Count(result => result.Passed),
            firstFailure);
    }
}
