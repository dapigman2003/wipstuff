using SteamKit2;

namespace StS2Launcher.Core;

public enum SteamOwnershipVerificationOutcome
{
    Failed = 0,
    NoSavedSession = 1,
    Owned = 2,
    SessionRejected = 3,
    InvalidLocalSession = 4,
    IdentityMismatch = 5,
    TicketRejected = 6,
    EmptyTicket = 7,
    UnexpectedAppId = 8,
    Cancelled = 9,
    TimedOut = 10,
}

/// <summary>
/// Step 07 result contract. The ownership-ticket payload itself is deliberately
/// not retained in this record; only its byte length is exposed for diagnostics.
/// </summary>
public sealed record SteamOwnershipVerificationResult(
    SteamOwnershipVerificationOutcome Outcome,
    uint TargetAppId,
    bool SavedSessionFound,
    bool CmConnected,
    bool LoggedOnCallbackReceived,
    EResult? LogonResult,
    EResult? ExtendedLogonResult,
    bool IdentityMatched,
    bool OwnershipTicketCallbackReceived,
    EResult? OwnershipResult,
    uint? OwnershipAppId,
    int OwnershipTicketLength,
    string? AccountName,
    string? SteamId64,
    string? CurrentEndPoint,
    TimeSpan Elapsed,
    string? Error,
    uint? LoginId = null)
{
    public bool OwnershipProven => Outcome == SteamOwnershipVerificationOutcome.Owned;

    public string Summary => Outcome switch
    {
        SteamOwnershipVerificationOutcome.Owned => $"OWNERSHIP PASS — App {TargetAppId} owned",
        SteamOwnershipVerificationOutcome.NoSavedSession => "OWNERSHIP — no saved session",
        SteamOwnershipVerificationOutcome.SessionRejected => "OWNERSHIP BLOCKED — saved session rejected",
        SteamOwnershipVerificationOutcome.InvalidLocalSession => "OWNERSHIP BLOCKED — invalid saved session",
        SteamOwnershipVerificationOutcome.IdentityMismatch => "OWNERSHIP BLOCKED — identity mismatch",
        SteamOwnershipVerificationOutcome.TicketRejected => "OWNERSHIP NOT PROVEN — ticket rejected",
        SteamOwnershipVerificationOutcome.EmptyTicket => "OWNERSHIP NOT PROVEN — empty ticket",
        SteamOwnershipVerificationOutcome.UnexpectedAppId => "OWNERSHIP NOT PROVEN — unexpected AppID",
        SteamOwnershipVerificationOutcome.TimedOut => "OWNERSHIP TIMEOUT",
        SteamOwnershipVerificationOutcome.Cancelled => "OWNERSHIP CANCELLED",
        _ => "OWNERSHIP FAIL",
    };
}
