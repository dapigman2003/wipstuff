namespace StS2Launcher.Core;

public sealed class TransformedRealStS2GameLifecycleEntryGateSequence
{
    private readonly List<TransformedRealStS2GameLifecycleEntryGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2GameLifecycleEntryGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2GameLifecycleEntryGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 38.0.1 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2GameLifecycleEntryGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 38.0.1 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2GameLifecycleEntrySummary Snapshot() => new(_results.ToArray());
}
