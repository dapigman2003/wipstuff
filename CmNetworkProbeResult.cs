namespace StS2Launcher.Core;

public sealed record CmNetworkProbeResult(
    bool DirectoryHttpsPassed,
    int? DirectoryStatusCode,
    int EndpointCount,
    string? TcpEndpoint,
    bool DnsPassed,
    string DnsDetail,
    bool TcpPassed,
    string TcpDetail,
    string? WebSocketEndpoint,
    bool WebSocketPassed,
    string WebSocketDetail,
    TimeSpan Elapsed,
    string Summary,
    string Detail);
