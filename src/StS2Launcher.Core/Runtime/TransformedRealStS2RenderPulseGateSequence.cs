namespace StS2Launcher.Core;

public sealed class TransformedRealStS2RenderPulseGateSequence
{
    private readonly List<TransformedRealStS2RenderPulseGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2RenderPulseGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2RenderPulseGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 40.0 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2RenderPulseGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 40.0 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2RenderPulseSummary Snapshot() => new(_results.ToArray());
}
