namespace StS2Launcher.Core;

public enum SteamAuthenticationStage
{
    Starting = 0,
    Connecting = 1,
    CmConnected = 2,
    AuthSessionStarted = 3,
    WaitingForMobileApproval = 4,
    MobileApprovalAccepted = 5,
    LoggingOn = 6,
    Authenticated = 7,
    Failed = 8,
    Cancelled = 9,
    TimedOut = 10,
}

/// <summary>
/// Metadata-only progress for the Step 06.1 authentication boundary.
/// It must never contain passwords, Steam Guard codes, access tokens,
/// refresh tokens, or raw Steam protocol payloads.
/// </summary>
public sealed record SteamAuthenticationProgress(
    SteamAuthenticationStage Stage,
    string Message);
