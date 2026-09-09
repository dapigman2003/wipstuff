using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests.Runtime;

[TestClass]
public sealed class TransformedRealStS2GameStartupFrontierTests
{
    [TestMethod]
    public void GateSequenceCompletesFourOfFourInOrder()
    {
        var gates = new TransformedRealStS2GameStartupFrontierGateSequence();
        gates.Record(new(TransformedRealStS2GameStartupFrontierGate.ClosedStep40FrozenAuthority, true, "a"));
        gates.Record(new(TransformedRealStS2GameStartupFrontierGate.GameStartupAsyncStateMachineMap, true, "b"));
        gates.Record(new(TransformedRealStS2GameStartupFrontierGate.TransitiveStartupBoundaryMap, true, "c"));
        gates.Record(new(TransformedRealStS2GameStartupFrontierGate.FrozenNoInvocationConfinement, true, "d"));

        var snapshot = gates.Snapshot();
        Assert.IsTrue(snapshot.Passed);
        Assert.AreEqual("STEP 41.0 GAMESTARTUP ASYNC FRONTIER MAP COMPLETE — 4/4", snapshot.Summary);
        Assert.AreEqual(4, snapshot.Gates.Count);
    }

    [TestMethod]
    public void GateSequenceRejectsOutOfOrderAdvance()
    {
        var gates = new TransformedRealStS2GameStartupFrontierGateSequence();
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2GameStartupFrontierGate.TransitiveStartupBoundaryMap, true, "bad")));
    }

    [TestMethod]
    public void GateSequenceRejectsAdvanceAfterFailure()
    {
        var gates = new TransformedRealStS2GameStartupFrontierGateSequence();
        gates.Record(new(TransformedRealStS2GameStartupFrontierGate.ClosedStep40FrozenAuthority, false, "fail"));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2GameStartupFrontierGate.GameStartupAsyncStateMachineMap, true, "bad")));
    }

    [TestMethod]
    public void GateOrdinalsAreStable()
    {
        Assert.AreEqual(1, (int)TransformedRealStS2GameStartupFrontierGate.ClosedStep40FrozenAuthority);
        Assert.AreEqual(2, (int)TransformedRealStS2GameStartupFrontierGate.GameStartupAsyncStateMachineMap);
        Assert.AreEqual(3, (int)TransformedRealStS2GameStartupFrontierGate.TransitiveStartupBoundaryMap);
        Assert.AreEqual(4, (int)TransformedRealStS2GameStartupFrontierGate.FrozenNoInvocationConfinement);
    }
}
