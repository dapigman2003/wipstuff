using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Final Step 05 unauthenticated Steam CM smoke test.
/// It proves SteamClient construction, ConnectedCallback, and a clean
/// DisconnectedCallback over the WebSocket transport. No account data is sent.
/// </summary>
public sealed class SteamConnectionProbe
{
    public static string AssemblyVersion =>
        typeof(SteamClient).Assembly.GetName().Version?.ToString() ?? "unknown";

    public Task<SteamConnectionProbeResult> RunAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        return Task.Run(
            () => RunBlocking(timeout, cancellationToken),
            cancellationToken);
    }

    private static SteamConnectionProbeResult RunBlocking(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        const ProtocolTypes protocols = ProtocolTypes.WebSocket;
        const string transportName = "WebSocket/SocketsHttpHandler";

        var sw = Stopwatch.StartNew();
        var factoryCalls = new ConcurrentQueue<HttpClientPurpose>();
        SteamClient? steamClient = null;
        var clientConstructed = false;
        var connected = false;
        var disconnected = false;
        bool? disconnectedUserInitiated = null;
        var isConnectedEver = false;
        string? lastEndPoint = null;
        string? error = null;

        HttpClient Factory(HttpClientPurpose purpose)
        {
            factoryCalls.Enqueue(purpose);
            return SteamHttpClientFactory.Create(purpose);
        }

        try
        {
            var configuration = SteamConfiguration.Create(builder => builder
                .WithProtocolTypes(protocols)
                .WithHttpClientFactory(Factory));

            steamClient = new SteamClient(configuration);
            clientConstructed = true;

            var manager = new CallbackManager(steamClient);
            manager.Subscribe<SteamClient.ConnectedCallback>(_ => connected = true);
            manager.Subscribe<SteamClient.DisconnectedCallback>(callback =>
            {
                disconnectedUserInitiated = callback.UserInitiated;
                disconnected = true;
            });

            steamClient.Connect();
            var connectDeadline = DateTime.UtcNow + timeout;

            while (!connected && !disconnected && DateTime.UtcNow < connectDeadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(50));
                isConnectedEver |= steamClient.IsConnected;
                lastEndPoint = steamClient.CurrentEndPoint?.ToString() ?? lastEndPoint;
            }

            if (connected)
            {
                isConnectedEver = true;
                lastEndPoint = steamClient.CurrentEndPoint?.ToString() ?? lastEndPoint;
                steamClient.Disconnect();

                var disconnectDeadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
                while (!disconnected && DateTime.UtcNow < disconnectDeadline)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(50));
                }
            }
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            if (steamClient is not null)
            {
                try
                {
                    isConnectedEver |= steamClient.IsConnected;
                    lastEndPoint = steamClient.CurrentEndPoint?.ToString() ?? lastEndPoint;
                    if (steamClient.IsConnected)
                        steamClient.Disconnect();
                }
                catch
                {
                    // Best-effort cleanup for the unauthenticated smoke-test connection.
                }
            }

            sw.Stop();
        }

        return new SteamConnectionProbeResult(
            TransportName: transportName,
            Protocols: protocols.ToString(),
            ClientConstructed: clientConstructed,
            ConnectedCallbackReceived: connected,
            DisconnectedCallbackReceived: disconnected,
            DisconnectedUserInitiated: disconnectedUserInitiated,
            IsConnectedEver: isConnectedEver,
            LastCurrentEndPoint: lastEndPoint,
            CmWebSocketFactoryUsed: factoryCalls.Contains(HttpClientPurpose.CMWebSocket),
            SteamKitAssemblyVersion: AssemblyVersion,
            Elapsed: sw.Elapsed,
            Error: error);
    }
}
