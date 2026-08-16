namespace StS2Launcher.Core;

public sealed record CredentialStoreVerificationResult(
    bool Passed,
    int PassedChecks,
    int TotalChecks,
    string Summary);

/// <summary>
/// Platform-neutral round-trip verification for an <see cref="ICredentialStore"/>.
/// The verifier intentionally uses harmless caller-supplied sentinel values and
/// leaves the store clean when it completes.
/// </summary>
public static class CredentialStoreVerifier
{
    public static CredentialStoreVerificationResult RunRoundTrip(
        ICredentialStore store,
        string key,
        string firstValue,
        string secondValue)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(firstValue);
        ArgumentNullException.ThrowIfNull(secondValue);

        var passed = 0;
        const int total = 7;

        try
        {
            store.Delete(key);
            if (store.Get(key) is null)
                passed++;

            store.Set(key, firstValue);
            if (store.Get(key) == firstValue)
                passed++;

            store.Set(key, secondValue);
            if (store.Get(key) == secondValue)
                passed++;

            if (store.Get(key) != firstValue)
                passed++;

            if (store.Delete(key))
                passed++;

            if (store.Get(key) is null)
                passed++;

            if (!store.Delete(key))
                passed++;

            return new CredentialStoreVerificationResult(
                Passed: passed == total,
                PassedChecks: passed,
                TotalChecks: total,
                Summary: passed == total
                    ? $"CREDENTIAL STORE PASS — {passed}/{total}"
                    : $"CREDENTIAL STORE FAIL — {passed}/{total}");
        }
        finally
        {
            try { store.Delete(key); } catch { }
        }
    }
}
