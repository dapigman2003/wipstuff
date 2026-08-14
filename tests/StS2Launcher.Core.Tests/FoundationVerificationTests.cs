using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class FoundationVerificationTests
{
    [TestMethod]
    public void AllFiveFoundationGatesProducePass()
    {
        var result = MakePassingResult();

        Assert.IsTrue(result.Passed);
        Assert.AreEqual(5, result.PassedGates);
        Assert.AreEqual("FOUNDATION PASS — 5/5", result.Summary);
    }

    [TestMethod]
    public void UiStartupIsRequired()
    {
        var passing = MakePassingResult();
        var result = passing with { UiStartupPassed = false };

        Assert.IsFalse(result.Passed);
        Assert.AreEqual(4, result.PassedGates);
    }

    [TestMethod]
    public void ActiveLifecycleIsRequired()
    {
        var passing = MakePassingResult();
        var result = passing with { LifecycleActive = false };

        Assert.IsFalse(result.Passed);
        Assert.AreEqual(4, result.PassedGates);
    }

    [TestMethod]
    public void CoreCredentialAndSteamGatesAreEachRequired()
    {
        var passing = MakePassingResult();

        Assert.IsFalse((passing with
        {
            Core = passing.Core with { Passed = false, PassedChecks = 11 }
        }).Passed);

        Assert.IsFalse((passing with
        {
            CredentialStore = passing.CredentialStore with { Passed = false, PassedChecks = 6 }
        }).Passed);

        Assert.IsFalse((passing with
        {
            Steam = passing.Steam with { ConnectedCallbackReceived = false }
        }).Passed);
    }

    private static FoundationVerificationResult MakePassingResult()
    {
        var core = new CoreSelfTestResult(true, 12, 12, "CORE SELF-TEST PASS — 12/12");
        var credentials = new CredentialStoreVerificationResult(true, 7, 7, "CREDENTIAL STORE PASS — 7/7");
        var steam = new SteamConnectionProbeResult(
            TransportName: "WebSocket/SocketsHttpHandler",
            Protocols: "WebSocket",
            ClientConstructed: true,
            ConnectedCallbackReceived: true,
            DisconnectedCallbackReceived: true,
            DisconnectedUserInitiated: true,
            IsConnectedEver: true,
            LastCurrentEndPoint: "cm.example:443",
            CmWebSocketFactoryUsed: true,
            SteamKitAssemblyVersion: "3.4.0.0",
            Elapsed: TimeSpan.FromMilliseconds(100),
            Error: null);

        return new FoundationVerificationResult(
            UiStartupPassed: true,
            LifecycleActive: true,
            Core: core,
            CredentialStore: credentials,
            Steam: steam);
    }
}
