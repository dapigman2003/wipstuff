namespace StS2Launcher.Core;

/// <summary>
/// Platform-neutral secure credential storage contract.
/// Step 04 uses this only with a non-secret test sentinel.
/// </summary>
public interface ICredentialStore
{
    void Set(string key, string value);
    string? Get(string key);
    bool Delete(string key);
}
