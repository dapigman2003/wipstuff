using SteamKit2;

namespace StS2Launcher.Core;

public enum SteamSessionRecoveryAction
{
    KeepSavedSession = 0,
    ClearSavedSessionAndRequireInteractiveAuthentication = 1,
}

/// <summary>
/// Step 06.3.1 recovery policy for a persisted Steam refresh-token session.
///
/// The policy is intentionally conservative: transient connection/service
/// failures never destroy a previously working Keychain session. The saved
/// session is cleared only when the local record is invalid, Steam returns a
/// result that specifically means the credential is unusable, or the server
/// authenticates a different Steam identity than the one that was stored.
/// </summary>
public static class SteamSessionRecoveryPolicy
{
    public static SteamSessionRecoveryAction Evaluate(SteamSessionResumeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Outcome is SteamSessionResumeOutcome.InvalidLocalSession or
            SteamSessionResumeOutcome.IdentityMismatch)
        {
            return SteamSessionRecoveryAction.ClearSavedSessionAndRequireInteractiveAuthentication;
        }

        if (result.Outcome == SteamSessionResumeOutcome.Rejected &&
            IsDefinitelyUnusableCredential(result.LogonResult, result.ExtendedLogonResult))
        {
            return SteamSessionRecoveryAction.ClearSavedSessionAndRequireInteractiveAuthentication;
        }

        return SteamSessionRecoveryAction.KeepSavedSession;
    }

    private static bool IsDefinitelyUnusableCredential(
        EResult? primary,
        EResult? extended) =>
        IsDefinitelyUnusableCredential(primary) ||
        IsDefinitelyUnusableCredential(extended);

    private static bool IsDefinitelyUnusableCredential(EResult? result) =>
        result is EResult.InvalidPassword or EResult.Revoked or EResult.Expired;
}
