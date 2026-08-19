using StS2Launcher.Core;

namespace StS2Launcher.iOS.Platform;

/// <summary>
/// Physical-device adapter for the platform-neutral credential-store verifier.
/// It uses harmless sentinels and removes the probe value before returning.
/// </summary>
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

    public CredentialStoreVerificationResult RunRoundTrip() =>
        CredentialStoreVerifier.RunRoundTrip(
            _store,
            TestKey,
            AlphaValue,
            BetaValue);
}
