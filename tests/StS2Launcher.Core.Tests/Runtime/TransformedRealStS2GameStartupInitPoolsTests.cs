using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests.Runtime;

[TestClass]
public sealed class TransformedRealStS2GameStartupInitPoolsTests
{
    [TestMethod]
    public void GateSequenceCompletesFourOfFourInOrder()
    {
        var gates = new TransformedRealStS2GameStartupInitPoolsGateSequence();
        gates.Record(new(TransformedRealStS2GameStartupInitPoolsGate.ClosedStep41FrozenAuthority, true, "a"));
        gates.Record(new(TransformedRealStS2GameStartupInitPoolsGate.InitPoolsStaticClosureAudit, true, "b"));
        gates.Record(new(TransformedRealStS2GameStartupInitPoolsGate.ControlledInitPoolsInvocation, true, "c"));
        gates.Record(new(TransformedRealStS2GameStartupInitPoolsGate.FrozenPostInitPoolsConfinement, true, "d"));

        var snapshot = gates.Snapshot();
        Assert.IsTrue(snapshot.Passed);
        Assert.AreEqual("STEP 42.0 CONTROLLED GAMESTARTUP INITPOOLS COMPLETE — 4/4", snapshot.Summary);
        Assert.AreEqual(4, snapshot.Gates.Count);
    }

    [TestMethod]
    public void GateSequenceRejectsOutOfOrderAdvance()
    {
        var gates = new TransformedRealStS2GameStartupInitPoolsGateSequence();
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2GameStartupInitPoolsGate.ControlledInitPoolsInvocation, true, "bad")));
    }

    [TestMethod]
    public void GateSequenceRejectsAdvanceAfterFailure()
    {
        var gates = new TransformedRealStS2GameStartupInitPoolsGateSequence();
        gates.Record(new(TransformedRealStS2GameStartupInitPoolsGate.ClosedStep41FrozenAuthority, false, "fail"));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2GameStartupInitPoolsGate.InitPoolsStaticClosureAudit, true, "bad")));
    }

    [TestMethod]
    public void GateOrdinalsAreStable()
    {
        Assert.AreEqual(1, (int)TransformedRealStS2GameStartupInitPoolsGate.ClosedStep41FrozenAuthority);
        Assert.AreEqual(2, (int)TransformedRealStS2GameStartupInitPoolsGate.InitPoolsStaticClosureAudit);
        Assert.AreEqual(3, (int)TransformedRealStS2GameStartupInitPoolsGate.ControlledInitPoolsInvocation);
        Assert.AreEqual(4, (int)TransformedRealStS2GameStartupInitPoolsGate.FrozenPostInitPoolsConfinement);
    }
}
