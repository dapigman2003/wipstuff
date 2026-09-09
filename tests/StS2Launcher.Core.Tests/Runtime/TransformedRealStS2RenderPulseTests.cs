using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests.Runtime;

[TestClass]
public sealed class TransformedRealStS2RenderPulseTests
{
    [TestMethod]
    public void GateSequenceCompletesFourOfFourInOrder()
    {
        var gates = new TransformedRealStS2RenderPulseGateSequence();
        gates.Record(new(TransformedRealStS2RenderPulseGate.ClosedStep39FrozenAuthority, true, "a"));
        gates.Record(new(TransformedRealStS2RenderPulseGate.FrameDrivenManagedSurfaceAudit, true, "b"));
        gates.Record(new(TransformedRealStS2RenderPulseGate.BoundedRenderPulseAndRefreeze, true, "c"));
        gates.Record(new(TransformedRealStS2RenderPulseGate.FrozenPostPulseConfinement, true, "d"));

        var snapshot = gates.Snapshot();
        Assert.IsTrue(snapshot.Passed);
        Assert.AreEqual("STEP 40.0 CONTROLLED REAL-GAME RENDER PULSE COMPLETE — 4/4", snapshot.Summary);
        Assert.AreEqual(4, snapshot.Gates.Count);
    }

    [TestMethod]
    public void GateSequenceRejectsOutOfOrderAdvance()
    {
        var gates = new TransformedRealStS2RenderPulseGateSequence();
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2RenderPulseGate.BoundedRenderPulseAndRefreeze, true, "bad")));
    }

    [TestMethod]
    public void GateSequenceRejectsAdvanceAfterFailure()
    {
        var gates = new TransformedRealStS2RenderPulseGateSequence();
        gates.Record(new(TransformedRealStS2RenderPulseGate.ClosedStep39FrozenAuthority, false, "fail"));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2RenderPulseGate.FrameDrivenManagedSurfaceAudit, true, "bad")));
    }

    [TestMethod]
    public void Step40GateOrdinalsAreStable()
    {
        Assert.AreEqual(1, (int)TransformedRealStS2RenderPulseGate.ClosedStep39FrozenAuthority);
        Assert.AreEqual(2, (int)TransformedRealStS2RenderPulseGate.FrameDrivenManagedSurfaceAudit);
        Assert.AreEqual(3, (int)TransformedRealStS2RenderPulseGate.BoundedRenderPulseAndRefreeze);
        Assert.AreEqual(4, (int)TransformedRealStS2RenderPulseGate.FrozenPostPulseConfinement);
    }

    [TestMethod]
    public void Step40PulseWindowIsNarrowAndPinned()
    {
        Assert.AreEqual(100, TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseTargetMilliseconds);
        Assert.AreEqual(500, TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseMaximumMilliseconds);
        Assert.IsTrue(TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseMaximumMilliseconds >=
                      TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseTargetMilliseconds);
    }
}
