namespace StS2Launcher.Core;

public sealed class TransformedRealStS2GameStartupInitPoolsGateSequence
{
    private readonly List<TransformedRealStS2GameStartupInitPoolsGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2GameStartupInitPoolsGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2GameStartupInitPoolsGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 42.0 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2GameStartupInitPoolsGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 42.0 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2GameStartupInitPoolsSummary Snapshot() => new(_results.ToArray());
}
