using System.Text;

namespace StS2Launcher.Core;

/// <summary>
/// Stores exactly one reusable Steam client session in the platform credential
/// store. Step 06.2 persists only account identity metadata plus the refresh
/// token returned by Steam's persistent authentication session.
///
/// The payload format is intentionally tiny and reflection-free so the iOS
/// trimmer/AOT pipeline does not need an additional JSON serializer boundary.
/// </summary>
public sealed class SteamSessionStore
{
    public const string StorageKey = "steam.session.v1";
    private const string FormatMarker = "STS2-STEAM-SESSION-V1";

    private readonly ICredentialStore _credentialStore;

    public SteamSessionStore(ICredentialStore credentialStore)
    {
        _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
    }

    public void Save(SteamSavedSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var payload = string.Join('\n',
            FormatMarker,
            Encode(session.AccountName),
            session.SteamId64,
            Encode(session.RefreshToken));

        _credentialStore.Set(StorageKey, payload);
    }

    public SteamSavedSession? Load()
    {
        var payload = _credentialStore.Get(StorageKey);
        if (payload is null)
            return null;

        var lines = payload.Split('\n');
        if (lines.Length != 4 || !string.Equals(lines[0], FormatMarker, StringComparison.Ordinal))
            throw new InvalidDataException("Saved Steam session has an unsupported format.");

        string accountName;
        string refreshToken;
        try
        {
            accountName = Decode(lines[1]);
            refreshToken = Decode(lines[3]);
        }
        catch (FormatException ex)
        {
            throw new InvalidDataException("Saved Steam session contains invalid encoded data.", ex);
        }

        try
        {
            return new SteamSavedSession(accountName, lines[2], refreshToken);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidDataException("Saved Steam session contains invalid identity data.", ex);
        }
    }

    public bool Clear() => _credentialStore.Delete(StorageKey);

    private static string Encode(string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    private static string Decode(string value) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(value));
}
