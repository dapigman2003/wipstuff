namespace StS2Launcher.Core;

public sealed class TransformedRealStS2AssemblyAdmissionGateSequence
{
    private readonly List<TransformedRealStS2AssemblyAdmissionGateResult> _results = [];
    public IReadOnlyList<TransformedRealStS2AssemblyAdmissionGateResult> Results => _results;

    public void Reset() => _results.Clear();

    public void Record(TransformedRealStS2AssemblyAdmissionGateResult result)
    {
        if (_results.Any(item => !item.Passed))
            throw new InvalidOperationException("Step 33 cannot advance after a failed gate.");
        var expected = (TransformedRealStS2AssemblyAdmissionGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 33 gate {expected}, received {result.Gate}.");
        _results.Add(result);
    }

    public TransformedRealStS2AssemblyAdmissionSummary Snapshot() => new(_results.ToArray());
}
