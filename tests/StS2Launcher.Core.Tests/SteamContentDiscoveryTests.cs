using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;
using SteamKit2;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamContentDiscoveryTests
{
    [TestMethod]
    public void TargetAppIdRemainsSlayTheSpire2()
    {
        Assert.AreEqual(2868840u, SteamContentDiscoveryAttempt.TargetAppId);
        Assert.AreEqual(SteamOwnershipVerificationAttempt.TargetAppId, SteamContentDiscoveryAttempt.TargetAppId);
    }

    [TestMethod]
    public void ParserExtractsDepotPlatformAndVisibleBranchManifests()
    {
        var root = new KeyValue("appinfo");
        var depots = Add(root, "depots");

        // Non-depot app-info metadata must not be treated as a depot.
        Add(depots, "branches");

        var macDepot = Add(depots, "2868841");
        var config = Add(macDepot, "config");
        Add(config, "oslist", "macos");
        Add(config, "osarch", "64");
        Add(config, "language", "english");
        var manifests = Add(macDepot, "manifests");
        Add(manifests, "public", "1234567890123456789");

        var sharedDepot = Add(depots, "2868842");
        Add(sharedDepot, "depotfromapp", "123456");
        var nestedManifests = Add(sharedDepot, "manifests");
        var beta = Add(nestedManifests, "beta");
        Add(beta, "gid", "9876543210987654321");

        var parsed = SteamContentDiscoveryParser.Parse(root);

        Assert.AreEqual(2, parsed.Count);
        Assert.AreEqual(2868841u, parsed[0].DepotId);
        Assert.AreEqual("macos", parsed[0].OsList);
        Assert.AreEqual("64", parsed[0].OsArch);
        Assert.AreEqual("english", parsed[0].Language);
        Assert.AreEqual(1, parsed[0].Manifests.Count);
        Assert.AreEqual("public", parsed[0].Manifests[0].Branch);
        Assert.AreEqual("1234567890123456789", parsed[0].Manifests[0].ManifestId);

        Assert.AreEqual(2868842u, parsed[1].DepotId);
        Assert.AreEqual(123456u, parsed[1].DepotFromAppId);
        Assert.AreEqual(1, parsed[1].Manifests.Count);
        Assert.AreEqual("beta", parsed[1].Manifests[0].Branch);
        Assert.AreEqual("9876543210987654321", parsed[1].Manifests[0].ManifestId);
    }

    [TestMethod]
    public void ParserToleratesOneWrapperLevelAndSkipsInvalidManifestIds()
    {
        var root = new KeyValue("response");
        var app = Add(root, "2868840");
        var depots = Add(app, "depots");
        var depot = Add(depots, "2868843");
        var manifests = Add(depot, "manifests");
        Add(manifests, "public", "0");
        Add(manifests, "broken", "not-a-number");
        Add(manifests, "preview", "555");

        var parsed = SteamContentDiscoveryParser.Parse(root);

        Assert.AreEqual(1, parsed.Count);
        Assert.AreEqual(1, parsed[0].Manifests.Count);
        Assert.AreEqual("preview", parsed[0].Manifests[0].Branch);
        Assert.AreEqual("555", parsed[0].Manifests[0].ManifestId);
    }

    [TestMethod]
    public void DiscoveryResultExposesNoRawOwnershipTicketOrPicsAccessTokenValue()
    {
        var properties = typeof(SteamContentDiscoveryResult).GetProperties();

        Assert.IsFalse(properties.Any(property => property.PropertyType == typeof(byte[])));
        Assert.IsFalse(properties.Any(property =>
            property.Name.Contains("AccessToken", StringComparison.OrdinalIgnoreCase) &&
            property.PropertyType != typeof(bool)));
    }

    [TestMethod]
    public void DiscoveredSummaryReportsDepotAndManifestCounts()
    {
        var result = new SteamContentDiscoveryResult(
            Outcome: SteamContentDiscoveryOutcome.Discovered,
            TargetAppId: SteamContentDiscoveryAttempt.TargetAppId,
            SavedSessionFound: true,
            CmConnected: true,
            LoggedOnCallbackReceived: true,
            LogonResult: EResult.OK,
            ExtendedLogonResult: EResult.OK,
            IdentityMatched: true,
            OwnershipTicketCallbackReceived: true,
            OwnershipResult: EResult.OK,
            OwnershipTicketLength: 128,
            OwnershipProven: true,
            PicsAccessTokenCallbackReceived: true,
            PicsAccessTokenReceived: true,
            PicsProductInfoCallbackReceived: true,
            PicsAppInfoFound: true,
            PicsMissingToken: false,
            PicsChangeNumber: 42,
            Depots:
            [
                new SteamDepotDiscovery(
                    2868841,
                    "macos",
                    "64",
                    null,
                    [new SteamManifestDiscovery("public", "123")]),
                new SteamDepotDiscovery(
                    2868842,
                    null,
                    null,
                    null,
                    [
                        new SteamManifestDiscovery("public", "456"),
                        new SteamManifestDiscovery("beta", "789"),
                    ]),
            ],
            AccountName: "test",
            SteamId64: "76561198000000000",
            CurrentEndPoint: "example:443",
            Elapsed: TimeSpan.FromSeconds(1),
            Error: null,
            LoginId: 123);

        Assert.IsTrue(result.DiscoveryProven);
        Assert.AreEqual(2, result.DepotCount);
        Assert.AreEqual(3, result.ManifestCount);
        Assert.AreEqual("DISCOVERY PASS — 2 depots / 3 manifests", result.Summary);
    }

    private static KeyValue Add(KeyValue parent, string name, string? value = null)
    {
        var child = new KeyValue(name, value);
        parent.Children.Add(child);
        return child;
    }
}
