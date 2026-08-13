# Step 05.9 device test

The completed Step 05.8 physical-iPhone test proved:

```text
CM NETWORK PASS — 4/4
SOCKETS HANDLER ISOLATION PASS — 2/2
Custom-invoker WebSocket: PASS
STEAMKIT WEBSOCKET FAIL — 2/3
ConnectedCallback: NO
DisconnectedCallback: YES
First-chance SteamKit/runtime exceptions: (none captured)
```

The successful custom-invoker control and SteamKit did not use the same CM endpoint. Step 05.9 removes only that mismatch.

Build the unsigned IPA with the normal Codemagic workflow and install it on the same physical iPhone. Run **Run Step 05.9 Exact Endpoint Replay Test** once with the app kept in the foreground.

The test performs four stages:

1. Re-run native CM network checks (4/4).
2. Re-run the proven `SocketsHttpHandler` / custom-invoker regression checks (2/2).
3. Run the unchanged SteamKit WebSocket probe and capture its selected `CurrentEndPoint`.
4. Replay that exact endpoint with a plain `ClientWebSocket` using the same `CMWebSocket` factory client and no Steam protocol payload.

Send a screenshot containing:

```text
CM NETWORK ... x/4
SOCKETS HANDLER ISOLATION ... x/2
STEAMKIT WEBSOCKET ... x/3
CurrentEndPoint: ...
EXACT STEAMKIT ENDPOINT REPLAY PASS/FAIL
SteamKit-selected endpoint: ...
Replay URI: ...
Replay detail: ...
Replay exception/stack: ...
```

Interpretation:

- `EXACT STEAMKIT ENDPOINT REPLAY FAIL`: the selected CM itself does not reproduce the known-good upgrade; investigate candidate selection/quality next.
- `EXACT STEAMKIT ENDPOINT REPLAY PASS` while SteamKit remains 2/3: the same endpoint and same framework WebSocket path work outside SteamKit, so the next diagnostic belongs inside SteamKit's WebSocket lifecycle.
- `STEAM CONNECTION PASS — 3/3`: Step 05 is complete and Step 06 authentication can begin.

This step performs no authentication.
