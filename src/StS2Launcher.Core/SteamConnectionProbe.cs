using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.15 protobuf trim-preservation connection probe on SteamKit2 3.4.0.
///
/// Step 05.14 captured SteamKit's real post-connect exception: protobuf-net failed
/// while reflecting over CMsgProtoBufHeader because a property getter was missing
/// after iOS full trimming. Step 05.15 changes only trimmer preservation in the
/// iOS project; this probe intentionally keeps the same SteamKit DebugLog and
/// ClientHello instrumentation so the device result is directly comparable.
/// No authentication is performed.
/// </summary>
public sealed class SteamConnectionProbe
{
    public static string AssemblyVersion =>
        typeof(SteamClient).Assembly.GetName().Version?.ToString() ?? "unknown";

    public Task<SteamConnectionProbeResult> RunAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        return Task.Run(() => RunBlocking(timeout, cancellationToken), cancellationToken);
    }

    private static SteamConnectionProbeResult RunBlocking(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        const string transportName = "WebSocket/SocketsHttpHandler";
        const ProtocolTypes protocolTypes = ProtocolTypes.WebSocket;

        var sw = Stopwatch.StartNew();
        var exceptions = new ConcurrentQueue<string>();
        var factoryCalls = new ConcurrentQueue<string>();
        var steamDebugLines = new ConcurrentQueue<string>();
        var clientConstructed = false;
        var connected = false;
        var disconnected = false;
        bool? disconnectedUserInitiated = null;
        var isConnectedEver = false;
        string? lastEndPoint = null;
        SteamClient? steamClient = null;
        SteamNetworkTraceListener? networkTrace = null;
        var previousDebugLogEnabled = DebugLog.Enabled;

        HttpClient Factory(HttpClientPurpose purpose)
        {
            factoryCalls.Enqueue(purpose.ToString());
            return SteamHttpClientFactory.Create(purpose);
        }

        Action<string, string> debugListener = (category, message) =>
        {
            try
            {
                if (steamDebugLines.Count >= 80) return;

                var cleanMessage = message
                    .Replace("\r", string.Empty, StringComparison.Ordinal)
                    .Trim();
                steamDebugLines.Enqueue(
                    $"{sw.Elapsed.TotalMilliseconds:F0}ms [{category}] {cleanMessage}");
            }
            catch { }
        };

        void FirstChance(object? _, FirstChanceExceptionEventArgs e)
        {
            try
            {
                var ex = e.Exception;
                var stack = ex.StackTrace ?? string.Empty;
                var message = ex.Message ?? string.Empty;
                var isReflectionEmit =
                    message.Contains("ReflectionEmit", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("Reflection.Emit", StringComparison.OrdinalIgnoreCase);
                var interesting =
                    stack.Contains("SteamKit2", StringComparison.Ordinal) ||
                    stack.Contains("ProtoBuf", StringComparison.Ordinal) ||
                    isReflectionEmit ||
                    ex is NotSupportedException ||
                    ex.GetType().Name == "CryptographicException" ||
                    ex is System.Net.Http.HttpRequestException ||
                    ex is System.Net.Sockets.SocketException ||
                    ex is System.Net.WebSockets.WebSocketException;

                if (!interesting || exceptions.Count >= 20) return;

                var line = $"{ex.GetType().Name}: {ex.Message}";
                if (ex.InnerException is not null)
                    line += $" | Inner={ex.InnerException.GetType().Name}: {ex.InnerException.Message}";

                var stackLines = stack.Split('\n')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Take(10)
                    .ToArray();
                if (stackLines.Length > 0)
                    line += $"\n    {string.Join("\n    ", stackLines)}";

                exceptions.Enqueue(line);
            }
            catch { }
        }

        AppDomain.CurrentDomain.FirstChanceException += FirstChance;
        DebugLog.AddListener(debugListener);
        DebugLog.Enabled = true;

        try
        {
            var configuration = SteamConfiguration.Create(builder => builder
                .WithProtocolTypes(protocolTypes)
                .WithHttpClientFactory(Factory));

            steamClient = new SteamClient(configuration);
            clientConstructed = true;

            networkTrace = new SteamNetworkTraceListener(sw);
            steamClient.DebugNetworkListener = networkTrace;

            var manager = new CallbackManager(steamClient);
            manager.Subscribe<SteamClient.ConnectedCallback>(_ => connected = true);
            manager.Subscribe<SteamClient.DisconnectedCallback>(cb =>
            {
                disconnectedUserInitiated = cb.UserInitiated;
                disconnected = true;
            });

            steamClient.Connect();

            while (!connected && !disconnected && sw.Elapsed < timeout)
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

                while (!disconnected && sw.Elapsed < timeout)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(50));
                }
            }
            else
            {
                try { steamClient.Disconnect(); } catch { }
            }

            var exceptionText = FormatExceptions(exceptions);
            var factoryText = FormatFactoryCalls(factoryCalls);
            var traceText = networkTrace?.Snapshot() ?? "(listener not attached)";
            var clientHelloObserved = networkTrace?.OutgoingClientHelloObserved ?? false;
            var debugLogText = FormatDebugLog(steamDebugLines);
            var caughtPostConnectException = steamDebugLines.Any(line =>
                line.Contains("Unhandled exception after connecting", StringComparison.OrdinalIgnoreCase));
            var connectionSetupException = steamDebugLines.Any(line =>
                line.Contains("Unhandled exception when attempting to connect", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("Server record task threw exception", StringComparison.OrdinalIgnoreCase));
            var passed = clientConstructed && connected && disconnected;
            var checks = (clientConstructed ? 1 : 0) + (connected ? 1 : 0) + (disconnected ? 1 : 0);

            var detail =
                $"HTTP factory calls: {factoryText}. " +
                $"CM WebSocket handler: {nameof(SocketsHttpHandler)}.\n" +
                $"Outgoing ClientHello observed: {(clientHelloObserved ? "YES" : "NO")}.\n" +
                $"Debug network trace (metadata only):\n{traceText}\n" +
                $"IsConnected ever: {isConnectedEver}. CurrentEndPoint: {lastEndPoint ?? "never-set"}. " +
                $"Disconnected.UserInitiated={FormatNullable(disconnectedUserInitiated)}.\n" +
                $"SteamKit DebugLog previous Enabled={previousDebugLogEnabled}; capture Enabled=True.\n" +
                $"SteamKit post-connect exception logged: {(caughtPostConnectException ? "YES" : "NO")}. " +
                $"Connection-setup exception logged: {(connectionSetupException ? "YES" : "NO")}.\n" +
                $"SteamKit DebugLog:\n{debugLogText}\n" +
                $"First-chance supplemental exceptions:\n{exceptionText}";

            return new SteamConnectionProbeResult(
                passed, transportName, protocolTypes.ToString(), clientConstructed, connected, disconnected,
                disconnectedUserInitiated, isConnectedEver, lastEndPoint, clientHelloObserved, traceText,
                exceptionText, AssemblyVersion, sw.Elapsed,
                $"STEAMKIT WEBSOCKET {(passed ? "PASS" : "FAIL")} — {checks}/3", detail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            var exceptionText = FormatExceptions(exceptions);
            var traceText = networkTrace?.Snapshot() ?? "(listener not attached)";
            var clientHelloObserved = networkTrace?.OutgoingClientHelloObserved ?? false;
            return new SteamConnectionProbeResult(
                false, transportName, protocolTypes.ToString(), clientConstructed, connected, disconnected,
                disconnectedUserInitiated, isConnectedEver, lastEndPoint, clientHelloObserved, traceText,
                exceptionText, AssemblyVersion, sw.Elapsed, "STEAMKIT WEBSOCKET FAIL",
                $"Outer probe exception: {ex}\n" +
                $"SteamKit DebugLog:\n{FormatDebugLog(steamDebugLines)}\n" +
                $"Captured first-chance supplemental exceptions:\n{exceptionText}");
        }
        finally
        {
            AppDomain.CurrentDomain.FirstChanceException -= FirstChance;
            if (steamClient is not null)
            {
                try { steamClient.Disconnect(); } catch { }
            }
            DebugLog.RemoveListener(debugListener);
            DebugLog.Enabled = previousDebugLogEnabled;
        }
    }

    private static string FormatExceptions(ConcurrentQueue<string> exceptions)
    {
        var items = exceptions.Distinct().Take(20).ToArray();
        return items.Length == 0 ? "(none captured)" : string.Join("\n", items.Select((x, i) => $"{i + 1}. {x}"));
    }

    private static string FormatDebugLog(ConcurrentQueue<string> lines)
    {
        var items = lines.Take(80).ToArray();
        return items.Length == 0 ? "(no SteamKit DebugLog lines captured)" : string.Join("\n", items);
    }

    private static string FormatFactoryCalls(ConcurrentQueue<string> calls)
    {
        var items = calls.ToArray();
        if (items.Length == 0) return "(none)";
        return string.Join(", ", items
            .GroupBy(x => x)
            .Select(g => $"{g.Key}={g.Count()}"));
    }

    private static string FormatNullable(bool? value) => value.HasValue ? value.Value.ToString() : "not-received";
}
