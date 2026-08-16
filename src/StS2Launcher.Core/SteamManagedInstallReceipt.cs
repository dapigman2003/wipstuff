using System.Text.Json;

namespace StS2Launcher.Core;

public sealed record SteamManagedInstallFile(
    string RelativePath,
    long Length,
    string Sha1Hex);

public sealed record SteamManagedInstallReceipt(
    int SchemaVersion,
    uint AppId,
    uint DepotId,
    ulong ManifestId,
    string Branch,
    DateTimeOffset CreatedUtc,
    IReadOnlyList<SteamManagedInstallFile> Files)
{
    public const int CurrentSchemaVersion = 1;
    public const string FileName = ".sts2launcher-install.json";

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = false,
    };
}
