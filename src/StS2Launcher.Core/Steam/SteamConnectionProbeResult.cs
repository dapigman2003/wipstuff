namespace StS2Launcher.Core;

public sealed record SteamConnectionProbeResult(
    string TransportName,
    string Protocols,
    bool ClientConstructed,
    bool ConnectedCallbackReceived,
    bool DisconnectedCallbackReceived,
    bool? DisconnectedUserInitiated,
    bool IsConnectedEver,
    string? LastCurrentEndPoint,
    bool CmWebSocketFactoryUsed,
    string SteamKitAssemblyVersion,
    TimeSpan Elapsed,
    string? Error)
{
    public const int TotalChecks = 3;

    public int PassedChecks =>
        (ClientConstructed ? 1 : 0) +
        (ConnectedCallbackReceived ? 1 : 0) +
        (DisconnectedCallbackReceived ? 1 : 0);

    public bool Passed => PassedChecks == TotalChecks;

    public string Summary => Passed
        ? $"STEAM CONNECTION PASS — {PassedChecks}/{TotalChecks}"
        : $"STEAM CONNECTION FAIL — {PassedChecks}/{TotalChecks}";
}
