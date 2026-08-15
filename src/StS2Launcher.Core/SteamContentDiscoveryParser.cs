using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Pure Step 08 parser for the PICS app-info KeyValues tree.
/// It only enumerates numeric depot nodes and already-visible branch manifest IDs.
/// </summary>
public static class SteamContentDiscoveryParser
{
    public static IReadOnlyList<SteamDepotDiscovery> Parse(KeyValue appInfo)
    {
        ArgumentNullException.ThrowIfNull(appInfo);

        var depotsNode = FindDepotsNode(appInfo);
        if (depotsNode is null)
            return Array.Empty<SteamDepotDiscovery>();

        var depots = new List<SteamDepotDiscovery>();
        foreach (var depotNode in depotsNode.Children)
        {
            if (!uint.TryParse(depotNode.Name, out var depotId) || depotId == 0)
                continue;

            var config = Child(depotNode, "config");
            var manifestsNode = Child(depotNode, "manifests");
            var manifests = new List<SteamManifestDiscovery>();

            if (manifestsNode is not null)
            {
                foreach (var branchNode in manifestsNode.Children)
                {
                    var branch = branchNode.Name?.Trim();
                    if (string.IsNullOrWhiteSpace(branch))
                        continue;

                    var rawManifestId = branchNode.Value;
                    if (string.IsNullOrWhiteSpace(rawManifestId))
                        rawManifestId = Child(branchNode, "gid")?.Value;

                    if (!ulong.TryParse(rawManifestId, out var manifestId) || manifestId == 0)
                        continue;

                    manifests.Add(new SteamManifestDiscovery(
                        Branch: branch,
                        ManifestId: manifestId.ToString()));
                }
            }

            depots.Add(new SteamDepotDiscovery(
                DepotId: depotId,
                OsList: Clean(config is null ? null : Child(config, "oslist")?.Value),
                OsArch: Clean(config is null ? null : Child(config, "osarch")?.Value),
                Language: Clean(config is null ? null : Child(config, "language")?.Value),
                Manifests: manifests
                    .OrderBy(m => m.Branch, StringComparer.OrdinalIgnoreCase)
                    .ToArray()));
        }

        return depots
            .OrderBy(d => d.DepotId)
            .ToArray();
    }

    private static KeyValue? FindDepotsNode(KeyValue root)
    {
        var direct = Child(root, "depots");
        if (direct is not null)
            return direct;

        // PICS normally gives an appinfo root with depots directly beneath it,
        // but tolerate one wrapper level so the parser is not coupled to a
        // cosmetic root-name difference.
        foreach (var child in root.Children)
        {
            var nested = Child(child, "depots");
            if (nested is not null)
                return nested;
        }

        return null;
    }

    private static KeyValue? Child(KeyValue parent, string name)
    {
        var child = parent[name];
        return ReferenceEquals(child, KeyValue.Invalid) ? null : child;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
