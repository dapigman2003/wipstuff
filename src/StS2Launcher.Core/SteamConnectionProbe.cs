using System.Diagnostics;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step-05.5 transport-isolation SteamKit probe.
///
/// Step 05.3 proved SteamClient construction succeeds on iOS after replacing
/// SteamKit 3.3.1's unsupported Process.StartTime assumption. It then received
/// an early DisconnectedCallback without ever receiving ConnectedCallback.
///
/// This probe keeps authentication forbidden and isolates CM transport choice:
/// callers explicitly select WebSocket-only or TCP-only. Each attempt constructs
/// a fresh SteamClient/SteamConfiguration so server scoring from one transport
/// cannot contaminate the other test.
/// </summary>
public sealed class SteamConnectionProbe
{
    public static string AssemblyVersion =>
        typeof(SteamClient).Assembly.GetName().Version?.ToString()
        ?? "unknown";

    public Task<SteamConnectionProbeResult> RunAsync(
        string transportName,
        ProtocolTypes protocolTypes,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transportName);

        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        if (protocolTypes != ProtocolTypes.WebSocket &&
            protocolTypes != ProtocolTypes.Tcp)
        {
            throw new ArgumentOutOfRangeException(
                nameof(protocolTypes),
                "Step 05.5 intentionally permits exactly one transport per probe.");
        }

        return Task.Run(
            () => RunBlocking(
                transportName,
                protocolTypes,
                timeout,
                cancellationToken),
            cancellationToken);
    }

    private static SteamConnectionProbeResult RunBlocking(
        string transportName,
        ProtocolTypes protocolTypes,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var clientConstructed = false;
        var connected = false;
        var disconnected = false;
        bool? disconnectedUserInitiated = null;
        var disconnectRequested = false;
        string? callbackFailure = null;
        var stage = $"SteamConfiguration.Create ({transportName})";

        try
        {
            // Force exactly one transport. SteamKit 3.x enables WebSocket in its
            // default protocol set, so using an explicit configuration is the
            // cleanest way to determine which transport can operate on iOS.
            var configuration = SteamConfiguration.Create(
                builder => builder.WithProtocolTypes(protocolTypes));

            stage = $"SteamClient constructor ({transportName})";
            var steamClient = new SteamClient(configuration);
            clientConstructed = true;

            stage = $"CallbackManager constructor ({transportName})";
            var manager = new CallbackManager(steamClient);

            stage = $"callback subscription ({transportName})";
            manager.Subscribe<SteamClient.ConnectedCallback>(_ =>
            {
                connected = true;

                // Step 05.x ends at network connectivity. No SteamUser handler,
                // authentication session, credentials, or tokens are used.
                try
                {
                    disconnectRequested = true;
                    steamClient.Disconnect();
                }
                catch (Exception ex)
                {
                    callbackFailure =
                        $"Disconnect request threw {ex.GetType().Name}: {ex.Message}";
                }
            });

            manager.Subscribe<SteamClient.DisconnectedCallback>(callback =>
            {
                disconnectedUserInitiated = callback.UserInitiated;
                disconnected = true;
            });

            stage = $"SteamClient.Connect ({transportName})";
            steamClient.Connect();
            stage = $"callback pump ({transportName})";

            while (!disconnected &&
                   callbackFailure is null &&
                   stopwatch.Elapsed < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();
                manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(250));
            }

            if (!connected)
            {
                TryDisconnect(steamClient);

                var reason = disconnected
                    ? $"DisconnectedCallback arrived before ConnectedCallback. " +
                      $"UserInitiated={FormatNullableBool(disconnectedUserInitiated)}."
                    : $"No ConnectedCallback or DisconnectedCallback arrived within " +
                      $"{timeout.TotalSeconds:F0}s.";

                return Fail(
                    transportName,
                    protocolTypes,
                    clientConstructed,
                    connected,
                    disconnected,
                    disconnectedUserInitiated,
                    stopwatch.Elapsed,
                    reason);
            }

            if (callbackFailure is not null)
            {
                TryDisconnect(steamClient);

                return Fail(
                    transportName,
                    protocolTypes,
                    clientConstructed,
                    connected,
                    disconnected,
                    disconnectedUserInitiated,
                    stopwatch.Elapsed,
                    callbackFailure);
            }

            if (!disconnectRequested)
            {
                TryDisconnect(steamClient);

                return Fail(
                    transportName,
                    protocolTypes,
                    clientConstructed,
                    connected,
                    disconnected,
                    disconnectedUserInitiated,
                    stopwatch.Elapsed,
                    "ConnectedCallback arrived, but the disconnect request was not issued.");
            }

            if (!disconnected)
            {
                TryDisconnect(steamClient);

                return Fail(
                    transportName,
                    protocolTypes,
                    clientConstructed,
                    connected,
                    disconnected,
                    disconnectedUserInitiated,
                    stopwatch.Elapsed,
                    "ConnectedCallback arrived, but DisconnectedCallback did not arrive before timeout.");
            }

            return new SteamConnectionProbeResult(
                Passed: true,
                TransportName: transportName,
                Protocols: protocolTypes.ToString(),
                ClientConstructed: true,
                ConnectedCallbackReceived: true,
                DisconnectedCallbackReceived: true,
                DisconnectedUserInitiated: disconnectedUserInitiated,
                SteamKitAssemblyVersion: AssemblyVersion,
                Elapsed: stopwatch.Elapsed,
                Summary: $"{transportName.ToUpperInvariant()} PASS — 3/3",
                Detail:
                    $"{transportName}: SteamKit client constructed; ConnectedCallback received; " +
                    "disconnect requested; DisconnectedCallback received. " +
                    $"UserInitiated={FormatNullableBool(disconnectedUserInitiated)}. " +
                    "No authentication was attempted.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(
                transportName,
                protocolTypes,
                clientConstructed,
                connected,
                disconnected,
                disconnectedUserInitiated,
                stopwatch.Elapsed,
                FormatException(stage, ex));
        }
    }

    private static void TryDisconnect(SteamClient steamClient)
    {
        try
        {
            steamClient.Disconnect();
        }
        catch
        {
            // Preserve the original failure in the probe result.
        }
    }

    private static string FormatException(string stage, Exception ex)
    {
        var text = $"Stage: {stage}\n{ex}";

        const int maxLength = 3500;
        return text.Length <= maxLength
            ? text
            : text[..maxLength] + "\n…(truncated)";
    }

    private static string FormatNullableBool(bool? value) =>
        value.HasValue ? value.Value.ToString() : "not-received";

    private static SteamConnectionProbeResult Fail(
        string transportName,
        ProtocolTypes protocolTypes,
        bool clientConstructed,
        bool connected,
        bool disconnected,
        bool? disconnectedUserInitiated,
        TimeSpan elapsed,
        string detail)
    {
        var passedChecks =
            (clientConstructed ? 1 : 0) +
            (connected ? 1 : 0) +
            (disconnected ? 1 : 0);

        return new SteamConnectionProbeResult(
            Passed: false,
            TransportName: transportName,
            Protocols: protocolTypes.ToString(),
            ClientConstructed: clientConstructed,
            ConnectedCallbackReceived: connected,
            DisconnectedCallbackReceived: disconnected,
            DisconnectedUserInitiated: disconnectedUserInitiated,
            SteamKitAssemblyVersion: AssemblyVersion,
            Elapsed: elapsed,
            Summary: $"{transportName.ToUpperInvariant()} FAIL — {passedChecks}/3",
            Detail: detail);
    }
}
