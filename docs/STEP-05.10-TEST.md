# Step 05.10 device test

The completed Step 05.9 physical-iPhone test proved:

```text
SOCKETS HANDLER ISOLATION PASS — 2/2
STEAMKIT WEBSOCKET FAIL — 2/3
ConnectedCallback: NO
DisconnectedCallback: YES
PlatformNotSupportedException: PlatformNotSupported_ReflectionEmit
EXACT STEAMKIT ENDPOINT REPLAY PASS
```

That removes both the .NET WebSocket implementation and SteamKit's chosen remote CM endpoint as the distinguishing boundary. Step 05.10 observes SteamKit's first outgoing CM message and captures the Reflection.Emit caller context more aggressively.

Build the unsigned IPA with the normal Codemagic workflow and install it on the same physical iPhone. Run **Run Step 05.10 ClientHello Diagnostics** once with the app kept in the foreground.

The test performs four stages:

1. Re-run native CM network checks.
2. Re-run the proven `SocketsHttpHandler` / custom-invoker checks.
3. Run SteamKit with a metadata-only `IDebugNetworkListener` and enhanced first-chance Reflection.Emit context capture.
4. Re-run the exact SteamKit-selected endpoint replay as a regression check.

Send a screenshot containing the completed SteamKit section, especially:

```text
STEAMKIT WEBSOCKET ... x/3
ConnectedCallback: ...
DisconnectedCallback: ...
IsConnected ever: ...
CurrentEndPoint: ...
Outgoing ClientHello: YES/NO
Debug network trace: ...
ReflectionEmit context: ...
Caller stack: ...
EXACT STEAMKIT ENDPOINT REPLAY PASS/FAIL
```

The trace stores message metadata only, not raw Steam message payloads. This step performs no authentication.
