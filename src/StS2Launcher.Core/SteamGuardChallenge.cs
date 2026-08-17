namespace StS2Launcher.Core;

public enum SteamGuardChallengeKind
{
    None = 0,
    DeviceConfirmation = 1,
    DeviceCode = 2,
    EmailCode = 3,
}

public sealed record SteamGuardChallenge(
    SteamGuardChallengeKind Kind,
    string? AssociatedMessage,
    bool PreviousCodeWasIncorrect)
{
    public string Summary => Kind switch
    {
        SteamGuardChallengeKind.DeviceConfirmation =>
            "STEAM GUARD REQUIRED — mobile-app confirmation",
        SteamGuardChallengeKind.DeviceCode =>
            PreviousCodeWasIncorrect
                ? "STEAM GUARD REQUIRED — authenticator code (previous code rejected)"
                : "STEAM GUARD REQUIRED — authenticator code",
        SteamGuardChallengeKind.EmailCode =>
            PreviousCodeWasIncorrect
                ? "STEAM GUARD REQUIRED — email code (previous code rejected)"
                : "STEAM GUARD REQUIRED — email code",
        _ => "STEAM GUARD REQUIRED",
    };
}
