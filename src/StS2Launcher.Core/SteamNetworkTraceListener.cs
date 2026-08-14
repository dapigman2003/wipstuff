using System.Collections.Concurrent;
using System.Diagnostics;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.15 metadata-only SteamKit network listener.
///
/// It deliberately records only direction, EMsg, serialized byte count, and elapsed
/// time. Raw Steam message payloads are never retained. For the current boundary,
/// seeing an outgoing ClientHello proves SteamKit reached CMClient.Send far enough
/// for ClientMsgProtobuf serialization to complete.
/// </summary>
public sealed class SteamNetworkTraceListener : IDebugNetworkListener
{
    private readonly Stopwatch _stopwatch;
    private readonly ConcurrentQueue<string> _events = new();
    private int _outgoingClientHelloObserved;

    public SteamNetworkTraceListener(Stopwatch stopwatch)
    {
        _stopwatch = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
    }

    public bool OutgoingClientHelloObserved =>
        Volatile.Read(ref _outgoingClientHelloObserved) != 0;

    public void OnIncomingNetworkMessage(EMsg msgType, byte[] data)
    {
        Capture("IN", msgType, data?.Length ?? 0);
    }

    public void OnOutgoingNetworkMessage(EMsg msgType, byte[] data)
    {
        if (msgType == EMsg.ClientHello)
            Interlocked.Exchange(ref _outgoingClientHelloObserved, 1);

        Capture("OUT", msgType, data?.Length ?? 0);
    }

    public string Snapshot()
    {
        var items = _events.Take(12).ToArray();
        return items.Length == 0
            ? "(no Steam messages observed)"
            : string.Join("\n", items);
    }

    private void Capture(string direction, EMsg msgType, int byteCount)
    {
        if (_events.Count >= 32) return;
        _events.Enqueue($"{_stopwatch.Elapsed.TotalMilliseconds:F0}ms {direction} {msgType} bytes={byteCount}");
    }
}
