using System.Text.Json;

namespace StS2Launcher.Core;

/// <summary>
/// Non-secret timing metadata decoded from the JWT payload of a Steam refresh
/// token. The raw token is never returned, displayed, or logged.
/// </summary>
public sealed record SteamRefreshTokenMetadata(
    DateTimeOffset? IssuedAtUtc,
    DateTimeOffset? ExpiresAtUtc)
{
    public bool IsExpiredAt(DateTimeOffset utcNow) =>
        ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= utcNow;

    public static bool TryParse(
        string refreshToken,
        out SteamRefreshTokenMetadata? metadata)
    {
        metadata = null;

        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        var parts = refreshToken.Split('.');
        if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
            return false;

        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            var remainder = payload.Length % 4;
            if (remainder != 0)
                payload += new string('=', 4 - remainder);

            var bytes = Convert.FromBase64String(payload);
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;

            metadata = new SteamRefreshTokenMetadata(
                ReadUnixTime(root, "iat"),
                ReadUnixTime(root, "exp"));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private static DateTimeOffset? ReadUnixTime(
        JsonElement root,
        string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return null;

        long seconds;
        if (value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt64(out seconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }

        if (value.ValueKind == JsonValueKind.String &&
            long.TryParse(value.GetString(), out seconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }

        return null;
    }
}
