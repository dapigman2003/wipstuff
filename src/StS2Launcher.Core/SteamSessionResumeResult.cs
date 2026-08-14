using SteamKit2;

namespace StS2Launcher.Core;

public enum SteamSessionResumeOutcome
{
    Failed = 0,
    NoSavedSession = 1,
    Authenticated = 2,
    Rejected = 3,
    Cancelled = 4,
    TimedOut = 5,
}

public sealed record SteamSessionResumeResult(
    SteamSessionResumeOutcome Outcome,
    bool SavedSessionFound,
    bool CmConnected,
    bool LoggedOnCallbackReceived,
    EResult? LogonResult,
    EResult? ExtendedLogonResult,
    bool IdentityMatched,
    string? AccountName,
    string? SteamId64,
    string? CurrentEndPoint,
    TimeSpan Elapsed,
    string? Error)
{
    public bool Authenticated => Outcome == SteamSessionResumeOutcome.Authenticated;

    public string Summary => Outcome switch
    {
        SteamSessionResumeOutcome.Authenticated => "SAVED SESSION PASS — authenticated",
        SteamSessionResumeOutcome.NoSavedSession => "SAVED SESSION — none",
        SteamSessionResumeOutcome.Rejected => "SAVED SESSION REJECTED",
        SteamSessionResumeOutcome.TimedOut => "SAVED SESSION TIMEOUT",
        SteamSessionResumeOutcome.Cancelled => "SAVED SESSION CANCELLED",
        _ => "SAVED SESSION FAIL",
    };
}
