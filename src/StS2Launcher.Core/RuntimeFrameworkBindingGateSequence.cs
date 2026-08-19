namespace StS2Launcher.Core;

public sealed class RuntimeFrameworkBindingGateSequence
{
    private readonly List<RuntimeFrameworkBindingGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<RuntimeFrameworkBindingGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public RuntimeFrameworkBindingGateResult Record(
        RuntimeFrameworkBindingGate gate,
        bool passed,
        string detail)
    {
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed runtime/framework-binding gate.");

        var expected = (RuntimeFrameworkBindingGate)(_results.Count + 1);
        if (gate != expected)
            throw new InvalidOperationException($"Expected runtime/framework-binding gate {expected}, received {gate}.");

        var result = new RuntimeFrameworkBindingGateResult(gate, passed, detail.Trim());
        _results.Add(result);
        if (!passed)
            _failed = true;
        return result;
    }

    public RuntimeFrameworkBindingSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new RuntimeFrameworkBindingSummary(
            _results.ToArray(),
            _results.Count(result => result.Passed),
            firstFailure);
    }
}
