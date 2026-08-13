namespace StS2Launcher.Core;

public sealed record LauncherSnapshot(
    LauncherState State,
    string Title,
    string Detail,
    string PrimaryActionTitle,
    float Progress,
    bool ShowProgress,
    int StateNumber,
    int StateCount);
