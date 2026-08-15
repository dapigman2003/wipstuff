using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamResumableDepotDownloadTests
{
    [TestMethod]
    public void Step11TargetAppIdRemainsSlayTheSpire2()
    {
        Assert.AreEqual(2868840u, SteamResumableDepotDownloadAttempt.TargetAppId);
        Assert.AreEqual(SteamFullDepotDownloadAttempt.TargetAppId, SteamResumableDepotDownloadAttempt.TargetAppId);
    }

    [TestMethod]
    public void Adler32MatchesStandardKnownVector()
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes("Wikipedia");
        Assert.AreEqual(0x11E60398u, SteamDepotResumeValidation.ComputeAdler32(bytes));
        Assert.AreEqual(1u, SteamDepotResumeValidation.ComputeAdler32(ReadOnlySpan<byte>.Empty));
    }

    [TestMethod]
    public async Task Adler32StreamingMatchesSpanImplementation()
    {
        var bytes = Enumerable.Range(0, 50_000).Select(i => (byte)(i % 251)).ToArray();
        await using var stream = new MemoryStream(bytes, writable: false);
        var streamed = await SteamDepotResumeValidation.ComputeAdler32Async(stream, bytes.Length);
        Assert.AreEqual(SteamDepotResumeValidation.ComputeAdler32(bytes), streamed);
    }

    [TestMethod]
    public void Step11ProgressAddsResumingPhaseWithoutChangingStep10Values()
    {
        Assert.AreEqual(0, (int)SteamDepotDownloadPhase.Preparing);
        Assert.AreEqual(4, (int)SteamDepotDownloadPhase.Complete);
        Assert.AreEqual(5, (int)SteamDepotDownloadPhase.Resuming);
    }

    [TestMethod]
    public void Step11ResultExposesOnlyResumeTelemetryNotSecretsOrPayloads()
    {
        var properties = typeof(SteamResumableDepotDownloadResult).GetProperties();

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
