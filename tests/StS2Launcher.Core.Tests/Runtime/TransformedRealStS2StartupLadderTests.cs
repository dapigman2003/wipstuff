using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests.Runtime;

[TestClass]
public sealed class TransformedRealStS2StartupLadderTests
{
    [TestMethod]
    public void GateSequenceCompletesFourOfFourInOrder()
    {
        var gates = new TransformedRealStS2StartupLadderGateSequence(46, "LAUNCHMAINMENU ASYNC MAP");
        gates.Record(new(46, "LAUNCHMAINMENU ASYNC MAP", TransformedRealStS2StartupLadderGate.PrerequisiteAuthority, true, "a"));
        gates.Record(new(46, "LAUNCHMAINMENU ASYNC MAP", TransformedRealStS2StartupLadderGate.StaticAuditOrBinding, true, "b"));
        gates.Record(new(46, "LAUNCHMAINMENU ASYNC MAP", TransformedRealStS2StartupLadderGate.ControlledAction, true, "c"));
        gates.Record(new(46, "LAUNCHMAINMENU ASYNC MAP", TransformedRealStS2StartupLadderGate.PostActionConfinement, true, "d"));

        var snapshot = gates.Snapshot();
        Assert.IsTrue(snapshot.Passed);
        Assert.AreEqual("STEP 46.0 LAUNCHMAINMENU ASYNC MAP COMPLETE — 4/4", snapshot.Summary);
        Assert.AreEqual(4, snapshot.Gates.Count);
    }

    [TestMethod]
    public void GateSequenceRejectsOutOfOrderAdvance()
    {
        var gates = new TransformedRealStS2StartupLadderGateSequence(43, "NULL PLATFORM AUTHORITY");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(43, "NULL PLATFORM AUTHORITY", TransformedRealStS2StartupLadderGate.ControlledAction, true, "bad")));
    }

    [TestMethod]
    public void GateSequenceRejectsWrongStepOrName()
    {
        var gates = new TransformedRealStS2StartupLadderGateSequence(43, "NULL PLATFORM AUTHORITY");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(44, "NULL PLATFORM AUTHORITY", TransformedRealStS2StartupLadderGate.PrerequisiteAuthority, true, "bad-step")));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(43, "WRONG", TransformedRealStS2StartupLadderGate.PrerequisiteAuthority, true, "bad-name")));
    }

    [TestMethod]
    public void GateSequenceRejectsAdvanceAfterFailure()
    {
        var gates = new TransformedRealStS2StartupLadderGateSequence(47, "CONTROLLED LAUNCHMAINMENU");
        gates.Record(new(47, "CONTROLLED LAUNCHMAINMENU", TransformedRealStS2StartupLadderGate.PrerequisiteAuthority, false, "fail"));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(47, "CONTROLLED LAUNCHMAINMENU", TransformedRealStS2StartupLadderGate.StaticAuditOrBinding, true, "bad")));
    }

    [TestMethod]
    public void GateOrdinalsAreStable()
    {
        Assert.AreEqual(1, (int)TransformedRealStS2StartupLadderGate.PrerequisiteAuthority);
        Assert.AreEqual(2, (int)TransformedRealStS2StartupLadderGate.StaticAuditOrBinding);
        Assert.AreEqual(3, (int)TransformedRealStS2StartupLadderGate.ControlledAction);
        Assert.AreEqual(4, (int)TransformedRealStS2StartupLadderGate.PostActionConfinement);
    }
}
