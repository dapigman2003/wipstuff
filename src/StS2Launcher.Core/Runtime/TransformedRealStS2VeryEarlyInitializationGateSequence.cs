namespace StS2Launcher.Core;

public sealed class TransformedRealStS2VeryEarlyInitializationGateSequence
{
    private readonly List<TransformedRealStS2VeryEarlyInitializationGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2VeryEarlyInitializationGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2VeryEarlyInitializationGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 35.0.30 diagnostic localization cannot advance after a failed gate.");
        var expected = (TransformedRealStS2VeryEarlyInitializationGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 35.0.30 diagnostic gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2VeryEarlyInitializationSummary Snapshot() => new(_results.ToArray());
}
