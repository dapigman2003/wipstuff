using System.Text.Json.Serialization;

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
}

/// <summary>
/// Step 12.1 iOS/AOT compatibility boundary for the managed-install receipt.
/// The receipt JSON contract is generated at compile time so full trimming does
/// not need to discover positional-record constructor parameter names through
/// reflection at runtime.
/// </summary>
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata,
    WriteIndented = true)]
[JsonSerializable(typeof(SteamManagedInstallReceipt))]
[JsonSerializable(typeof(SteamManagedInstallFile))]
public sealed partial class SteamManagedInstallJsonContext : JsonSerializerContext
{
}
