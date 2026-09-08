namespace StS2Launcher.Core;

public sealed class TransformedRealStS2SceneTreeAdmissionGateSequence
{
    private readonly List<TransformedRealStS2SceneTreeAdmissionGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2SceneTreeAdmissionGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2SceneTreeAdmissionGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 39.0 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2SceneTreeAdmissionGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 39.0 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2SceneTreeAdmissionSummary Snapshot() => new(_results.ToArray());
}
