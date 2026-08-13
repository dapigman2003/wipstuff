namespace StS2Launcher.Core;

public sealed record SteamConnectionProbeResult(
    bool Passed,
    bool ClientConstructed,
    bool ConnectedCallbackReceived,
    bool DisconnectedCallbackReceived,
    string SteamKitAssemblyVersion,
    TimeSpan Elapsed,
    string Summary,
    string Detail);
