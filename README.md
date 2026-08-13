# StS2 Launcher iOS — Step 05.9

Step 05.9 isolates the remaining endpoint-selection boundary exposed by the completed physical-iPhone Step 05.8 test.

Proven before this step:

- unsigned IPA build/install/launch/lifecycle work;
- Core regression self-test remains 12/12;
- SteamKit2 3.3.1 loads and `SteamClient` constructs after the isolated `Process.StartTime` compatibility patch;
- the generated macOS-only `DiskArbitration` framework reference is filtered before native link;
- Valve CM directory HTTPS, DNS, raw TCP, and raw `ClientWebSocket` pass on the iPhone (4/4);
- SteamKit requests `HttpClientPurpose.CMWebSocket` and receives the dedicated `SocketsHttpHandler`;
- Step 05.8 passed `SocketsHttpHandler` HTTPS and `ClientWebSocket.ConnectAsync(uri, HttpMessageInvoker, token)` 2/2 on-device;
- SteamKit still disconnected before `ConnectedCallback`, but its selected CM differed from the CM used by the successful below-SteamKit control;
- Step 05.8 captured no SteamKit/runtime first-chance exception, so the earlier Step 05.7 `Reflection.Emit` first-chance exception is no longer assumed to be causal.

Step 05.9 adds one diagnostic capability only: after the unchanged SteamKit WebSocket probe fails or completes, replay **SteamKit's exact `CurrentEndPoint`** through the already-proven custom-invoker `ClientWebSocket` path.

Interpretation:

- exact endpoint replay **fails**: investigate SteamKit CM candidate selection / endpoint quality before changing SteamKit internals;
- exact endpoint replay **passes** while SteamKit still fails: endpoint reachability is eliminated and the next boundary is SteamKit's connection lifecycle around the same framework call;
- SteamKit reaches 3/3: Step 05 is complete.

No authentication, Steam Guard, ownership, depot, RuntimePatch, Godot, or game code is added.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.9.ipa
```

Expected device header:

```text
STEP 05.9 — EXACT STEAMKIT ENDPOINT REPLAY
Version 0.0.15
```
