using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class GodotFoundationGateTests
{
    [TestMethod]
    public void OrderedGodotFoundationGatesReachFourOfFourPass()
    {
        var sequence = new GodotFoundationGateSequence();
        sequence.Record(GodotFoundationGate.NativeAvailability, true, "Godot 4.5.1 static bridge resolved.");
        sequence.Record(GodotFoundationGate.EngineInitializeRenderLoop, true, "Engine initialized and render loop stopped/restarted.");
        sequence.Record(GodotFoundationGate.MetalRender, true, "Metal layer + project-owned render marker observed.");
        sequence.Record(GodotFoundationGate.TouchLifecycle, true, "Touch and background/foreground callbacks observed.");

        var summary = sequence.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.IsNull(summary.FirstFailingGate);
        Assert.AreEqual("GODOT FOUNDATION PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void GodotFoundationStopsAtFirstFailingGate()
    {
        var sequence = new GodotFoundationGateSequence();
        sequence.Record(GodotFoundationGate.NativeAvailability, true, "A pass");
        sequence.Record(GodotFoundationGate.EngineInitializeRenderLoop, true, "B pass");
        sequence.Record(GodotFoundationGate.MetalRender, false, "C failed");

        var summary = sequence.Snapshot();
        Assert.IsFalse(summary.Passed);
        Assert.AreEqual(GodotFoundationGate.MetalRender, summary.FirstFailingGate);
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            sequence.Record(GodotFoundationGate.TouchLifecycle, true, "must not advance"));
    }

    [TestMethod]
    public void GodotFoundationRejectsOutOfOrderGate()
    {
        var sequence = new GodotFoundationGateSequence();
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            sequence.Record(GodotFoundationGate.MetalRender, true, "out of order"));
    }

    [TestMethod]
    public void GodotFoundationCanResetForFreshProcessRun()
    {
        var sequence = new GodotFoundationGateSequence();
        sequence.Record(GodotFoundationGate.NativeAvailability, false, "first attempt failed");
        sequence.Reset();
        sequence.Record(GodotFoundationGate.NativeAvailability, true, "fresh attempt passed");

        var summary = sequence.Snapshot();
        Assert.AreEqual(1, summary.PassedGates);
        Assert.IsNull(summary.FirstFailingGate);
        Assert.IsFalse(summary.Passed);
    }
}
