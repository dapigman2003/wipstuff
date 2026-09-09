namespace StS2Launcher.Core;

public sealed class TransformedRealStS2StartupLadderGateSequence
{
    private readonly int _step;
    private readonly string _stepName;
    private readonly List<TransformedRealStS2StartupLadderGateResult> _results = [];

    public TransformedRealStS2StartupLadderGateSequence(int step, string stepName)
    {
        _step = step;
        _stepName = stepName;
    }

    public IReadOnlyList<TransformedRealStS2StartupLadderGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2StartupLadderGateResult result)
    {
        if (result.Step != _step || !string.Equals(result.StepName, _stepName, StringComparison.Ordinal))
            throw new InvalidOperationException($"Step {_step}.0 gate sequence received a result for Step {result.Step}.0 / {result.StepName}.");
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException($"Step {_step}.0 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2StartupLadderGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step {_step}.0 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2StartupLadderSummary Snapshot()
        => new(_step, _stepName, _results.ToArray());
}
