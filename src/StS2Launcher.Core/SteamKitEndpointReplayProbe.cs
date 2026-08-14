using System.Diagnostics;
using System.Net.Http;
using System.Net.WebSockets;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.14 replays the exact CM endpoint selected by SteamKit through the
/// already-proven ClientWebSocket + CMWebSocket HttpMessageInvoker path.
///
/// Step 05.8 proved that SocketsHttpHandler HTTPS and the custom-invoker
/// ClientWebSocket handshake work on the physical iPhone, but that control used
/// the native network probe's CM while SteamKit selected a different CM.
/// This probe removes that endpoint mismatch without sending a Steam protocol
/// payload and without performing authentication.
/// </summary>
public sealed class SteamKitEndpointReplayProbe
{
    public async Task<SteamKitEndpointReplayProbeResult> RunAsync(
        string? steamKitEndPoint,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        var sw = Stopwatch.StartNew();

        if (!TryBuildWebSocketUri(steamKitEndPoint, out var uri, out var parseDetail))
        {
            return new SteamKitEndpointReplayProbeResult(
                false,
                steamKitEndPoint,
                null,
                parseDetail,
                "(not run)",
                sw.Elapsed,
                "EXACT STEAMKIT ENDPOINT REPLAY: NOT RUN");
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            using var client = SteamHttpClientFactory.Create(HttpClientPurpose.CMWebSocket);
            using var socket = new ClientWebSocket();

            // Intentionally set no ClientWebSocket options here. SteamKit's
            // WebSocketContext also constructs a plain ClientWebSocket before
            // calling this same custom-HttpMessageInvoker ConnectAsync overload.
            await socket.ConnectAsync(uri, client, cts.Token).ConfigureAwait(false);

            var passed = socket.State == WebSocketState.Open;
            var detail = passed
                ? $"HTTP upgrade succeeded on SteamKit-selected CM: {uri}"
                : $"ConnectAsync returned state {socket.State}: {uri}";

            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    using var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Step 05.14 exact endpoint replay complete",
                        closeCts.Token).ConfigureAwait(false);
                }
                catch
                {
                    socket.Abort();
                }
            }

            return new SteamKitEndpointReplayProbeResult(
                passed,
                steamKitEndPoint,
                uri.ToString(),
                detail,
                "(none captured)",
                sw.Elapsed,
                $"EXACT STEAMKIT ENDPOINT REPLAY {(passed ? "PASS" : "FAIL")}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return new SteamKitEndpointReplayProbeResult(
                false,
                steamKitEndPoint,
                uri.ToString(),
                FormatException(ex),
                FormatExceptionWithStack(ex),
                sw.Elapsed,
                "EXACT STEAMKIT ENDPOINT REPLAY FAIL");
        }
    }

    private static bool TryBuildWebSocketUri(
        string? endpoint,
        out Uri uri,
        out string detail)
    {
        uri = default!;
        detail = "SteamKit did not expose a CM endpoint to replay.";

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.Equals(endpoint, "never-set", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var clean = endpoint.Trim();

        // DnsEndPoint.ToString() is commonly rendered as
        // "Unspecified/hostname:port". Strip only that address-family prefix.
        var slash = clean.IndexOf('/');
        if (slash >= 0 && slash < clean.Length - 1)
            clean = clean[(slash + 1)..];

        if (!Uri.TryCreate("tcp://" + clean, UriKind.Absolute, out var parsed) ||
            string.IsNullOrWhiteSpace(parsed.Host) ||
            parsed.Port <= 0)
        {
            detail = $"Could not parse SteamKit CurrentEndPoint: {endpoint}";
            return false;
        }

        uri = new UriBuilder("wss", parsed.Host, parsed.Port, "/cmsocket/").Uri;
        detail = uri.ToString();
        return true;
    }

    private static string FormatException(Exception ex)
    {
        var text = $"{ex.GetType().Name}: {ex.Message}";
        if (ex.InnerException is not null)
            text += $" | Inner={ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
        return text;
    }

    private static string FormatExceptionWithStack(Exception ex)
    {
        var lines = ex.ToString().Split('\n')
            .Select(x => x.TrimEnd())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Take(18);
        return string.Join("\n", lines);
    }
}
