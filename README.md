# StS2 Launcher iOS — Step 05.8

Step 05.8 isolates the `PlatformNotSupported_ReflectionEmit` failure exposed by the physical-iPhone Step 05.7 test.

Proven before this step:

- unsigned IPA build/install/launch/lifecycle work;
- Core regression self-test remains 12/12;
- SteamKit2 3.3.1 loads and `SteamClient` constructs after the isolated `Process.StartTime` compatibility patch;
- the generated macOS-only `DiskArbitration` framework reference is filtered before native link;
- Valve CM directory HTTPS, DNS, raw TCP, and raw `ClientWebSocket` pass on the iPhone (4/4);
- Step 05.7 confirmed SteamKit requests `HttpClientPurpose.CMWebSocket` and receives the dedicated `SocketsHttpHandler` client;
- the prior `NSUrlSessionHandler` synchronous-send exception is no longer the observed failure;
- SteamKit still disconnects before `ConnectedCallback`, now with `PlatformNotSupported_ReflectionEmit`.

Step 05.8 adds one diagnostic capability only: isolate whether the Reflection.Emit failure occurs in `SocketsHttpHandler` HTTPS, in the `ClientWebSocket` custom-`HttpMessageInvoker` handshake, or only inside SteamKit around that path. It also captures a larger Reflection.Emit stack excerpt from the SteamKit probe.

No authentication, Steam Guard, ownership, depot, RuntimePatch, Godot, or game code is added.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.8.ipa
```

Expected device header:

```text
STEP 05.8 — REFLECTION.EMIT ORIGIN ISOLATION
Version 0.0.14
```
