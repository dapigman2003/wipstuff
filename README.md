# StS2 Launcher iOS — Step 05.10

Step 05.10 isolates the first SteamKit operation after the WebSocket upgrade that can explain the current iOS AOT failure.

Proven before this step:

- unsigned IPA build/install/launch/lifecycle work;
- Core regression self-test remains 12/12;
- SteamKit2 3.3.1 loads and `SteamClient` constructs after the isolated `Process.StartTime` compatibility patch;
- the generated macOS-only `DiskArbitration` framework reference is filtered before native link;
- Valve CM directory HTTPS, DNS, raw TCP, and raw `ClientWebSocket` pass on the physical iPhone;
- SteamKit requests `HttpClientPurpose.CMWebSocket` and receives the dedicated `SocketsHttpHandler`;
- the exact `SocketsHttpHandler` + custom-`HttpMessageInvoker` `ClientWebSocket` path passes on-device;
- Step 05.9 replayed SteamKit's exact selected CM host and port through that same custom-invoker path and the HTTP WebSocket upgrade passed;
- SteamKit itself still disconnected before `ConnectedCallback`, and `PlatformNotSupported_ReflectionEmit` reappeared only inside the SteamKit path.

Step 05.10 adds one diagnostic capability only: instrument SteamKit's immediate post-upgrade path to determine whether the first outgoing `ClientHello` successfully serializes.

It attaches a metadata-only `IDebugNetworkListener` before `SteamClient.Connect()`. The listener records only direction, `EMsg`, serialized byte count, and elapsed time; it never retains raw Steam payloads. The first-chance exception listener also captures `Environment.StackTrace`, `TargetSite`, thread ID, `IsConnected`, and `CurrentEndPoint` at the instant a Reflection.Emit platform exception is observed.

Interpretation:

- `Outgoing ClientHello: NO` plus Reflection.Emit while `IsConnected at throw=True`: the WebSocket connected, but SteamKit failed while constructing/serializing the initial ClientHello before it could be exposed to the network listener;
- `Outgoing ClientHello: YES`: ClientHello serialization completed, so the next boundary is after that send attempt;
- SteamKit reaches 3/3: Step 05 is complete.

The Step 05.9 exact-endpoint replay remains as a regression check. No authentication, Steam Guard, ownership, depot, RuntimePatch, Godot, or game code is added.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.10.ipa
```

Expected device header:

```text
STEP 05.10 — CLIENTHELLO AOT DIAGNOSTICS
Version 0.0.16
```
