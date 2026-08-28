namespace StS2Launcher.Core;

public sealed class TransformedRealStS2PrewarmJitExecutionGateSequence
{
    private readonly List<TransformedRealStS2PrewarmJitExecutionGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2PrewarmJitExecutionGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2PrewarmJitExecutionGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 34 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2PrewarmJitExecutionGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 34 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2PrewarmJitExecutionSummary Snapshot() => new(_results.ToArray());
}
