# StS2 Launcher iOS — Step 05.7

Step 05.7 targets the exact SteamKit WebSocket failure exposed by Step 05.6.

Proven before this step:

- unsigned IPA build succeeds;
- install/launch/lifecycle succeed;
- Core regression self-test is 12/12;
- SteamKit2 3.3.1 loads on iOS;
- the generated macOS-only DiskArbitration framework reference is filtered before native link;
- the iOS-incompatible SteamClient Process.StartTime constructor assumption is patched in the isolated build copy;
- Valve CM directory HTTPS, DNS, raw TCP, and raw ClientWebSocket all pass on the physical iPhone (4/4);
- SteamKit WebSocket fails before ConnectedCallback with an inner NotSupportedException from NSUrlSessionHandler (`net_http_missing_sync_implementation`);
- SteamKit TCP reaches a CM endpoint but can be reset by the peer, so WebSocket remains the preferred mobile path to solve first.

Step 05.7 makes one runtime compatibility change:

- SteamKit `HttpClientPurpose.CMWebSocket` gets a dedicated `SocketsHttpHandler`;
- SteamKit `WebAPI` and `CDN` purposes keep the platform-default `HttpClient`;
- the normal SteamKit user-agent is preserved;
- the existing CM network 4/4 probe is rerun first;
- SteamKit WebSocket is then tested for constructor + ConnectedCallback + DisconnectedCallback (3/3).

No authentication, Steam Guard, ownership, depot, RuntimePatch, Godot, or game code is added.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.7.ipa
```

Expected device header:

```text
STEP 05.7 — IOS WEBSOCKET HANDLER FIX
Version 0.0.13
```

Success condition:

```text
CM NETWORK PASS — 4/4
STEAM CONNECTION PASS — 3/3
HTTP factory calls includes CMWebSocket
CM WebSocket handler: SocketsHttpHandler
```
