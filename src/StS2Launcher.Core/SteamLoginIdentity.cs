using System.Security.Cryptography;

namespace StS2Launcher.Core;

/// <summary>
/// Generates a non-secret per-logon Steam LoginID.
/// </summary>
public static class SteamLoginIdentity
{
    public static uint Create()
    {
        return checked((uint)RandomNumberGenerator.GetInt32(1, int.MaxValue));
    }
}
