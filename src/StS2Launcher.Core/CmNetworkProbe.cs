using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.6 diagnostics below SteamKit's CM connection layer.
///
/// The previous step proved SteamClient construction is healthy on iOS, but
/// SteamKit never reaches ConnectedCallback. This probe therefore verifies the
/// same underlying network primitives without authenticating or sending Steam
/// protocol messages:
///   1) HTTPS access to Valve's CM-directory endpoint;
///   2) DNS for a returned CM host;
///   3) raw TCP connect to a returned socket CM;
///   4) raw ClientWebSocket handshake to a returned websocket CM.
///
/// A websocket connection is closed immediately after the HTTP upgrade. No
/// Steam logon/authentication payload is ever sent.
/// </summary>
public sealed class CmNetworkProbe
{
    private static readonly Uri DirectoryUri = new(
        "https://api.steampowered.com/ISteamDirectory/GetCMListForConnect/v0001/?format=json&cellid=0");

    public Task<CmNetworkProbeResult> RunAsync(
        TimeSpan directoryTimeout,
        TimeSpan connectTimeout,
        CancellationToken cancellationToken = default)
    {
        if (directoryTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(directoryTimeout));
        if (connectTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(connectTimeout));

        return RunCoreAsync(directoryTimeout, connectTimeout, cancellationToken);
    }

    private static async Task<CmNetworkProbeResult> RunCoreAsync(
        TimeSpan directoryTimeout,
        TimeSpan connectTimeout,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpointRecords = new List<CmEndpointRecord>();
        int? directoryStatus = null;
        var directoryPassed = false;
        var directoryDetail = "not-run";

        try
        {
            using var directoryCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            directoryCts.CancelAfter(directoryTimeout);

            using var http = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("StS2Launcher-iOS-Step05.6/0.0.12");

            using var response = await http.GetAsync(
                DirectoryUri,
                HttpCompletionOption.ResponseHeadersRead,
                directoryCts.Token).ConfigureAwait(false);

            directoryStatus = (int)response.StatusCode;
            var json = await response.Content.ReadAsStringAsync(directoryCts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(json);
            CollectEndpointRecords(doc.RootElement, endpointRecords, inheritedType: null);

            directoryPassed = endpointRecords.Count > 0;
            directoryDetail = directoryPassed
                ? $"HTTP {directoryStatus}; discovered {endpointRecords.Count} CM endpoint record(s)."
                : $"HTTP {directoryStatus}; JSON parsed but no endpoint records were found.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            directoryDetail = FormatException(ex);
        }

        if (!directoryPassed)
        {
            return BuildResult(
                directoryPassed,
                directoryStatus,
                endpointRecords,
                null,
                false,
                "skipped because CM directory discovery failed",
                false,
                "skipped because CM directory discovery failed",
                null,
                false,
                "skipped because CM directory discovery failed",
                stopwatch.Elapsed,
                directoryDetail);
        }

        var tcpRecord = SelectTcpEndpoint(endpointRecords);
        var webSocketRecord = SelectWebSocketEndpoint(endpointRecords);

        var dnsPassed = false;
        var dnsDetail = "no usable endpoint selected";
        var dnsHost = tcpRecord?.Host ?? webSocketRecord?.Host;

        if (!string.IsNullOrWhiteSpace(dnsHost))
        {
            try
            {
                using var dnsCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                dnsCts.CancelAfter(connectTimeout);
                var addresses = await Dns.GetHostAddressesAsync(dnsHost, dnsCts.Token).ConfigureAwait(false);
                dnsPassed = addresses.Length > 0;
                dnsDetail = addresses.Length == 0
                    ? $"{dnsHost}: zero addresses returned"
                    : $"{dnsHost}: {string.Join(", ", addresses.Take(4).Select(a => a.ToString()))}";
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                dnsDetail = FormatException(ex);
            }
        }

        var tcpPassed = false;
        var tcpDetail = tcpRecord is null ? "no socket/TCP endpoint found" : "not-run";

        if (tcpRecord is not null)
        {
            try
            {
                using var tcpCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                tcpCts.CancelAfter(connectTimeout);
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(tcpRecord.Host, tcpRecord.Port, tcpCts.Token).ConfigureAwait(false);
                tcpPassed = tcpClient.Connected;
                tcpDetail = tcpPassed
                    ? $"connected to {tcpRecord.Host}:{tcpRecord.Port}"
                    : $"ConnectAsync returned without a connected socket for {tcpRecord.Host}:{tcpRecord.Port}";
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                tcpDetail = FormatException(ex);
            }
        }

        var webSocketPassed = false;
        var webSocketDetail = webSocketRecord is null ? "no websocket endpoint found" : "not-run";

        if (webSocketRecord is not null)
        {
            try
            {
                using var wsCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                wsCts.CancelAfter(connectTimeout);
                using var socket = new ClientWebSocket();
                socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);

                var uri = new UriBuilder(
                    "wss",
                    webSocketRecord.Host,
                    webSocketRecord.Port,
                    "/cmsocket/").Uri;

                await socket.ConnectAsync(uri, wsCts.Token).ConfigureAwait(false);
                webSocketPassed = socket.State == WebSocketState.Open;
                webSocketDetail = webSocketPassed
                    ? $"HTTP upgrade succeeded: {uri}"
                    : $"ConnectAsync returned state {socket.State}: {uri}";

                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        using var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Step 05.6 network probe complete",
                            closeCts.Token).ConfigureAwait(false);
                    }
                    catch
                    {
                        socket.Abort();
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                webSocketDetail = FormatException(ex);
            }
        }

