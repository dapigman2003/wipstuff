using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamFullDepotDownloadTests
{
    [TestMethod]
    public void Step10TargetAppIdRemainsSlayTheSpire2()
    {
        Assert.AreEqual(2868840u, SteamFullDepotDownloadAttempt.TargetAppId);
        Assert.AreEqual(SteamSingleFileDownloadAttempt.TargetAppId, SteamFullDepotDownloadAttempt.TargetAppId);
    }

    [TestMethod]
    public void Step10DepotSelectorRetainsDirectPublicMacosPreference()
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

        var selected = SteamDepotDownloadPlanner.SelectDepot(
            depots,
            SteamFullDepotDownloadAttempt.TargetAppId);

        Assert.IsNotNull(selected);
        Assert.AreEqual(200u, selected.DepotId);
        Assert.AreEqual(2000UL, selected.ManifestId);
        Assert.AreEqual("public", selected.Branch);
    }

    [TestMethod]
    public void ProgressReportsByteFractionAndFileCounts()
    {
        var progress = new SteamDepotDownloadProgress(
            SteamDepotDownloadPhase.Downloading,
            CompletedFiles: 3,
            TotalFiles: 10,
            CompletedChunks: 8,
            TotalChunks: 20,
            CompletedBytes: 250,
            TotalBytes: 1000,
            CurrentFile: "data/file.bin");

        Assert.AreEqual(0.25d, progress.Fraction, 0.0001d);
        Assert.AreEqual(25, progress.Percent);
        StringAssert.Contains(progress.Summary, "3/10 files");
    }

    [TestMethod]
    public void Step10ResultExposesNoRawSecretOrDownloadedByteArrays()
    {
        var properties = typeof(SteamFullDepotDownloadResult).GetProperties();

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
