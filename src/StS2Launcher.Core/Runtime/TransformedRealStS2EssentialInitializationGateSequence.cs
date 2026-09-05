namespace StS2Launcher.Core;

public sealed class TransformedRealStS2EssentialInitializationGateSequence
{
    private readonly List<TransformedRealStS2EssentialInitializationGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2EssentialInitializationGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2EssentialInitializationGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 36.0 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2EssentialInitializationGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 36.0 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2EssentialInitializationSummary Snapshot() => new(_results.ToArray());
}
