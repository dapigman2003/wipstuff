using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Net.Http;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.7 SteamKit CM WebSocket compatibility probe.
///
/// Step 05.6 proved the iPhone can reach Steam through HTTPS, DNS, raw TCP,
/// and raw ClientWebSocket. It also exposed the remaining WebSocket failure as
/// NSUrlSessionHandler's missing synchronous HTTP implementation. This step
/// supplies SteamKit's CM WebSocket purpose with SocketsHttpHandler while
/// retaining the native/default HttpClient for other Steam HTTP purposes.
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
        var clientConstructed = false;
        var connected = false;
        var disconnected = false;
        bool? disconnectedUserInitiated = null;
        var isConnectedEver = false;
        string? lastEndPoint = null;
        var stage = "SteamConfiguration.Create";
        SteamClient? steamClient = null;

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
                var stack = ex.StackTrace ?? string.Empty;
                var interesting = stack.Contains("SteamKit2", StringComparison.Ordinal) ||
                                  ex is PlatformNotSupportedException ||
                                  ex is NotSupportedException ||
                                  ex.GetType().Name == "CryptographicException" ||
                                  ex is System.Net.Http.HttpRequestException ||
                                  ex is System.Net.Sockets.SocketException ||
                                  ex is System.Net.WebSockets.WebSocketException;
                if (!interesting || exceptions.Count >= 16) return;

                var line = $"{ex.GetType().Name}: {ex.Message}";
                if (ex.InnerException is not null)
                    line += $" | Inner={ex.InnerException.GetType().Name}: {ex.InnerException.Message}";

                var firstRelevantLine = stack.Split('\n')
                    .Select(x => x.Trim())
                    .FirstOrDefault(x =>
                        x.Contains("SteamKit2", StringComparison.Ordinal) ||
                        x.Contains("System.Net.Http", StringComparison.Ordinal) ||
                        x.Contains("System.Net.WebSockets", StringComparison.Ordinal));
                if (!string.IsNullOrWhiteSpace(firstRelevantLine))
                    line += $" | {firstRelevantLine}";

                exceptions.Enqueue(line);
            }
            catch { }
        }

        AppDomain.CurrentDomain.FirstChanceException += FirstChance;
        try
        {
            var configuration = SteamConfiguration.Create(builder => builder
                .WithProtocolTypes(protocolTypes)
                .WithHttpClientFactory(Factory));

            stage = "SteamClient constructor";
            steamClient = new SteamClient(configuration);
            clientConstructed = true;

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
                manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(200));
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
                    manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(200));
                }
            }
            else
            {
                try { steamClient.Disconnect(); } catch { }
            }

            var exceptionText = FormatExceptions(exceptions);
            var factoryText = FormatFactoryCalls(factoryCalls);
            var passed = clientConstructed && connected && disconnected;
            var checks = (clientConstructed ? 1 : 0) + (connected ? 1 : 0) + (disconnected ? 1 : 0);
            var detail =
                $"HTTP factory calls: {factoryText}. " +
                $"CM WebSocket handler: {nameof(SocketsHttpHandler)}.\n" +
                $"IsConnected ever: {isConnectedEver}. CurrentEndPoint: {lastEndPoint ?? "never-set"}. " +
                $"Disconnected.UserInitiated={FormatNullable(disconnectedUserInitiated)}.\n" +
                $"First-chance SteamKit/runtime exceptions:\n{exceptionText}";

            return new SteamConnectionProbeResult(
                passed, transportName, protocolTypes.ToString(), clientConstructed, connected, disconnected,
                disconnectedUserInitiated, isConnectedEver, lastEndPoint, exceptionText, AssemblyVersion,
                sw.Elapsed, $"STEAMKIT WEBSOCKET {(passed ? "PASS" : "FAIL")} — {checks}/3", detail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            var exceptionText = FormatExceptions(exceptions);
            return new SteamConnectionProbeResult(
                false, transportName, protocolTypes.ToString(), clientConstructed, connected, disconnected,
                disconnectedUserInitiated, isConnectedEver, lastEndPoint, exceptionText, AssemblyVersion,
                sw.Elapsed, "STEAMKIT WEBSOCKET FAIL",
                $"Stage: {stage}\nHTTP factory calls: {FormatFactoryCalls(factoryCalls)}\n{ex}\n" +
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
