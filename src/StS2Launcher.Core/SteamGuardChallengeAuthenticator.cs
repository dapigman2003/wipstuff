using SteamKit2.Authentication;

namespace StS2Launcher.Core;

/// <summary>
/// Step 06.2 authenticator policy (retained from Step 06.1).
///
/// Mobile Steam Guard confirmation is supported by opting into SteamKit's
/// polling path. Device-code and email-code challenges remain deliberately
/// outside this step and are reported without submitting a code.
/// </summary>
public sealed class SteamGuardChallengeAuthenticator : IAuthenticator
{
    private readonly IProgress<SteamAuthenticationProgress>? _progress;

    public SteamGuardChallengeAuthenticator(
        IProgress<SteamAuthenticationProgress>? progress = null)
    {
        _progress = progress;
    }

    public SteamGuardChallenge? LastChallenge { get; private set; }
    public bool MobileApprovalRequested { get; private set; }

    public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect) =>
        UnsupportedCode<string>(new SteamGuardChallenge(
            SteamGuardChallengeKind.DeviceCode,
            AssociatedMessage: null,
            PreviousCodeWasIncorrect: previousCodeWasIncorrect));

    public Task<string> GetEmailCodeAsync(
        string email,
        bool previousCodeWasIncorrect) =>
        UnsupportedCode<string>(new SteamGuardChallenge(
            SteamGuardChallengeKind.EmailCode,
            AssociatedMessage: email,
            PreviousCodeWasIncorrect: previousCodeWasIncorrect));

    public Task<bool> AcceptDeviceConfirmationAsync()
    {
        LastChallenge = new SteamGuardChallenge(
            SteamGuardChallengeKind.DeviceConfirmation,
            AssociatedMessage: null,
            PreviousCodeWasIncorrect: false);
        MobileApprovalRequested = true;
        _progress?.Report(new SteamAuthenticationProgress(
            SteamAuthenticationStage.WaitingForMobileApproval,
            "Waiting for Steam Guard mobile approval. Open the Steam app, approve the sign-in, then return to StS2 Launcher."));

        // SteamKit interprets true as: keep this same authentication session
        // alive and poll until the mobile confirmation is accepted.
        return Task.FromResult(true);
    }

    private Task<T> UnsupportedCode<T>(SteamGuardChallenge challenge)
    {
        LastChallenge = challenge;
        return Task.FromException<T>(
            new SteamGuardChallengeRequiredException(challenge));
    }
}
