namespace StS2Launcher.Core;

public enum LauncherState
{
    SignedOut = 0,
    Authenticating = 1,
    CheckingOwnership = 2,
    ReadyToInstall = 3,
    Downloading = 4,
    ReadyToPlay = 5,
    Error = 6
}
