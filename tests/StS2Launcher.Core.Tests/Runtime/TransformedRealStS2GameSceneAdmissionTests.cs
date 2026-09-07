using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests.Runtime;

[TestClass]
public sealed class TransformedRealStS2GameSceneAdmissionTests
{
    [TestMethod]
    public void GateSequenceCompletesFourOfFourInOrder()
    {
        var gates = new TransformedRealStS2GameSceneAdmissionGateSequence();
        gates.Record(new(TransformedRealStS2GameSceneAdmissionGate.ClosedEssentialAndSealedScenePreflight, true, "a"));
        gates.Record(new(TransformedRealStS2GameSceneAdmissionGate.FmodNeutralDerivativePreparation, true, "b"));
        gates.Record(new(TransformedRealStS2GameSceneAdmissionGate.PackedSceneLoad, true, "c"));
        gates.Record(new(TransformedRealStS2GameSceneAdmissionGate.OffTreeInstantiationAudit, true, "d"));

        var snapshot = gates.Snapshot();
        Assert.IsTrue(snapshot.Passed);
        Assert.AreEqual("STEP 37.0 CONTROLLED GAME-SCENE ADMISSION COMPLETE — 4/4", snapshot.Summary);
        Assert.AreEqual(4, snapshot.Gates.Count);
    }

    [TestMethod]
    public void GateSequenceRejectsOutOfOrderAdvance()
    {
        var gates = new TransformedRealStS2GameSceneAdmissionGateSequence();
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2GameSceneAdmissionGate.PackedSceneLoad, true, "bad")));
    }

    [TestMethod]
    public void SealedGameSceneAuthorityConstantsArePinned()
    {
        Assert.AreEqual("res://scenes/game.tscn", TransformedRealStS2VeryEarlyInitialization.GameSceneResourcePath);
        Assert.AreEqual(10_414, TransformedRealStS2VeryEarlyInitialization.ClosedGameSceneBytes);
        Assert.AreEqual("aaec1e04f689122fd812b83fee09ea6e30e2991cf5851dc599b01b7802320ad8", TransformedRealStS2VeryEarlyInitialization.ClosedGameSceneSha256);
        Assert.AreEqual("dc9a89798eb1bb38e23563866e746ac5", TransformedRealStS2VeryEarlyInitialization.ClosedGameSceneMd5);
        Assert.AreEqual(3u, TransformedRealStS2VeryEarlyInitialization.ClosedPckFormat);
        Assert.AreEqual(4u, TransformedRealStS2VeryEarlyInitialization.ClosedPckEngineMajor);
        Assert.AreEqual(5u, TransformedRealStS2VeryEarlyInitialization.ClosedPckEngineMinor);
        Assert.AreEqual(1u, TransformedRealStS2VeryEarlyInitialization.ClosedPckEnginePatch);
        Assert.AreEqual(0x00000002u, TransformedRealStS2VeryEarlyInitialization.ClosedPckFlags);
        Assert.AreEqual(12_328u, TransformedRealStS2VeryEarlyInitialization.ClosedPckDirectoryEntries);
    }

    [TestMethod]
    public void Step37GateOrdinalsAreStable()
    {
        Assert.AreEqual(1, (int)TransformedRealStS2GameSceneAdmissionGate.ClosedEssentialAndSealedScenePreflight);
        Assert.AreEqual(2, (int)TransformedRealStS2GameSceneAdmissionGate.FmodNeutralDerivativePreparation);
        Assert.AreEqual(3, (int)TransformedRealStS2GameSceneAdmissionGate.PackedSceneLoad);
        Assert.AreEqual(4, (int)TransformedRealStS2GameSceneAdmissionGate.OffTreeInstantiationAudit);
    }
}
