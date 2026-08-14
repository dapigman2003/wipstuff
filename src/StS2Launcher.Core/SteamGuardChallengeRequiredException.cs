namespace StS2Launcher.Core;

/// <summary>
/// Internal control-flow exception used only by the Step 06 challenge observer.
/// It deliberately stops the auth flow before any Steam Guard code or approval
/// is supplied. Step 06.1 will add real challenge handling.
/// </summary>
public sealed class SteamGuardChallengeRequiredException : Exception
{
    public SteamGuardChallengeRequiredException(SteamGuardChallenge challenge)
        : base(challenge.Summary)
    {
        Challenge = challenge;
    }

    public SteamGuardChallenge Challenge { get; }
}
