using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.6 SteamKit internal-boundary probe.
///
/// Step 05.5 proved Valve directory HTTPS, DNS, raw TCP and raw ClientWebSocket
/// all work on the same iPhone. Both SteamKit transports still fail before
/// ConnectedCallback, so this step captures the state and hidden exceptions
/// inside SteamKit's asynchronous connection machinery without authenticating.
/// </summary>
public sealed class SteamConnectionProbe
{
    public static string AssemblyVersion =>
        typeof(SteamClient).Assembly.GetName().Version?.ToString() ?? "unknown";

    public Task<SteamConnectionProbeResult> RunAsync(
        string transportName,
        ProtocolTypes protocolTypes,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transportName);
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        if (protocolTypes != ProtocolTypes.WebSocket && protocolTypes != ProtocolTypes.Tcp)
            throw new ArgumentOutOfRangeException(nameof(protocolTypes));

        return Task.Run(() => RunBlocking(transportName, protocolTypes, timeout, cancellationToken), cancellationToken);
    }

    private static SteamConnectionProbeResult RunBlocking(
        string transportName,
        ProtocolTypes protocolTypes,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var exceptions = new ConcurrentQueue<string>();
        var clientConstructed = false;
        var connected = false;
        var disconnected = false;
        bool? disconnectedUserInitiated = null;
        var isConnectedEver = false;
        string? lastEndPoint = null;
        var stage = $"SteamConfiguration.Create ({transportName})";
        SteamClient? steamClient = null;

        void FirstChance(object? _, FirstChanceExceptionEventArgs e)
        {
            try
            {
                var ex = e.Exception;
                var stack = ex.StackTrace ?? string.Empty;
                var interesting = stack.Contains("SteamKit2", StringComparison.Ordinal) ||
                                  ex is PlatformNotSupportedException ||
                                  ex.GetType().Name == "CryptographicException" ||
                                  ex is System.Net.Sockets.SocketException ||
                                  ex is System.Net.WebSockets.WebSocketException;
                if (!interesting || exceptions.Count >= 12) return;
                var line = $"{ex.GetType().Name}: {ex.Message}";
                if (ex.InnerException is not null)
                    line += $" | Inner={ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
                var firstSteamLine = stack.Split('\n')
                    .Select(x => x.Trim())
                    .FirstOrDefault(x => x.Contains("SteamKit2", StringComparison.Ordinal));
                if (!string.IsNullOrWhiteSpace(firstSteamLine)) line += $" | {firstSteamLine}";
                exceptions.Enqueue(line);
            }
            catch { }
        }

        AppDomain.CurrentDomain.FirstChanceException += FirstChance;
        try
        {
            var configuration = SteamConfiguration.Create(builder => builder.WithProtocolTypes(protocolTypes));
            stage = $"SteamClient constructor ({transportName})";
            steamClient = new SteamClient(configuration);
            clientConstructed = true;

            stage = $"CallbackManager constructor ({transportName})";
            var manager = new CallbackManager(steamClient);
            manager.Subscribe<SteamClient.ConnectedCallback>(_ => connected = true);
            manager.Subscribe<SteamClient.DisconnectedCallback>(cb =>
            {
                disconnectedUserInitiated = cb.UserInitiated;
                disconnected = true;
            });

            stage = $"SteamClient.Connect ({transportName})";
            steamClient.Connect();
            stage = $"callback/state pump ({transportName})";

            while (!connected && !disconnected && sw.Elapsed < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();
                manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(200));
                isConnectedEver |= steamClient.IsConnected;
                lastEndPoint = steamClient.CurrentEndPoint?.ToString() ?? lastEndPoint;
            }

            // If ConnectedCallback arrived, prove the normal disconnect callback too.
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
            var passed = clientConstructed && connected && disconnected;
            var checks = (clientConstructed ? 1 : 0) + (connected ? 1 : 0) + (disconnected ? 1 : 0);
            var detail =
                $"IsConnected ever: {isConnectedEver}. CurrentEndPoint: {lastEndPoint ?? "never-set"}. " +
                $"Disconnected.UserInitiated={FormatNullable(disconnectedUserInitiated)}.\n" +
                $"First-chance SteamKit/runtime exceptions:\n{exceptionText}";

            return new SteamConnectionProbeResult(
                passed, transportName, protocolTypes.ToString(), clientConstructed, connected, disconnected,
                disconnectedUserInitiated, isConnectedEver, lastEndPoint, exceptionText, AssemblyVersion,
                sw.Elapsed, $"{transportName.ToUpperInvariant()} {(passed ? "PASS" : "FAIL")} — {checks}/3", detail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            var exceptionText = FormatExceptions(exceptions);
            return new SteamConnectionProbeResult(
                false, transportName, protocolTypes.ToString(), clientConstructed, connected, disconnected,
                disconnectedUserInitiated, isConnectedEver, lastEndPoint, exceptionText, AssemblyVersion,
                sw.Elapsed, $"{transportName.ToUpperInvariant()} FAIL",
                $"Stage: {stage}\n{ex}\nCaptured first-chance exceptions:\n{exceptionText}");
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
        var items = exceptions.Distinct().Take(12).ToArray();
        return items.Length == 0 ? "(none captured)" : string.Join("\n", items.Select((x, i) => $"{i + 1}. {x}"));
    }

    private static string FormatNullable(bool? value) => value.HasValue ? value.Value.ToString() : "not-received";
}
