namespace StS2Launcher.Core;

public sealed class LauncherController
{
    public const int StateCount = 7;

    public LauncherState State { get; private set; } = LauncherState.SignedOut;

    public LauncherSnapshot Snapshot => CreateSnapshot(State);

    public LauncherSnapshot NextDemoState()
    {
        State = State == LauncherState.Error
            ? LauncherState.SignedOut
            : State + 1;

        return Snapshot;
    }

    public LauncherSnapshot Reset()
    {
        State = LauncherState.SignedOut;
        return Snapshot;
    }

    public string DescribePrimaryAction()
    {
        return $"PASS: Core handled primary action in state {State}. No service call was made.";
    }

    public static LauncherSnapshot CreateSnapshot(LauncherState state)
    {
        return new LauncherSnapshot(
            State: state,
            Title: Title(state),
            Detail: Detail(state),
            PrimaryActionTitle: PrimaryActionTitle(state),
            Progress: state == LauncherState.Downloading ? 0.42f : 0.0f,
            ShowProgress: state == LauncherState.Downloading,
            StateNumber: (int)state + 1,
            StateCount: StateCount);
    }

    private static string Title(LauncherState state) => state switch
    {
        LauncherState.SignedOut => "Signed out",
        LauncherState.Authenticating => "Signing in…",
        LauncherState.CheckingOwnership => "Checking ownership…",
        LauncherState.ReadyToInstall => "Ready to install",
        LauncherState.Downloading => "Downloading…",
        LauncherState.ReadyToPlay => "Ready to play",
        LauncherState.Error => "Example error",
        _ => "Unknown"
    };

    private static string Detail(LauncherState state) => state switch
    {
        LauncherState.SignedOut =>
            "Steam is not connected. This is mock UI only.",
        LauncherState.Authenticating =>
            "Pretending to authenticate with Steam.",
        LauncherState.CheckingOwnership =>
            "Pretending to verify Slay the Spire 2 ownership.",
        LauncherState.ReadyToInstall =>
            "Ownership verified. Game files are not installed.",
        LauncherState.Downloading =>
            "Pretending to download game files. Progress should show 42%.",
        LauncherState.ReadyToPlay =>
            "Mock installation is ready. Play is intentionally disabled in Step 03.",
        LauncherState.Error =>
            "TEST ERROR: This is a deliberate visible error state.",
        _ => "Unknown state."
    };

    private static string PrimaryActionTitle(LauncherState state) => state switch
    {
        LauncherState.SignedOut => "Sign in with Steam (mock)",
        LauncherState.Authenticating => "Authentication busy (mock)",
        LauncherState.CheckingOwnership => "Ownership check busy (mock)",
        LauncherState.ReadyToInstall => "Install (mock)",
        LauncherState.Downloading => "Downloading 42% (mock)",
        LauncherState.ReadyToPlay => "Play disabled in Step 03",
        LauncherState.Error => "Retry (mock)",
        _ => "Mock action"
    };
}
