using SteamKit2;

namespace StS2Launcher.Core;

public enum SteamAuthenticationOutcome
{
    Failed = 0,
    GuardRequired = 1,
    Authenticated = 2,
    Cancelled = 3,
}

public sealed record SteamAuthenticationResult(
    SteamAuthenticationOutcome Outcome,
    bool CmConnected,
    bool AuthSessionStarted,
    bool LoggedOnCallbackReceived,
    EResult? LogonResult,
    EResult? ExtendedLogonResult,
    string? AccountName,
    string? SteamId64,
    SteamGuardChallenge? GuardChallenge,
    string? CurrentEndPoint,
    TimeSpan Elapsed,
    string? Error)
{
    public bool Authenticated => Outcome == SteamAuthenticationOutcome.Authenticated;
    public bool GuardRequired => Outcome == SteamAuthenticationOutcome.GuardRequired;

    public string Summary => Outcome switch
    {
        SteamAuthenticationOutcome.Authenticated => "STEAM AUTH PASS — authenticated",
        SteamAuthenticationOutcome.GuardRequired => GuardChallenge?.Summary ?? "STEAM GUARD REQUIRED",
        SteamAuthenticationOutcome.Cancelled => "STEAM AUTH CANCELLED",
        _ => "STEAM AUTH FAIL",
    };
}
