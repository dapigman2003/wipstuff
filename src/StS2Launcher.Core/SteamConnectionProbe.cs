using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.13 Reflection.Emit stage-localization probe on SteamKit2 3.4.0.
///
/// Step 05.12 proved that upgrading SteamKit from 3.3.1 to 3.4.0 did not remove
/// the iOS failure: the exact selected CM replay still worked, no ClientHello
/// reached IDebugNetworkListener, and PlatformNotSupported_ReflectionEmit still
/// appeared. Step 05.13 therefore changes no connection behavior. It timestamps
/// every synchronous SteamKit setup/connect stage and records the active stage
/// at the instant a first-chance Reflection.Emit exception is observed.
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
        var reflectionEmitStages = new ConcurrentQueue<string>();
        var stageTimeline = new ConcurrentQueue<string>();
        var factoryCalls = new ConcurrentQueue<string>();
        var clientConstructed = false;
        var connected = false;
        var disconnected = false;
        bool? disconnectedUserInitiated = null;
        var isConnectedEver = false;
        string? lastEndPoint = null;
        var stage = "probe entered";
        var protobufAotMode = "not-configured";
        SteamClient? steamClient = null;
        SteamNetworkTraceListener? networkTrace = null;

        void SetStage(string next)
        {
            stage = next;
            stageTimeline.Enqueue($"{sw.Elapsed.TotalMilliseconds:F0}ms — {next}");
        }

        SetStage(stage);

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
                var isReflectionEmit = ex is PlatformNotSupportedException &&
                                           (ex.Message.Contains("ReflectionEmit", StringComparison.OrdinalIgnoreCase) ||
                                            ex.Message.Contains("Reflection.Emit", StringComparison.OrdinalIgnoreCase)) ||
                                       ex.Message.Contains("ReflectionEmit", StringComparison.OrdinalIgnoreCase) ||
                                       ex.Message.Contains("Reflection.Emit", StringComparison.OrdinalIgnoreCase);
                var interesting = exceptionStack.Contains("SteamKit2", StringComparison.Ordinal) ||
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
                    // Exception.StackTrace is frequently still empty at FirstChanceException time on
                    // iOS. The explicit stage marker is therefore the primary Step 05.13 diagnostic.
                    var stageAtThrow = stage;
                    reflectionEmitStages.Enqueue(
                        $"{sw.Elapsed.TotalMilliseconds:F0}ms — {stageAtThrow} — thread {Environment.CurrentManagedThreadId}");

                    var callerStack = Environment.StackTrace;
                    var connectedAtThrow = steamClient?.IsConnected;
                    var endPointAtThrow = steamClient?.CurrentEndPoint?.ToString() ?? "none";
                    var targetSite = ex.TargetSite?.ToString() ?? "unknown";
                    var source = ex.Source ?? "unknown";
                    line +=
                        $"\n    ReflectionEmit context: elapsed={sw.Elapsed.TotalMilliseconds:F0}ms; " +
                        $"stage={stageAtThrow}; thread={Environment.CurrentManagedThreadId}; " +
                        $"IsConnected at throw={FormatNullable(connectedAtThrow)}; " +
                        $"CurrentEndPoint at throw={endPointAtThrow}; TargetSite={targetSite}; Source={source}; " +
                        $"DynamicCodeSupported={RuntimeFeature.IsDynamicCodeSupported}; " +
                        $"DynamicCodeCompiled={RuntimeFeature.IsDynamicCodeCompiled}";
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
            SetStage("protobuf-net AOT configuration");
            protobufAotMode = ProtobufAotCompatibility.Configure();

            SetStage("SteamConfiguration.Create");
            var configuration = SteamConfiguration.Create(builder => builder
                .WithProtocolTypes(protocolTypes)
                .WithHttpClientFactory(Factory));

            SetStage("SteamClient constructor");
            steamClient = new SteamClient(configuration);
            clientConstructed = true;

            SetStage("attach IDebugNetworkListener");
            networkTrace = new SteamNetworkTraceListener(sw);
            steamClient.DebugNetworkListener = networkTrace;

            SetStage("CallbackManager constructor");
            var manager = new CallbackManager(steamClient);

            SetStage("subscribe ConnectedCallback");
            manager.Subscribe<SteamClient.ConnectedCallback>(_ => connected = true);

            SetStage("subscribe DisconnectedCallback");
            manager.Subscribe<SteamClient.DisconnectedCallback>(cb =>
            {
                disconnectedUserInitiated = cb.UserInitiated;
                disconnected = true;
            });

            SetStage("SteamClient.Connect call");
            steamClient.Connect();

            SetStage("post-Connect callback/state pump");
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
                SetStage("SteamClient.Disconnect after ConnectedCallback");
                steamClient.Disconnect();

                SetStage("post-disconnect callback pump");
                while (!disconnected && sw.Elapsed < timeout)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(50));
                }
            }
            else
            {
                SetStage("SteamClient.Disconnect after failed connection");
                try { steamClient.Disconnect(); } catch { }
            }

            SetStage("probe result formatting");

            var exceptionText = FormatExceptions(exceptions);
            var factoryText = FormatFactoryCalls(factoryCalls);
            var traceText = networkTrace?.Snapshot() ?? "(listener not attached)";
            var clientHelloObserved = networkTrace?.OutgoingClientHelloObserved ?? false;
            var passed = clientConstructed && connected && disconnected;
            var checks = (clientConstructed ? 1 : 0) + (connected ? 1 : 0) + (disconnected ? 1 : 0);
            var stageText = FormatStageTimeline(stageTimeline);
            var emitStageText = FormatReflectionStages(reflectionEmitStages);
            var detail =
                $"RuntimeFeature dynamic code: supported={RuntimeFeature.IsDynamicCodeSupported}; " +
                $"compiled={RuntimeFeature.IsDynamicCodeCompiled}.\n" +
                $"ReflectionEmit observed stage(s):\n{emitStageText}\n" +
                $"Stage timeline:\n{stageText}\n" +
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
            SetStage($"outer catch: {ex.GetType().Name}");
            var exceptionText = FormatExceptions(exceptions);
            var traceText = networkTrace?.Snapshot() ?? "(listener not attached)";
            var clientHelloObserved = networkTrace?.OutgoingClientHelloObserved ?? false;
            return new SteamConnectionProbeResult(
                false, transportName, protocolTypes.ToString(), clientConstructed, connected, disconnected,
                disconnectedUserInitiated, isConnectedEver, lastEndPoint, clientHelloObserved, traceText,
                exceptionText, AssemblyVersion, sw.Elapsed, "STEAMKIT WEBSOCKET FAIL",
                $"Stage: {stage}\n" +
                $"RuntimeFeature dynamic code: supported={RuntimeFeature.IsDynamicCodeSupported}; compiled={RuntimeFeature.IsDynamicCodeCompiled}\n" +
                $"ReflectionEmit observed stage(s):\n{FormatReflectionStages(reflectionEmitStages)}\n" +
                $"Stage timeline:\n{FormatStageTimeline(stageTimeline)}\n" +
                $"Protobuf AOT mode: {protobufAotMode}\nHTTP factory calls: {FormatFactoryCalls(factoryCalls)}\n" +
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
        var items = exceptions.Distinct().Take(20).ToArray();
        return items.Length == 0 ? "(none captured)" : string.Join("\n", items.Select((x, i) => $"{i + 1}. {x}"));
    }

    private static string FormatStageTimeline(ConcurrentQueue<string> stages)
    {
        var items = stages.ToArray();
        return items.Length == 0 ? "(none captured)" : string.Join("\n", items.Select(x => $"- {x}"));
    }

    private static string FormatReflectionStages(ConcurrentQueue<string> stages)
    {
        var items = stages.Distinct().ToArray();
        return items.Length == 0 ? "(none captured)" : string.Join("\n", items.Select(x => $"- {x}"));
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
