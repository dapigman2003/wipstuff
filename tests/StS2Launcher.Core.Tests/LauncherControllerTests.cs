using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class LauncherControllerTests
{
    [TestMethod]
    public void StartsSignedOutWithFirstSnapshot()
    {
        var controller = new LauncherController();

        Assert.AreEqual(LauncherState.SignedOut, controller.State);
        Assert.AreEqual(1, controller.Snapshot.StateNumber);
        Assert.AreEqual(LauncherController.StateCount, controller.Snapshot.StateCount);
    }

    [TestMethod]
    public void DemoStateCycleVisitsEveryStateAndWraps()
    {
        var controller = new LauncherController();
        var expected = new[]
        {
            LauncherState.Authenticating,
            LauncherState.CheckingOwnership,
            LauncherState.ReadyToInstall,
            LauncherState.Downloading,
            LauncherState.ReadyToPlay,
            LauncherState.Error,
            LauncherState.SignedOut
        };

        foreach (var state in expected)
            Assert.AreEqual(state, controller.NextDemoState().State);
    }

    [TestMethod]
    public void DownloadSnapshotHasExpectedProgressContract()
    {
        var snapshot = LauncherController.CreateSnapshot(LauncherState.Downloading);

        Assert.IsTrue(snapshot.ShowProgress);
        Assert.AreEqual(0.42f, snapshot.Progress, 0.0001f);
        Assert.AreEqual("Downloading…", snapshot.Title);
    }

    [TestMethod]
    public void NonDownloadSnapshotsDoNotShowProgress()
    {
        foreach (var state in Enum.GetValues<LauncherState>())
        {
            if (state == LauncherState.Downloading)
                continue;

            var snapshot = LauncherController.CreateSnapshot(state);
            Assert.IsFalse(snapshot.ShowProgress, state.ToString());
            Assert.AreEqual(0.0f, snapshot.Progress, 0.0001f, state.ToString());
        }
    }

    [TestMethod]
    public void ResetAlwaysReturnsToSignedOut()
    {
        var controller = new LauncherController();
        controller.NextDemoState();
        controller.NextDemoState();

        var snapshot = controller.Reset();

        Assert.AreEqual(LauncherState.SignedOut, snapshot.State);
        Assert.AreEqual(LauncherState.SignedOut, controller.State);
    }

    [TestMethod]
    public void EveryStateProducesCompleteUiText()
    {
        foreach (var state in Enum.GetValues<LauncherState>())
        {
            var snapshot = LauncherController.CreateSnapshot(state);
            Assert.IsFalse(string.IsNullOrWhiteSpace(snapshot.Title), state.ToString());
            Assert.IsFalse(string.IsNullOrWhiteSpace(snapshot.Detail), state.ToString());
            Assert.IsFalse(string.IsNullOrWhiteSpace(snapshot.PrimaryActionTitle), state.ToString());
        }
    }

    [TestMethod]
    public void ExistingCoreRegressionSelfTestStillPassesTwelveOfTwelve()
    {
        var result = CoreSelfTest.Run();

        Assert.IsTrue(result.Passed);
        Assert.AreEqual(12, result.PassedChecks);
        Assert.AreEqual(12, result.TotalChecks);
    }
}
