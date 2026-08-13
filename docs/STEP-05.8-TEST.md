# Step 05.8 device test

Step 05.7 moved the WebSocket boundary: native CM networking still passed 4/4, SteamKit requested `CMWebSocket`, the factory returned `SocketsHttpHandler`, and the old `NSUrlSessionHandler` synchronous-send failure disappeared. The new first-chance failure was:

```text
PlatformNotSupportedException: PlatformNotSupported_ReflectionEmit
```

Step 05.8 does not guess at a patch. It isolates the origin of that exception before changing SteamKit again.

Build the unsigned IPA with the normal Codemagic workflow and install it on the same physical iPhone.

Run **Run Step 05.8 Reflection.Emit Isolation Test** once with the app kept in the foreground.

The test performs three stages:

1. Re-run the proven native CM network regression checks (4/4).
2. Exercise the exact `CMWebSocket` factory client below SteamKit:
   - HTTPS using `SocketsHttpHandler`;
   - raw `ClientWebSocket.ConnectAsync(uri, HttpMessageInvoker, token)` using the factory-created client.
3. Re-run the unchanged SteamKit WebSocket connection probe and capture a larger stack excerpt for AOT/Reflection.Emit failures.

Report or screenshot these sections:

```text
CM NETWORK ... x/4
SOCKETS HANDLER ISOLATION ... x/2
SocketsHttpHandler HTTPS: PASS/FAIL
Custom-invoker WebSocket: PASS/FAIL
Handler exception/stack: ...
STEAMKIT WEBSOCKET ... x/3
ConnectedCallback: YES/NO
DisconnectedCallback: YES/NO
HTTP factory calls: ...
CurrentEndPoint: ...
First-chance SteamKit/runtime exceptions: ...
```

Interpretation:

- HTTPS fails: `SocketsHttpHandler` itself is the iOS/AOT boundary.
- HTTPS passes but custom-invoker WebSocket fails: the `ClientWebSocket` + supplied `HttpMessageInvoker` path is the boundary.
- Handler isolation passes 2/2 but SteamKit still fails: the remaining Reflection.Emit call is SteamKit-specific around an otherwise-working framework path.
- SteamKit reaches 3/3: Step 05 is complete and Step 06 can begin.

This step performs no authentication.
