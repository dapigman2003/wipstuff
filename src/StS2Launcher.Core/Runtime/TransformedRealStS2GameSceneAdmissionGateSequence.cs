namespace StS2Launcher.Core;

public sealed class TransformedRealStS2GameSceneAdmissionGateSequence
{
    private readonly List<TransformedRealStS2GameSceneAdmissionGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2GameSceneAdmissionGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2GameSceneAdmissionGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 37.0.1 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2GameSceneAdmissionGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 37.0.1 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2GameSceneAdmissionSummary Snapshot() => new(_results.ToArray());
}
