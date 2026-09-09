namespace StS2Launcher.Core;

public sealed class TransformedRealStS2GameStartupFrontierGateSequence
{
    private readonly List<TransformedRealStS2GameStartupFrontierGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2GameStartupFrontierGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2GameStartupFrontierGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 41.0 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2GameStartupFrontierGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 41.0 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2GameStartupFrontierSummary Snapshot() => new(_results.ToArray());
}
