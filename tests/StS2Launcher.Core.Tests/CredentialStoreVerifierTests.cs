using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class CredentialStoreVerifierTests
{
    [TestMethod]
    public void RoundTripPassesAndLeavesStoreClean()
    {
        var store = new InMemoryCredentialStore();

        var result = CredentialStoreVerifier.RunRoundTrip(
            store,
            "test-key",
            "alpha",
            "beta");

        Assert.IsTrue(result.Passed);
        Assert.AreEqual(7, result.PassedChecks);
        Assert.AreEqual(7, result.TotalChecks);
        Assert.IsNull(store.Get("test-key"));
    }

    [TestMethod]
    public void RoundTripVerifiesOverwriteSemantics()
    {
        var store = new NonOverwritingCredentialStore();

        var result = CredentialStoreVerifier.RunRoundTrip(
            store,
            "test-key",
            "alpha",
            "beta");

        Assert.IsFalse(result.Passed);
        Assert.IsTrue(result.PassedChecks < result.TotalChecks);
        Assert.IsNull(store.Get("test-key"));
    }

    [TestMethod]
    public void InvalidKeyIsRejectedBeforeStoreMutation()
    {
        var store = new InMemoryCredentialStore();

        Assert.ThrowsException<ArgumentException>(() =>
            CredentialStoreVerifier.RunRoundTrip(store, " ", "alpha", "beta"));

        Assert.AreEqual(0, store.MutationCount);
    }

    private sealed class InMemoryCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);
        public int MutationCount { get; private set; }

        public void Set(string key, string value)
        {
            MutationCount++;
            _values[key] = value;
        }

        public string? Get(string key) =>
            _values.TryGetValue(key, out var value) ? value : null;

        public bool Delete(string key)
        {
            MutationCount++;
            return _values.Remove(key);
        }
    }

    private sealed class NonOverwritingCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

        public void Set(string key, string value)
        {
            _values.TryAdd(key, value);
        }

        public string? Get(string key) =>
            _values.TryGetValue(key, out var value) ? value : null;

        public bool Delete(string key) => _values.Remove(key);
    }
}
