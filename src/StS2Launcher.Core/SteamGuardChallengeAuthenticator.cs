using SteamKit2.Authentication;

namespace StS2Launcher.Core;

/// <summary>
/// Step 06 intentionally observes Steam Guard boundaries without completing
/// them. No invalid code is sent and no mobile confirmation is accepted here.
/// </summary>
public sealed class SteamGuardChallengeAuthenticator : IAuthenticator
{
    public SteamGuardChallenge? LastChallenge { get; private set; }

    public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect) =>
        Fail<string>(new SteamGuardChallenge(
            SteamGuardChallengeKind.DeviceCode,
            AssociatedMessage: null,
            PreviousCodeWasIncorrect: previousCodeWasIncorrect));

    public Task<string> GetEmailCodeAsync(
        string email,
        bool previousCodeWasIncorrect) =>
        Fail<string>(new SteamGuardChallenge(
            SteamGuardChallengeKind.EmailCode,
            AssociatedMessage: email,
            PreviousCodeWasIncorrect: previousCodeWasIncorrect));

    public Task<bool> AcceptDeviceConfirmationAsync() =>
        Fail<bool>(new SteamGuardChallenge(
            SteamGuardChallengeKind.DeviceConfirmation,
            AssociatedMessage: null,
            PreviousCodeWasIncorrect: false));

    private Task<T> Fail<T>(SteamGuardChallenge challenge)
    {
        LastChallenge = challenge;
        return Task.FromException<T>(
            new SteamGuardChallengeRequiredException(challenge));
    }
}
