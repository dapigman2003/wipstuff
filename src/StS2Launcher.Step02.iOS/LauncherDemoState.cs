namespace StS2Launcher.Step02.iOS;

public enum LauncherDemoState
{
    SignedOut = 0,
    Authenticating = 1,
    CheckingOwnership = 2,
    ReadyToInstall = 3,
    Downloading = 4,
    ReadyToPlay = 5,
    Error = 6
}

public static class LauncherDemoStatePresentation
{
    public static string Title(LauncherDemoState state) => state switch
    {
        LauncherDemoState.SignedOut => "Signed out",
        LauncherDemoState.Authenticating => "Signing in…",
        LauncherDemoState.CheckingOwnership => "Checking ownership…",
        LauncherDemoState.ReadyToInstall => "Ready to install",
        LauncherDemoState.Downloading => "Downloading…",
        LauncherDemoState.ReadyToPlay => "Ready to play",
        LauncherDemoState.Error => "Example error",
        _ => "Unknown"
    };

    public static string Detail(LauncherDemoState state) => state switch
    {
        LauncherDemoState.SignedOut =>
            "Steam is not connected. This is mock UI only.",
        LauncherDemoState.Authenticating =>
            "Pretending to authenticate with Steam.",
        LauncherDemoState.CheckingOwnership =>
            "Pretending to verify Slay the Spire 2 ownership.",
        LauncherDemoState.ReadyToInstall =>
            "Ownership verified. Game files are not installed.",
        LauncherDemoState.Downloading =>
            "Pretending to download game files. Progress should show 42%.",
        LauncherDemoState.ReadyToPlay =>
            "Mock installation is ready. Play is intentionally disabled in Step 02.",
        LauncherDemoState.Error =>
            "TEST ERROR: This is a deliberate visible error state.",
        _ => "Unknown state."
    };

    public static float Progress(LauncherDemoState state) =>
        state == LauncherDemoState.Downloading ? 0.42f : 0.0f;

    public static LauncherDemoState Next(LauncherDemoState state) =>
        state == LauncherDemoState.Error
            ? LauncherDemoState.SignedOut
            : (LauncherDemoState)((int)state + 1);
}
