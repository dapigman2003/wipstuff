using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Pure Step 07 interpretation of Steam's app-ownership ticket response.
/// Ownership is proven only by an OK response for the requested AppID with a
/// non-empty ticket. Non-OK results are intentionally treated as inconclusive
/// rather than guessed to mean a specific licensing state.
/// </summary>
public static class SteamOwnershipDecision
{
    public static SteamOwnershipVerificationOutcome EvaluateTicket(
        uint targetAppId,
        EResult result,
        uint returnedAppId,
        int ticketLength)
    {
        if (returnedAppId != targetAppId)
            return SteamOwnershipVerificationOutcome.UnexpectedAppId;

        if (result != EResult.OK)
            return SteamOwnershipVerificationOutcome.TicketRejected;

        if (ticketLength <= 0)
            return SteamOwnershipVerificationOutcome.EmptyTicket;

        return SteamOwnershipVerificationOutcome.Owned;
    }
}
