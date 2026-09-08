using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests.Runtime;

[TestClass]
public sealed class TransformedRealStS2SceneTreeAdmissionTests
{
    [TestMethod]
    public void GateSequenceCompletesFourOfFourInOrder()
    {
        var gates = new TransformedRealStS2SceneTreeAdmissionGateSequence();
        gates.Record(new(TransformedRealStS2SceneTreeAdmissionGate.PreInsertionAuthorityAndResourceAudit, true, "a"));
        gates.Record(new(TransformedRealStS2SceneTreeAdmissionGate.OffTreeHierarchyAndLifecycleSurfaceAudit, true, "b"));
        gates.Record(new(TransformedRealStS2SceneTreeAdmissionGate.RealSceneTreeInsertion, true, "c"));
        gates.Record(new(TransformedRealStS2SceneTreeAdmissionGate.FrozenPostInsertionConfinement, true, "d"));

        var snapshot = gates.Snapshot();
        Assert.IsTrue(snapshot.Passed);
        Assert.AreEqual("STEP 39.0 REAL SCENETREE ADMISSION COMPLETE — 4/4", snapshot.Summary);
        Assert.AreEqual(4, snapshot.Gates.Count);
    }

    [TestMethod]
    public void GateSequenceRejectsOutOfOrderAdvance()
    {
        var gates = new TransformedRealStS2SceneTreeAdmissionGateSequence();
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2SceneTreeAdmissionGate.RealSceneTreeInsertion, true, "bad")));
    }

    [TestMethod]
    public void GateSequenceRejectsAdvanceAfterFailure()
    {
        var gates = new TransformedRealStS2SceneTreeAdmissionGateSequence();
        gates.Record(new(TransformedRealStS2SceneTreeAdmissionGate.PreInsertionAuthorityAndResourceAudit, false, "fail"));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2SceneTreeAdmissionGate.OffTreeHierarchyAndLifecycleSurfaceAudit, true, "bad")));
    }

    [TestMethod]
    public void Step39GateOrdinalsAreStable()
    {
        Assert.AreEqual(1, (int)TransformedRealStS2SceneTreeAdmissionGate.PreInsertionAuthorityAndResourceAudit);
        Assert.AreEqual(2, (int)TransformedRealStS2SceneTreeAdmissionGate.OffTreeHierarchyAndLifecycleSurfaceAudit);
        Assert.AreEqual(3, (int)TransformedRealStS2SceneTreeAdmissionGate.RealSceneTreeInsertion);
        Assert.AreEqual(4, (int)TransformedRealStS2SceneTreeAdmissionGate.FrozenPostInsertionConfinement);
    }

    [TestMethod]
    public void Step39PreflightAuthoritiesArePinned()
    {
        Assert.AreEqual("res://src/gdscript/audio_manager_proxy.gd.remap", TransformedRealStS2VeryEarlyInitialization.Step39AudioProxyRemapPath);
        Assert.AreEqual("res://src/gdscript/audio_manager_proxy.gdc", TransformedRealStS2VeryEarlyInitialization.Step39AudioProxyGdcPath);
        Assert.AreEqual("res://scenes/ui/reaction_wheel.tscn", TransformedRealStS2VeryEarlyInitialization.Step39ReactionWheelPath);
        Assert.AreEqual("res://scenes/ui/multiplayer_timeout_overlay.tscn", TransformedRealStS2VeryEarlyInitialization.Step39TimeoutOverlayPath);
        Assert.AreEqual("4fc30d10a67bd68d1ba57bc4bf214218fb8b6375e2df60341b86f960f094f1bb", TransformedRealStS2VeryEarlyInitialization.Step39AudioProxyGdcSha256);
        Assert.AreEqual((uint)101, TransformedRealStS2VeryEarlyInitialization.Step39AudioProxyGdcTokenizerVersion);
        Assert.AreEqual((uint)9_828, TransformedRealStS2VeryEarlyInitialization.Step39AudioProxyGdcDecompressedBytes);
        Assert.AreEqual(69, TransformedRealStS2VeryEarlyInitialization.Step39AuditedAudioProxyIdentifierCount);
        Assert.AreEqual("0927e9debca56fe6726b682debe2037928be34f1d363b04ea6f18d97430028d5", TransformedRealStS2VeryEarlyInitialization.Step39ReactionWheelSha256);
        Assert.AreEqual("f32ce3ff57698802566ff36ee3c11e84f586bdfa72e5e13a467a148117bb6dee", TransformedRealStS2VeryEarlyInitialization.Step39TimeoutOverlaySha256);
    }
}
