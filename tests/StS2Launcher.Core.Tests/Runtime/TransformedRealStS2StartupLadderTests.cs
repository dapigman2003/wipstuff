using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests.Runtime;

[TestClass]
public sealed class TransformedRealStS2StartupLadderTests
{
    [TestMethod]
    public void GateSequenceCompletesFourOfFourInOrder()
    {
        var gates = new TransformedRealStS2StartupLadderGateSequence(46, "LAUNCHMAINMENU IMMEDIATE FRONTIER MAP");
        gates.Record(new(46, "LAUNCHMAINMENU IMMEDIATE FRONTIER MAP", TransformedRealStS2StartupLadderGate.PrerequisiteAuthority, true, "a"));
        gates.Record(new(46, "LAUNCHMAINMENU IMMEDIATE FRONTIER MAP", TransformedRealStS2StartupLadderGate.StaticAuditOrBinding, true, "b"));
        gates.Record(new(46, "LAUNCHMAINMENU IMMEDIATE FRONTIER MAP", TransformedRealStS2StartupLadderGate.ControlledAction, true, "c"));
        gates.Record(new(46, "LAUNCHMAINMENU IMMEDIATE FRONTIER MAP", TransformedRealStS2StartupLadderGate.PostActionConfinement, true, "d"));

        var snapshot = gates.Snapshot();
        Assert.IsTrue(snapshot.Passed);
        Assert.AreEqual("STEP 46.0 LAUNCHMAINMENU IMMEDIATE FRONTIER MAP COMPLETE — 4/4", snapshot.Summary);
        Assert.AreEqual(4, snapshot.Gates.Count);
    }

    [TestMethod]
    public void DirectMainMenuRungNamesProduceDistinctFourOfFourSummaries()
    {
        var rungs = new[]
        {
            (47, "MAIN MENU RESOURCE PREPARATION"),
            (48, "MAIN MENU OFF-TREE INSTANTIATION"),
            (49, "MAIN MENU FROZEN SCENETREE ADMISSION"),
            (50, "MAIN MENU CONTROLLED RENDER PULSE"),
            (51, "SINGLEPLAYER FRONTIER MAP"),
            (52, "MAIN MENU SUSTAINED RENDER RESIDENCY"),
            (53, "SINGLEPLAYER OPEN FRONTIER"),
            (54, "SINGLEPLAYER SUBMENU FROZEN OPEN"),
            (55, "SINGLEPLAYER SUBMENU RENDER RESIDENCY"),
            (56, "CHARACTER SELECT FRONTIER MAP"),
            (57, "CHARACTER SELECT RESOURCE PREFLIGHT"),
            (58, "CHARACTER SELECT FACTORY FRONTIER"),
            (59, "CHARACTER SELECT FROZEN OFF-TREE CREATION"),
            (60, "CHARACTER SELECT INITIALIZE FRONTIER"),
            (61, "CHARACTER SELECT OFF-TREE INITIALIZATION"),
            (62, "CHARACTER SELECT PUSH FRONTIER"),
            (63, "CHARACTER SELECT FROZEN SCENETREE ADMISSION"),
            (64, "CHARACTER SELECT RENDER RESIDENCY")
        };

        var summaries = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (step, name) in rungs)
        {
            var gates = new TransformedRealStS2StartupLadderGateSequence(step, name);
            gates.Record(new(step, name, TransformedRealStS2StartupLadderGate.PrerequisiteAuthority, true, "a"));
            gates.Record(new(step, name, TransformedRealStS2StartupLadderGate.StaticAuditOrBinding, true, "b"));
            gates.Record(new(step, name, TransformedRealStS2StartupLadderGate.ControlledAction, true, "c"));
            gates.Record(new(step, name, TransformedRealStS2StartupLadderGate.PostActionConfinement, true, "d"));
            var snapshot = gates.Snapshot();
            Assert.IsTrue(snapshot.Passed);
            Assert.IsTrue(summaries.Add(snapshot.Summary));
        }

        Assert.AreEqual(18, summaries.Count);
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
        var gates = new TransformedRealStS2StartupLadderGateSequence(50, "MAIN MENU CONTROLLED RENDER PULSE");
        gates.Record(new(50, "MAIN MENU CONTROLLED RENDER PULSE", TransformedRealStS2StartupLadderGate.PrerequisiteAuthority, false, "fail"));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(50, "MAIN MENU CONTROLLED RENDER PULSE", TransformedRealStS2StartupLadderGate.StaticAuditOrBinding, true, "bad")));
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
