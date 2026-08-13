namespace StS2Launcher.Core;

public sealed record SteamConnectionProbeResult(
    bool Passed,
    string TransportName,
    string Protocols,
    bool ClientConstructed,
    bool ConnectedCallbackReceived,
    bool DisconnectedCallbackReceived,
    bool? DisconnectedUserInitiated,
    string SteamKitAssemblyVersion,
    TimeSpan Elapsed,
    string Summary,
    string Detail);
