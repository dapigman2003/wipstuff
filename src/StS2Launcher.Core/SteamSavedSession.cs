namespace StS2Launcher.Core;

/// <summary>
/// Sensitive reusable Steam authentication material for Step 06.2/06.3.
///
/// The refresh token must never be logged, displayed, or serialized outside
/// the platform credential store. Passwords and Steam Guard secrets are not
/// part of this object.
/// </summary>
public sealed class SteamSavedSession
{
    public SteamSavedSession(
        string accountName,
        string steamId64,
        string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountName);
        ArgumentException.ThrowIfNullOrWhiteSpace(steamId64);
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        if (!ulong.TryParse(steamId64, out _))
            throw new ArgumentException("SteamID64 must be an unsigned integer.", nameof(steamId64));

        AccountName = accountName.Trim();
        SteamId64 = steamId64;
        RefreshToken = refreshToken;
    }

    public string AccountName { get; }
    public string SteamId64 { get; }
    public string RefreshToken { get; }

    public override string ToString() =>
        $"SteamSavedSession(AccountName={AccountName}, SteamId64={SteamId64}, RefreshToken=<redacted>)";
}
