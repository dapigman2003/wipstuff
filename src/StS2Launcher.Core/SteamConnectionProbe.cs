using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Net.Http;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.11 SteamKit post-WebSocket-upgrade / ClientHello AOT diagnostic.
///
/// Step 05.9 proved that SteamKit's exact selected CM endpoint can complete the
/// same custom-HttpMessageInvoker WebSocket upgrade outside SteamKit. This probe
/// now observes only the next SteamKit boundary: whether an outgoing ClientHello
/// is successfully serialized and exposed to IDebugNetworkListener before the
/// library disconnects. No authentication is performed.
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
        var clientConstructed = false;
        var connected = false;
        var disconnected = false;
        bool? disconnectedUserInitiated = null;
        var isConnectedEver = false;
        string? lastEndPoint = null;
        var stage = "protobuf-net AOT configuration";
        var protobufAotMode = "not-configured";
        SteamClient? steamClient = null;
        SteamNetworkTraceListener? networkTrace = null;

        HttpClient Factory(HttpClientPurpose purpose)
        {
            factoryCalls.Enqueue(purpose.ToString());
            return SteamHttpClientFactory.Create(purpose);
        }

        void FirstChance(object? _, FirstChanceExceptionEventArgs e)
        {
            try
            {
                var ex = e.Exception;
                var exceptionStack = ex.StackTrace ?? string.Empty;
                var isReflectionEmit = ex is PlatformNotSupportedException ||
                                       ex.Message.Contains("ReflectionEmit", StringComparison.OrdinalIgnoreCase) ||
                                       ex.Message.Contains("Reflection.Emit", StringComparison.OrdinalIgnoreCase);
                var interesting = exceptionStack.Contains("SteamKit2", StringComparison.Ordinal) ||
                                  isReflectionEmit ||
                                  ex is NotSupportedException ||
                                  ex.GetType().Name == "CryptographicException" ||
                                  ex is System.Net.Http.HttpRequestException ||
                                  ex is System.Net.Sockets.SocketException ||
                                  ex is System.Net.WebSockets.WebSocketException;
                if (!interesting || exceptions.Count >= 16) return;

                var line = $"{ex.GetType().Name}: {ex.Message}";
                if (ex.InnerException is not null)
                    line += $" | Inner={ex.InnerException.GetType().Name}: {ex.InnerException.Message}";

                var stackLines = exceptionStack.Split('\n')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                var firstRelevantLine = stackLines.FirstOrDefault(x =>
                    x.Contains("SteamKit2", StringComparison.Ordinal) ||
                    x.Contains("ProtoBuf", StringComparison.Ordinal) ||
                    x.Contains("System.Net.Http", StringComparison.Ordinal) ||
                    x.Contains("System.Net.WebSockets", StringComparison.Ordinal) ||
                    x.Contains("System.Reflection", StringComparison.Ordinal) ||
                    x.Contains("System.Linq.Expressions", StringComparison.Ordinal));
                if (!string.IsNullOrWhiteSpace(firstRelevantLine))
                    line += $" | {firstRelevantLine}";

                if (isReflectionEmit)
                {
                    // FirstChanceException can fire before Exception.StackTrace is populated on iOS.
                    // Capture the caller synchronously while we are still inside the throwing path.
                    var callerStack = Environment.StackTrace;
                    var connectedAtThrow = steamClient?.IsConnected;
                    var endPointAtThrow = steamClient?.CurrentEndPoint?.ToString() ?? "none";
                    var targetSite = ex.TargetSite?.ToString() ?? "unknown";
                    var source = ex.Source ?? "unknown";
                    line +=
                        $"\n    ReflectionEmit context: elapsed={sw.Elapsed.TotalMilliseconds:F0}ms; " +
                        $"thread={Environment.CurrentManagedThreadId}; IsConnected at throw={FormatNullable(connectedAtThrow)}; " +
                        $"CurrentEndPoint at throw={endPointAtThrow}; TargetSite={targetSite}; Source={source}";
                    line += $"\n    Caller stack:\n    {TrimStack(callerStack, 18)}";
                }
                else
                {
                    var stackExcerpt = string.Join("\n    ", stackLines.Take(8));
                    if (!string.IsNullOrWhiteSpace(stackExcerpt))
                        line += $"\n    {stackExcerpt}";
                }

                exceptions.Enqueue(line);
            }
            catch { }
        }

        AppDomain.CurrentDomain.FirstChanceException += FirstChance;
        try
        {
            protobufAotMode = ProtobufAotCompatibility.Configure();
            stage = "SteamConfiguration.Create";

            var configuration = SteamConfiguration.Create(builder => builder
                .WithProtocolTypes(protocolTypes)
                .WithHttpClientFactory(Factory));

            stage = "SteamClient constructor";
            steamClient = new SteamClient(configuration);
            clientConstructed = true;

            stage = "attach IDebugNetworkListener";
            networkTrace = new SteamNetworkTraceListener(sw);
            steamClient.DebugNetworkListener = networkTrace;

            stage = "CallbackManager constructor";
            var manager = new CallbackManager(steamClient);
            manager.Subscribe<SteamClient.ConnectedCallback>(_ => connected = true);
            manager.Subscribe<SteamClient.DisconnectedCallback>(cb =>
            {
                disconnectedUserInitiated = cb.UserInitiated;
                disconnected = true;
            });

            stage = "SteamClient.Connect";
            steamClient.Connect();
            stage = "callback/state pump";

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
            var passed = clientConstructed && connected && disconnected;
            var checks = (clientConstructed ? 1 : 0) + (connected ? 1 : 0) + (disconnected ? 1 : 0);
            var detail =
                $"Protobuf AOT mode: {protobufAotMode}.\n" +
                $"HTTP factory calls: {factoryText}. " +
                $"CM WebSocket handler: {nameof(SocketsHttpHandler)}.\n" +
                $"Outgoing ClientHello observed: {(clientHelloObserved ? "YES" : "NO")}.\n" +
                $"Debug network trace (metadata only):\n{traceText}\n" +
                $"IsConnected ever: {isConnectedEver}. CurrentEndPoint: {lastEndPoint ?? "never-set"}. " +
                $"Disconnected.UserInitiated={FormatNullable(disconnectedUserInitiated)}.\n" +
                $"First-chance SteamKit/runtime exceptions:\n{exceptionText}";

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
                $"Stage: {stage}\nProtobuf AOT mode: {protobufAotMode}\nHTTP factory calls: {FormatFactoryCalls(factoryCalls)}\n" +
                $"Outgoing ClientHello observed: {(clientHelloObserved ? "YES" : "NO")}\n" +
                $"Debug network trace (metadata only):\n{traceText}\n{ex}\n" +
                $"Captured first-chance exceptions:\n{exceptionText}");
        }
        finally
        {
            AppDomain.CurrentDomain.FirstChanceException -= FirstChance;
            if (steamClient is not null)
            {
                try { steamClient.Disconnect(); } catch { }
            }
        }
    }

    private static string TrimStack(string stack, int maxLines)
    {
        var lines = stack.Split('\n')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Take(maxLines)
            .ToArray();
        return lines.Length == 0 ? "(stack unavailable)" : string.Join("\n    ", lines);
    }

    private static string FormatExceptions(ConcurrentQueue<string> exceptions)
    {
        var items = exceptions.Distinct().Take(16).ToArray();
        return items.Length == 0 ? "(none captured)" : string.Join("\n", items.Select((x, i) => $"{i + 1}. {x}"));
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
