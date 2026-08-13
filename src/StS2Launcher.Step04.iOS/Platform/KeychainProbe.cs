using StS2Launcher.Core;

namespace StS2Launcher.Step04.iOS.Platform;

public sealed record KeychainProbeResult(
    bool Passed,
    int PassedChecks,
    int TotalChecks,
    string Summary);

public sealed class KeychainProbe
{
    public const string TestKey = "step04-device-test";
    public const string AlphaValue = "STEP04-ALPHA";
    public const string BetaValue = "STEP04-BETA";

    private readonly ICredentialStore _store;

    public KeychainProbe(ICredentialStore store)
    {
        _store = store;
    }

    public string? ReadPersistedValue() => _store.Get(TestKey);

    public KeychainProbeResult RunRoundTrip()
    {
        var passed = 0;
        const int total = 6;

        // 1. Clean starting point.
        _store.Delete(TestKey);
        if (_store.Get(TestKey) is null)
            passed++;

        // 2. First write.
        _store.Set(TestKey, AlphaValue);
        if (_store.Get(TestKey) == AlphaValue)
            passed++;

        // 3. Verify it exists through another query.
        if (_store.Get(TestKey) is not null)
            passed++;

        // 4. Overwrite using the same logical key.
        _store.Set(TestKey, BetaValue);
        if (_store.Get(TestKey) == BetaValue)
            passed++;

        // 5. Verify the old value is no longer returned.
        if (_store.Get(TestKey) != AlphaValue)
            passed++;

        // 6. Deliberately leave Beta in Keychain for the restart test.
        if (_store.Get(TestKey) == BetaValue)
            passed++;

        return new KeychainProbeResult(
            Passed: passed == total,
            PassedChecks: passed,
            TotalChecks: total,
            Summary: passed == total
                ? $"KEYCHAIN ROUND-TRIP PASS — {passed}/{total}"
                : $"KEYCHAIN ROUND-TRIP FAIL — {passed}/{total}");
    }

    public bool DeleteTestValue()
    {
        _store.Delete(TestKey);
        return _store.Get(TestKey) is null;
    }
}
