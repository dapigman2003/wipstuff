using System.Diagnostics;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step-05 network-only SteamKit probe.
///
/// It deliberately performs no authentication and sends no account credentials.
/// The probe constructs SteamKit, connects to a Steam CM, observes the official
/// ConnectedCallback, requests a disconnect, and observes DisconnectedCallback.
/// </summary>
public sealed class SteamConnectionProbe
{
    public static string AssemblyVersion =>
        typeof(SteamClient).Assembly.GetName().Version?.ToString()
        ?? "unknown";

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
        var stopwatch = Stopwatch.StartNew();

        var clientConstructed = false;
        var connected = false;
        var disconnected = false;
        var disconnectRequested = false;
        string? callbackFailure = null;

        try
        {
            var steamClient = new SteamClient();
            clientConstructed = true;

            var manager = new CallbackManager(steamClient);

            manager.Subscribe<SteamClient.ConnectedCallback>(_ =>
            {
                connected = true;

                // This step stops here on purpose. No SteamUser handler is used,
                // no authentication session is started, and no credentials exist.
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

            manager.Subscribe<SteamClient.DisconnectedCallback>(_ =>
            {
                disconnected = true;
            });

            steamClient.Connect();

            while (!disconnected &&
                   callbackFailure is null &&
                   stopwatch.Elapsed < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // SteamKit callbacks are explicitly pumped through CallbackManager,
                // matching the upstream sample architecture.
                manager.RunWaitCallbacks(TimeSpan.FromMilliseconds(250));
            }

            if (!connected)
            {
                TryDisconnect(steamClient);

                return Fail(
                    clientConstructed,
                    connected,
                    disconnected,
                    stopwatch.Elapsed,
                    "No ConnectedCallback was received before timeout.");
            }

            if (callbackFailure is not null)
            {
                TryDisconnect(steamClient);

                return Fail(
                    clientConstructed,
                    connected,
                    disconnected,
                    stopwatch.Elapsed,
                    callbackFailure);
            }

            if (!disconnectRequested)
            {
                TryDisconnect(steamClient);

                return Fail(
                    clientConstructed,
                    connected,
                    disconnected,
                    stopwatch.Elapsed,
                    "ConnectedCallback arrived, but the disconnect request was not issued.");
            }

            if (!disconnected)
            {
                TryDisconnect(steamClient);

                return Fail(
                    clientConstructed,
                    connected,
                    disconnected,
                    stopwatch.Elapsed,
                    "ConnectedCallback arrived, but DisconnectedCallback did not arrive before timeout.");
            }

            return new SteamConnectionProbeResult(
                Passed: true,
                ClientConstructed: true,
                ConnectedCallbackReceived: true,
                DisconnectedCallbackReceived: true,
                SteamKitAssemblyVersion: AssemblyVersion,
                Elapsed: stopwatch.Elapsed,
                Summary: "STEAM CONNECTION PASS — 3/3",
                Detail:
                    "SteamKit client constructed; ConnectedCallback received; " +
                    "disconnect requested; DisconnectedCallback received. " +
                    "No authentication was attempted.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(
                clientConstructed,
                connected,
                disconnected,
                stopwatch.Elapsed,
                $"{ex.GetType().Name}: {ex.Message}");
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

    private static SteamConnectionProbeResult Fail(
        bool clientConstructed,
        bool connected,
        bool disconnected,
        TimeSpan elapsed,
        string detail)
    {
        var passedChecks =
            (clientConstructed ? 1 : 0) +
            (connected ? 1 : 0) +
            (disconnected ? 1 : 0);

        return new SteamConnectionProbeResult(
            Passed: false,
            ClientConstructed: clientConstructed,
            ConnectedCallbackReceived: connected,
            DisconnectedCallbackReceived: disconnected,
            SteamKitAssemblyVersion: AssemblyVersion,
            Elapsed: elapsed,
            Summary: $"STEAM CONNECTION FAIL — {passedChecks}/3",
            Detail: detail);
    }
}
