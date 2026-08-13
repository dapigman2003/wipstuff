# Step 05.11 device test

The completed Step 05.10 physical-iPhone run reached an actually connected SteamKit WebSocket, but no Steam message was observed and `Outgoing ClientHello` remained `NO`. `PlatformNotSupported_ReflectionEmit` occurred while `IsConnected` was true, and replaying the exact selected CM endpoint outside SteamKit passed.

Step 05.11 changes only protobuf-net runtime serializer compilation: `RuntimeTypeModel.Default.AutoCompile` is set to `false` before SteamKit connects. All previous networking and endpoint checks remain regressions.

Build the unsigned IPA with the normal Codemagic workflow, install it on the same physical iPhone, and run **Run Step 05.11 Protobuf No-Emit Test** once with the app in the foreground.

Send a screenshot containing the completed SteamKit section, especially:

```text
Protobuf AOT mode: ...
STEAMKIT WEBSOCKET ... x/3
ConnectedCallback: ...
DisconnectedCallback: ...
IsConnected ever: ...
CurrentEndPoint: ...
Outgoing ClientHello: YES/NO
Debug network trace: ...
First-chance SteamKit/runtime exceptions: ...
EXACT STEAMKIT ENDPOINT REPLAY PASS/FAIL
```

The network trace stores metadata only, not raw Steam message payloads. This step performs no authentication.
