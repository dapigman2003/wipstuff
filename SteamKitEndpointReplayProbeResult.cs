namespace StS2Launcher.Core;

public sealed record SteamKitEndpointReplayProbeResult(
    bool Passed,
    string? SourceEndPoint,
    string? WebSocketUri,
    string Detail,
    string ExceptionDetail,
    TimeSpan Elapsed,
    string Summary);
