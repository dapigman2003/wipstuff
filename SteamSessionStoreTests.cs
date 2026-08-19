using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamSessionStoreTests
{
    [TestMethod]
    public void SaveLoadRoundTripPreservesIdentityAndToken()
    {
        var credentials = new InMemoryCredentialStore();
        var store = new SteamSessionStore(credentials);
        var saved = new SteamSavedSession(
            "example_account",
            "76561198000000000",
            "refresh-token-secret");

        store.Save(saved);
        var loaded = store.Load();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(saved.AccountName, loaded.AccountName);
        Assert.AreEqual(saved.SteamId64, loaded.SteamId64);
        Assert.AreEqual(saved.RefreshToken, loaded.RefreshToken);
    }

    [TestMethod]
    public void SaveOverwritesPreviousSession()
    {
        var credentials = new InMemoryCredentialStore();
        var store = new SteamSessionStore(credentials);

        store.Save(new SteamSavedSession("first", "76561198000000001", "token-one"));
        store.Save(new SteamSavedSession("second", "76561198000000002", "token-two"));

        var loaded = store.Load();
        Assert.IsNotNull(loaded);
        Assert.AreEqual("second", loaded.AccountName);
        Assert.AreEqual("76561198000000002", loaded.SteamId64);
        Assert.AreEqual("token-two", loaded.RefreshToken);
    }

    [TestMethod]
    public void ClearRemovesSavedSessionAndIsIdempotent()
    {
        var credentials = new InMemoryCredentialStore();
        var store = new SteamSessionStore(credentials);

        store.Save(new SteamSavedSession("example", "76561198000000000", "token"));

        Assert.IsTrue(store.Clear());
        Assert.IsNull(store.Load());
        Assert.IsFalse(store.Clear());
    }

    [TestMethod]
    public void MalformedStoredPayloadIsRejected()
    {
        var credentials = new InMemoryCredentialStore();
        credentials.Set(SteamSessionStore.StorageKey, "not-a-supported-session");
        var store = new SteamSessionStore(credentials);

        Assert.ThrowsExactly<InvalidDataException>(() => store.Load());
    }

    [TestMethod]
    public void SavedSessionToStringNeverExposesRefreshToken()
    {
        var session = new SteamSavedSession(
            "example",
            "76561198000000000",
            "super-secret-refresh-token");

        var rendered = session.ToString();

        StringAssert.Contains(rendered, "RefreshToken=<redacted>");
        Assert.IsFalse(rendered.Contains("super-secret-refresh-token", StringComparison.Ordinal));
    }

    [TestMethod]
    public void InvalidSteamIdIsRejectedBeforeCredentialStoreMutation()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new SteamSavedSession("example", "not-a-steamid", "token"));
    }

    private sealed class InMemoryCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

        public void Set(string key, string value) => _values[key] = value;

        public string? Get(string key) =>
            _values.TryGetValue(key, out var value) ? value : null;

        public bool Delete(string key) => _values.Remove(key);
    }
}
