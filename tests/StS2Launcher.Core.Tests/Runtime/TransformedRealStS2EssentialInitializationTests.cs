using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests.Runtime;

[TestClass]
public sealed class TransformedRealStS2EssentialInitializationTests
{
    [TestMethod]
    public void GateSequenceCompletesFourOfFourInOrder()
    {
        var gates = new TransformedRealStS2EssentialInitializationGateSequence();
        gates.Record(new(TransformedRealStS2EssentialInitializationGate.ExactStep35ClosureAndStaticPreflight, true, "a"));
        gates.Record(new(TransformedRealStS2EssentialInitializationGate.ExactAuthorityContinuityAndBinding, true, "b"));
        gates.Record(new(TransformedRealStS2EssentialInitializationGate.ExecuteEssentialInvocation, true, "c"));
        gates.Record(new(TransformedRealStS2EssentialInitializationGate.FinalIsolationAudit, true, "d"));

        var snapshot = gates.Snapshot();
        Assert.IsTrue(snapshot.Passed);
        Assert.AreEqual("STEP 36.0 ESSENTIAL INITIALIZATION COMPLETE — 4/4", snapshot.Summary);
        Assert.AreEqual(4, snapshot.Gates.Count);
    }

    [TestMethod]
    public void GateSequenceRejectsOutOfOrderAdvance()
    {
        var gates = new TransformedRealStS2EssentialInitializationGateSequence();
        try
        {
            gates.Record(new(TransformedRealStS2EssentialInitializationGate.ExecuteEssentialInvocation, true, "bad"));
            Assert.Fail("Expected out-of-order Step 36 gate recording to throw InvalidOperationException.");
        }
        catch (InvalidOperationException)
        {
            // Expected: the four-gate sequence remains strictly ordered.
        }
    }

    [TestMethod]
    public void ExecuteEssentialAuthorityConstantsArePinned()
    {
        Assert.AreEqual("ExecuteEssential", TransformedRealStS2VeryEarlyInitialization.EssentialTargetMethodName);
        Assert.AreEqual(
            "System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteEssential()",
            TransformedRealStS2VeryEarlyInitialization.EssentialTargetMethodFullName);
        Assert.AreEqual(0x06007D03u, TransformedRealStS2VeryEarlyInitialization.SourceEssentialTargetMethodToken);
        Assert.AreEqual(1, TransformedRealStS2VeryEarlyInitialization.ExpectedStateAfterVeryEarly);
        Assert.AreEqual(2, TransformedRealStS2VeryEarlyInitialization.ExpectedStateAfterEssential);
    }

    [TestMethod]
    public void Step36GateOrdinalsAreStable()
    {
        Assert.AreEqual(1, (int)TransformedRealStS2EssentialInitializationGate.ExactStep35ClosureAndStaticPreflight);
        Assert.AreEqual(2, (int)TransformedRealStS2EssentialInitializationGate.ExactAuthorityContinuityAndBinding);
        Assert.AreEqual(3, (int)TransformedRealStS2EssentialInitializationGate.ExecuteEssentialInvocation);
        Assert.AreEqual(4, (int)TransformedRealStS2EssentialInitializationGate.FinalIsolationAudit);
    }
}
