using System.Diagnostics;
using System.Net.Http;
using System.Net.WebSockets;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.13 retains the Step 05.8 below-SteamKit handler regression probe.
///
/// The physical iPhone already passed both checks in Step 05.8. Re-run them here
/// to ensure the exact SocketsHttpHandler + custom-HttpMessageInvoker framework
/// path remains healthy before replaying SteamKit's selected CM endpoint.
///
/// No Steam protocol payload and no authentication data are sent.
/// </summary>
public sealed class SocketsHandlerIsolationProbe
{
    private static readonly Uri DirectoryUri = new(
        "https://api.steampowered.com/ISteamDirectory/GetCMListForConnect/v0001/?format=json&cellid=0");

    public async Task<SocketsHandlerIsolationProbeResult> RunAsync(
        string? webSocketEndpoint,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        var sw = Stopwatch.StartNew();
        var httpsPassed = false;
        var httpsDetail = "not-run";
        var webSocketPassed = false;
        var webSocketDetail = "not-run";
        var exceptions = new List<string>();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            using var client = SteamHttpClientFactory.Create(HttpClientPurpose.CMWebSocket);
            client.Timeout = Timeout.InfiniteTimeSpan;

            using var response = await client.GetAsync(
                DirectoryUri,
                HttpCompletionOption.ResponseHeadersRead,
                cts.Token).ConfigureAwait(false);

            httpsPassed = response.IsSuccessStatusCode;
            httpsDetail = $"{client.GetType().Name} / SocketsHttpHandler — HTTP {(int)response.StatusCode}";
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            httpsDetail = FormatException(ex);
            exceptions.Add("HTTPS:\n" + FormatExceptionWithStack(ex));
        }

        if (!TryBuildWebSocketUri(webSocketEndpoint, out var uri, out var parseDetail))
        {
            webSocketDetail = parseDetail;
        }
        else
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout);
                using var client = SteamHttpClientFactory.Create(HttpClientPurpose.CMWebSocket);
                using var socket = new ClientWebSocket();
                socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);

                // This overload is the important Step 05.13 boundary: it forces
                // ClientWebSocket to use the supplied CMWebSocket HttpMessageInvoker.
                await socket.ConnectAsync(uri, client, cts.Token).ConfigureAwait(false);

                webSocketPassed = socket.State == WebSocketState.Open;
                webSocketDetail = webSocketPassed
                    ? $"custom-invoker HTTP upgrade succeeded: {uri}"
                    : $"custom-invoker ConnectAsync returned state {socket.State}: {uri}";

                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        using var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Step 05.13 handler isolation complete",
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
                exceptions.Add("WEBSOCKET:\n" + FormatExceptionWithStack(ex));
            }
        }

        var checks = (httpsPassed ? 1 : 0) + (webSocketPassed ? 1 : 0);
        var summary = $"SOCKETS HANDLER ISOLATION {(checks == 2 ? "PASS" : "RESULT")} — {checks}/2";
        var exceptionDetail = exceptions.Count == 0
            ? "(none captured)"
            : string.Join("\n\n", exceptions);

        return new SocketsHandlerIsolationProbeResult(
            httpsPassed,
            httpsDetail,
            webSocketPassed,
            webSocketDetail,
            exceptionDetail,
            sw.Elapsed,
            summary);
    }

    private static bool TryBuildWebSocketUri(
        string? endpoint,
        out Uri uri,
        out string detail)
    {
        uri = default!;
        detail = "no websocket endpoint from the native CM probe";

        if (string.IsNullOrWhiteSpace(endpoint))
            return false;

        var clean = endpoint.Trim();
        var annotation = clean.IndexOf(" (", StringComparison.Ordinal);
        if (annotation >= 0)
            clean = clean[..annotation];

        if (!Uri.TryCreate("tcp://" + clean, UriKind.Absolute, out var parsed) ||
            string.IsNullOrWhiteSpace(parsed.Host) ||
            parsed.Port <= 0)
        {
            detail = $"could not parse native websocket endpoint: {endpoint}";
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
        var text = ex.ToString();
        var lines = text.Split('\n')
            .Select(x => x.TrimEnd())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Take(18);
        return string.Join("\n", lines);
    }
}
