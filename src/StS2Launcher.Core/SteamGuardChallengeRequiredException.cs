namespace StS2Launcher.Core;

/// <summary>
/// Internal control-flow exception used by Step 06.2 for code-based Steam Guard
/// challenges that this step intentionally does not handle. Mobile device
/// confirmation uses SteamKit's same-session polling path instead.
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
