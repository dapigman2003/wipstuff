using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests.Runtime;

[TestClass]
public sealed class TransformedRealStS2GameLifecycleEntryTests
{
    [TestMethod]
    public void GateSequenceCompletesFourOfFourInOrder()
    {
        var gates = new TransformedRealStS2GameLifecycleEntryGateSequence();
        gates.Record(new(TransformedRealStS2GameLifecycleEntryGate.LifecycleStaticAudit, true, "a"));
        gates.Record(new(TransformedRealStS2GameLifecycleEntryGate.OffTreeNGameReinstantiation, true, "b"));
        gates.Record(new(TransformedRealStS2GameLifecycleEntryGate.DirectEnterTreeInvocation, true, "c"));
        gates.Record(new(TransformedRealStS2GameLifecycleEntryGate.PostEnterTreeConfinementAndRelease, true, "d"));

        var snapshot = gates.Snapshot();
        Assert.IsTrue(snapshot.Passed);
        Assert.AreEqual("STEP 38.0.1 CONTROLLED NGAME _ENTERTREE ENTRY COMPLETE — 4/4", snapshot.Summary);
        Assert.AreEqual(4, snapshot.Gates.Count);
    }

    [TestMethod]
    public void GateSequenceRejectsOutOfOrderAdvance()
    {
        var gates = new TransformedRealStS2GameLifecycleEntryGateSequence();
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2GameLifecycleEntryGate.DirectEnterTreeInvocation, true, "bad")));
    }

    [TestMethod]
    public void LifecycleBoundaryNamesArePinned()
    {
        Assert.AreEqual("MegaCrit.Sts2.Core.Nodes.NGame", TransformedRealStS2VeryEarlyInitialization.NGameTypeFullName);
        Assert.AreEqual("_EnterTree", TransformedRealStS2VeryEarlyInitialization.NGameEnterTreeMethodName);
        Assert.AreEqual("_Ready", TransformedRealStS2VeryEarlyInitialization.NGameReadyMethodName);
        Assert.AreEqual("GameStartupWrapper", TransformedRealStS2VeryEarlyInitialization.NGameGameStartupWrapperMethodName);
        Assert.AreEqual("GameStartup", TransformedRealStS2VeryEarlyInitialization.NGameGameStartupMethodName);
        Assert.AreEqual("InitializePlatform", TransformedRealStS2VeryEarlyInitialization.NGameInitializePlatformMethodName);
        Assert.AreEqual("LaunchMainMenu", TransformedRealStS2VeryEarlyInitialization.NGameLaunchMainMenuMethodName);
        Assert.AreEqual("LoadDeferredStartupAssetsAsync", TransformedRealStS2VeryEarlyInitialization.NGameLoadDeferredStartupAssetsMethodName);
    }

    [TestMethod]
    public void Step38GateOrdinalsAreStable()
    {
        Assert.AreEqual(1, (int)TransformedRealStS2GameLifecycleEntryGate.LifecycleStaticAudit);
        Assert.AreEqual(2, (int)TransformedRealStS2GameLifecycleEntryGate.OffTreeNGameReinstantiation);
        Assert.AreEqual(3, (int)TransformedRealStS2GameLifecycleEntryGate.DirectEnterTreeInvocation);
        Assert.AreEqual(4, (int)TransformedRealStS2GameLifecycleEntryGate.PostEnterTreeConfinementAndRelease);
    }
}
