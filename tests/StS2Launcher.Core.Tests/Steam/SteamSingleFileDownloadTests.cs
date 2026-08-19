using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamSingleFileDownloadTests
{
    [TestMethod]
    public void Step09TargetAppIdRemainsSlayTheSpire2()
    {
        Assert.AreEqual(2868840u, SteamSingleFileDownloadAttempt.TargetAppId);
        Assert.AreEqual(SteamOwnershipVerificationAttempt.TargetAppId, SteamSingleFileDownloadAttempt.TargetAppId);
    }

    [TestMethod]
    public void ControlledFileCapIsExactlyTwoMiB()
    {
        Assert.AreEqual(2UL * 1024UL * 1024UL, SteamSingleFileTargetSelector.MaxTargetFileBytes);
    }

    [TestMethod]
    public void DepotSelectorPrefersDirectPublicMacosDepot()
    {
        IReadOnlyList<SteamDepotDiscovery> depots =
        [
            new SteamDepotDiscovery(
                100,
                "windows",
                "64",
                "english",
                [new SteamManifestDiscovery("public", "1000")]),
            new SteamDepotDiscovery(
                200,
                "macos",
                "64",
                null,
                [new SteamManifestDiscovery("public", "2000")]),
            new SteamDepotDiscovery(
                50,
                "macos",
                "64",
                null,
                [new SteamManifestDiscovery("public", "500")],
                DepotFromAppId: 999999),
        ];

        var selected = SteamSingleFileTargetSelector.SelectDepot(
            depots,
            SteamSingleFileDownloadAttempt.TargetAppId);

        Assert.IsNotNull(selected);
        Assert.AreEqual(200u, selected.DepotId);
        Assert.AreEqual(2000UL, selected.ManifestId);
        Assert.AreEqual("public", selected.Branch);
        Assert.AreEqual("macos", selected.OsList);
    }

    [TestMethod]
    public void DepotSelectorRequiresVisiblePublicManifest()
    {
        IReadOnlyList<SteamDepotDiscovery> depots =
        [
            new SteamDepotDiscovery(
                100,
                "macos",
                null,
                null,
                [new SteamManifestDiscovery("beta", "123")]),
        ];

        Assert.IsNull(SteamSingleFileTargetSelector.SelectDepot(
            depots,
            SteamSingleFileDownloadAttempt.TargetAppId));
    }

    [TestMethod]
    public void ManifestPathsRejectTraversalAndRootedPaths()
    {
        Assert.IsTrue(SteamSingleFileTargetSelector.IsSafeRelativePath("data/config.json"));
        Assert.IsTrue(SteamSingleFileTargetSelector.IsSafeRelativePath("config.json"));
        Assert.IsFalse(SteamSingleFileTargetSelector.IsSafeRelativePath("../secret.txt"));
        Assert.IsFalse(SteamSingleFileTargetSelector.IsSafeRelativePath("data/../../secret.txt"));
        Assert.IsFalse(SteamSingleFileTargetSelector.IsSafeRelativePath("/absolute/file.txt"));
    }

    [TestMethod]
    public void Step09ResultExposesNoRawSecretOrDownloadedByteArrays()
    {
        var properties = typeof(SteamSingleFileDownloadResult).GetProperties();

        Assert.IsFalse(properties.Any(property => property.PropertyType == typeof(byte[])));
        Assert.IsFalse(properties.Any(property =>
            property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase) &&
            property.PropertyType != typeof(bool)));
        Assert.IsFalse(properties.Any(property =>
            property.Name.Contains("DepotKey", StringComparison.OrdinalIgnoreCase) &&
            property.PropertyType != typeof(bool) &&
            property.PropertyType != typeof(SteamKit2.EResult?)));
        Assert.IsFalse(properties.Any(property =>
            property.Name.Contains("RequestCode", StringComparison.OrdinalIgnoreCase) &&
            property.PropertyType != typeof(bool)));
    }
}
