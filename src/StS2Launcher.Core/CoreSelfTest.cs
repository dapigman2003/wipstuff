namespace StS2Launcher.Core;

public sealed record CoreSelfTestResult(
    bool Passed,
    int PassedChecks,
    int TotalChecks,
    string Summary);

public static class CoreSelfTest
{
    public static CoreSelfTestResult Run()
    {
        var passed = 0;
        const int total = 12;

        var controller = new LauncherController();

        if (controller.State == LauncherState.SignedOut)
            passed++;

        if (controller.Snapshot.StateNumber == 1 &&
            controller.Snapshot.StateCount == LauncherController.StateCount)
            passed++;

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
        {
            var snapshot = controller.NextDemoState();
            if (snapshot.State == state)
                passed++;
        }

        var download = LauncherController.CreateSnapshot(LauncherState.Downloading);
        if (download.ShowProgress && Math.Abs(download.Progress - 0.42f) < 0.0001f)
            passed++;

        controller.NextDemoState(); // SignedOut -> Authenticating.
        var reset = controller.Reset();
        if (reset.State == LauncherState.SignedOut)
            passed++;

        var primary = controller.DescribePrimaryAction();
        if (primary.StartsWith("PASS:", StringComparison.Ordinal))
            passed++;

        return new CoreSelfTestResult(
            Passed: passed == total,
            PassedChecks: passed,
            TotalChecks: total,
            Summary: passed == total
                ? $"CORE SELF-TEST PASS — {passed}/{total}"
                : $"CORE SELF-TEST FAIL — {passed}/{total}");
    }
}
