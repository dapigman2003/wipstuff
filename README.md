# StS2 Launcher iOS — Step 05.5

Step 05.5 isolates the remaining Steam CM connection failure below SteamKit's connection layer.

Proven before this step:

- unsigned IPA build succeeds;
- install/launch/lifecycle succeed;
- Core regression self-test is 12/12;
- SteamKit2 3.3.1 loads on iOS;
- the generated macOS-only DiskArbitration framework reference is filtered before native link;
- the iOS-incompatible SteamClient Process.StartTime constructor assumption is patched in the isolated build copy;
- WebSocket-only SteamKit constructs and posts an early non-user DisconnectedCallback without ConnectedCallback;
- TCP-only SteamKit constructs but posts neither callback before the Step 05.4 timeout.

Step 05.5 keeps all of those proven boundaries and adds a network test below SteamKit:

1. HTTPS GET to Valve's `ISteamDirectory/GetCMListForConnect` endpoint;
2. JSON endpoint discovery;
3. DNS resolution of a returned CM host;
4. raw `TcpClient` connection to a returned CM socket endpoint;
5. raw `ClientWebSocket` HTTP upgrade to `/cmsocket/` on a returned websocket endpoint;
6. rerun SteamKit WebSocket-only;
7. rerun SteamKit TCP-only.

The raw TCP/WebSocket checks do not send Steam protocol messages. The WebSocket is closed immediately after a successful HTTP upgrade. No authentication, Steam Guard, ownership, depot, RuntimePatch, Godot, or game code is added.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.5.ipa
```

Expected device header:

```text
STEP 05.5 — CM NETWORK BOUNDARY
Version 0.0.11
```
