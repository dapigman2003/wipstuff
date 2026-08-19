using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Centralized construction of SteamKit token logon details for the persistent
/// session established by AuthSessionDetails.IsPersistentSession=true.
/// </summary>
public static class SteamPersistentLogOnDetails
{
    public static SteamUser.LogOnDetails Create(
        string accountName,
        string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountName);
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        return new SteamUser.LogOnDetails
        {
            Username = accountName,
            AccessToken = refreshToken,
            ShouldRememberPassword = true,
            LoginID = SteamLoginIdentity.Create(),
            ClientOSType = EOSType.IOSUnknown,
            MachineName = SteamAuthenticationAttempt.DeviceFriendlyName,
        };
    }
}
