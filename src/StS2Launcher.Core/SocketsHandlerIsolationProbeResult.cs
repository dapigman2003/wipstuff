namespace StS2Launcher.Core;

public sealed record SocketsHandlerIsolationProbeResult(
    bool HttpsPassed,
    string HttpsDetail,
    bool WebSocketPassed,
    string WebSocketDetail,
    string ExceptionDetail,
    TimeSpan Elapsed,
    string Summary);