        return BuildResult(
            directoryPassed,
            directoryStatus,
            endpointRecords,
            tcpRecord,
            dnsPassed,
            dnsDetail,
            tcpPassed,
            tcpDetail,
            webSocketRecord,
            webSocketPassed,
            webSocketDetail,
            stopwatch.Elapsed,
            directoryDetail);
    }

    private static CmNetworkProbeResult BuildResult(
        bool directoryPassed,
        int? directoryStatus,
        IReadOnlyCollection<CmEndpointRecord> records,
        CmEndpointRecord? tcpRecord,
        bool dnsPassed,
        string dnsDetail,
        bool tcpPassed,
        string tcpDetail,
        CmEndpointRecord? webSocketRecord,
        bool webSocketPassed,
        string webSocketDetail,
        TimeSpan elapsed,
        string directoryDetail)
    {
        var passedChecks =
            (directoryPassed ? 1 : 0) +
            (dnsPassed ? 1 : 0) +
            (tcpPassed ? 1 : 0) +
            (webSocketPassed ? 1 : 0);

        var summary = $"CM NETWORK {(passedChecks == 4 ? "PASS" : "RESULT")} — {passedChecks}/4";
        var detail =
            $"Directory: {directoryDetail}\n" +
            $"DNS: {dnsDetail}\n" +
            $"TCP: {tcpDetail}\n" +
            $"WebSocket: {webSocketDetail}";

        return new CmNetworkProbeResult(
            DirectoryHttpsPassed: directoryPassed,
            DirectoryStatusCode: directoryStatus,
            EndpointCount: records.Count,
            TcpEndpoint: tcpRecord?.Display,
            DnsPassed: dnsPassed,
            DnsDetail: dnsDetail,
            TcpPassed: tcpPassed,
            TcpDetail: tcpDetail,
            WebSocketEndpoint: webSocketRecord?.Display,
            WebSocketPassed: webSocketPassed,
            WebSocketDetail: webSocketDetail,
            Elapsed: elapsed,
            Summary: summary,
            Detail: detail);
    }

    private static void CollectEndpointRecords(
        JsonElement element,
        List<CmEndpointRecord> records,
        string? inheritedType)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            string? endpoint = null;
            string? type = inheritedType;

            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals("endpoint", StringComparison.OrdinalIgnoreCase) &&
                    property.Value.ValueKind == JsonValueKind.String)
                {
                    endpoint = property.Value.GetString();
                }
                else if (property.Name.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    type = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString(),
                        JsonValueKind.Number => property.Value.GetRawText(),
                        _ => type
                    };
                }
            }

            if (TryParseEndpoint(endpoint, type, out var record) &&
                !records.Any(r => r.Display.Equals(record.Display, StringComparison.OrdinalIgnoreCase) &&
                                  string.Equals(r.Type, record.Type, StringComparison.OrdinalIgnoreCase)))
            {
                records.Add(record);
            }

            foreach (var property in element.EnumerateObject())
            {
                var childType = InferTypeFromPropertyName(property.Name) ?? type;
                CollectEndpointRecords(property.Value, records, childType);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in element.EnumerateArray())
                CollectEndpointRecords(child, records, inheritedType);
        }
    }

    private static string? InferTypeFromPropertyName(string propertyName)
    {
        if (propertyName.Contains("websocket", StringComparison.OrdinalIgnoreCase))
            return "websockets";

        if (propertyName.Equals("serverlist", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("socket", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("tcp", StringComparison.OrdinalIgnoreCase))
        {
            return "tcp";
        }

        return null;
    }

    private static bool TryParseEndpoint(string? endpoint, string? type, out CmEndpointRecord record)
    {
        record = default!;

        if (string.IsNullOrWhiteSpace(endpoint))
            return false;

        if (!Uri.TryCreate("tcp://" + endpoint.Trim(), UriKind.Absolute, out var uri) ||
            string.IsNullOrWhiteSpace(uri.Host) ||
            uri.Port <= 0)
        {
            return false;
        }

        record = new CmEndpointRecord(uri.Host, uri.Port, type?.Trim());
        return true;
    }

    private static CmEndpointRecord? SelectTcpEndpoint(IEnumerable<CmEndpointRecord> records)
    {
        var list = records.ToList();
        return list.FirstOrDefault(r => ContainsAny(r.Type, "tcp", "socket"))
            ?? list.FirstOrDefault(r => !ContainsAny(r.Type, "websocket", "websockets", "ws"))
            ?? list.FirstOrDefault();
    }

    private static CmEndpointRecord? SelectWebSocketEndpoint(IEnumerable<CmEndpointRecord> records)
    {
        var list = records.ToList();
        return list.FirstOrDefault(r => ContainsAny(r.Type, "websocket", "websockets", "ws"))
            ?? list.FirstOrDefault();
    }

    private static bool ContainsAny(string? value, params string[] needles)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return needles.Any(n => value.Contains(n, StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatException(Exception ex)
    {
        var text = $"{ex.GetType().Name}: {ex.Message}";
        if (ex.InnerException is not null)
            text += $" | Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
        return text;
    }

    private sealed record CmEndpointRecord(string Host, int Port, string? Type)
    {
        public string Display => $"{Host}:{Port}" + (string.IsNullOrWhiteSpace(Type) ? string.Empty : $" ({Type})");
    }
}
