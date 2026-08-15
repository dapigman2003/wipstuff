namespace StS2Launcher.Core;

/// <summary>
/// Step 08 metadata for one visible branch manifest of one Steam depot.
/// This is discovery metadata only; no manifest body or CDN content is fetched.
/// </summary>
public sealed record SteamManifestDiscovery(
    string Branch,
    string ManifestId);

/// <summary>
/// Step 08 metadata for one depot listed in the target app's PICS product info.
/// DepotFromAppId is retained only so later controlled content tests can avoid
/// accidentally treating a shared/proxied depot as a direct depot.
/// </summary>
public sealed record SteamDepotDiscovery(
    uint DepotId,
    string? OsList,
    string? OsArch,
    string? Language,
    IReadOnlyList<SteamManifestDiscovery> Manifests,
    uint? DepotFromAppId = null);
