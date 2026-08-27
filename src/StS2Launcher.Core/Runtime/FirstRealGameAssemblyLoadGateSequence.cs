namespace StS2Launcher.Core;

public sealed class FirstRealGameAssemblyLoadGateSequence
{
    private readonly List<FirstRealGameAssemblyLoadGateResult> _results = [];
    private bool _failed;

    public IReadOnlyList<FirstRealGameAssemblyLoadGateResult> Results => _results;

    public void Reset()
    {
        _results.Clear();
        _failed = false;
    }

    public FirstRealGameAssemblyLoadGateResult Record(FirstRealGameAssemblyLoadGateResult result)
    {
        if (_failed)
            throw new InvalidOperationException("Cannot advance after the first failed Step 23 gate.");

        var expected = (FirstRealGameAssemblyLoadGate)(_results.Count + 1);
        if (result.Gate != expected)
            throw new InvalidOperationException($"Expected Step 23 gate {expected}, received {result.Gate}.");

        var normalized = result with { Detail = result.Detail.Trim() };
        _results.Add(normalized);
        if (!normalized.Passed)
            _failed = true;
        return normalized;
    }

    public FirstRealGameAssemblyLoadSummary Snapshot()
    {
        var firstFailure = _results.FirstOrDefault(result => !result.Passed)?.Gate;
        return new FirstRealGameAssemblyLoadSummary(
            _results.ToArray(),
            _results.Count(result => result.Passed),
            firstFailure);
    }
}
