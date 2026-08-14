using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamFoundationTests
{
    [TestMethod]
    public void SteamKitVersionIsPinnedToThreeFourZero()
    {
        Assert.IsTrue(
            SteamConnectionProbe.AssemblyVersion.StartsWith("3.4.0", StringComparison.Ordinal),
            SteamConnectionProbe.AssemblyVersion);
    }

    [TestMethod]
    public void CmWebSocketPurposeUsesSocketsHttpHandlerPolicy()
    {
        Assert.IsTrue(
            SteamHttpClientFactory.UsesSocketsHttpHandler(HttpClientPurpose.CMWebSocket));
    }

    [TestMethod]
    public void WebApiPurposeKeepsPlatformDefaultHandlerPolicy()
    {
        Assert.IsFalse(
            SteamHttpClientFactory.UsesSocketsHttpHandler(HttpClientPurpose.WebAPI));
    }

    [TestMethod]
    public void FactoryPreservesSteamKitUserAgent()
    {
        using var client = SteamHttpClientFactory.Create(HttpClientPurpose.CMWebSocket);
        var userAgent = client.DefaultRequestHeaders.UserAgent.ToString();

        Assert.IsTrue(userAgent.StartsWith("SteamKit/", StringComparison.Ordinal), userAgent);
    }

    [DataTestMethod]
    [DataRow(true, true, true, true)]
    [DataRow(false, true, true, false)]
    [DataRow(true, false, true, false)]
    [DataRow(true, true, false, false)]
    public void ThreeOfThreeResultRequiresAllConnectionGates(
        bool constructed,
        bool connected,
        bool disconnected,
        bool expectedPass)
    {
        var result = MakeSteamResult(constructed, connected, disconnected);

        Assert.AreEqual(expectedPass, result.Passed);
        Assert.AreEqual(
            (constructed ? 1 : 0) + (connected ? 1 : 0) + (disconnected ? 1 : 0),
            result.PassedChecks);
    }

    private static SteamConnectionProbeResult MakeSteamResult(
        bool constructed,
        bool connected,
        bool disconnected) =>
        new(
            TransportName: "WebSocket/SocketsHttpHandler",
            Protocols: "WebSocket",
            ClientConstructed: constructed,
            ConnectedCallbackReceived: connected,
            DisconnectedCallbackReceived: disconnected,
            DisconnectedUserInitiated: disconnected ? true : null,
            IsConnectedEver: connected,
            LastCurrentEndPoint: connected ? "cm.example:443" : null,
            CmWebSocketFactoryUsed: true,
            SteamKitAssemblyVersion: "3.4.0.0",
            Elapsed: TimeSpan.FromMilliseconds(100),
            Error: null);
}
