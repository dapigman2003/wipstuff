# StS2 Launcher iOS — Step 05.14

Step 05.14 captures SteamKit2's own internal debug log around the already-isolated CM WebSocket failure. It keeps SteamKit2 **3.4.0**, the Step 05.7 `SocketsHttpHandler` CM WebSocket factory, the exact-endpoint replay, and every previously proven iOS build/network fix. It adds no authentication.

## Evidence entering Step 05.14

Physical-iPhone Step 05.13 established:

- the recurring `PlatformNotSupported_ReflectionEmit` first-chance exception fired during this project's own `ProtobufAotCompatibility.Configure()` diagnostic at about 4 ms;
- `SteamConfiguration.Create` did not begin until about 6 ms and `SteamClient.Connect` until about 16 ms;
- therefore that Reflection.Emit observation is not reliable evidence of the later SteamKit connection failure;
- the SteamKit connection still failed before public `ConnectedCallback` and before an outgoing `ClientHello` reached `IDebugNetworkListener`;
- the exact SteamKit-selected CM endpoint still completed a WebSocket HTTP upgrade outside SteamKit.

Step 05.14 therefore removes the no-op protobuf compatibility diagnostic instead of continuing to chase its first-chance exception.

## Single Step 05.14 diagnostic change

The Steam connection probe enables SteamKit's public `DebugLog` only for the duration of the unauthenticated connection test and captures its category/message output with elapsed timestamps.

This matters because SteamKit's CM connection code catches exceptions raised immediately after the transport connects, logs the exception to `DebugLog`, and then disconnects. Capturing that log should reveal the exception that was previously hidden behind the generic non-user disconnect.

The probe also retains the metadata-only `IDebugNetworkListener` and first-chance exception capture as secondary diagnostics. It never records raw Steam network payloads.

## Preserved regression boundaries

Step 05.14 retains:

- SteamKit2 3.4.0;
- WebSocket-only SteamKit connection;
- no authentication, password, Steam Guard, or token behavior;
- `HttpClientPurpose.CMWebSocket` -> `SocketsHttpHandler`;
- native CM directory HTTPS / DNS / raw TCP / raw WebSocket checks;
- exact `SocketsHttpHandler` + custom-invoker WebSocket isolation;
- exact SteamKit-selected endpoint replay;
- metadata-only `IDebugNetworkListener` / ClientHello observation;
- Step 05.2 generated `DiskArbitration` linker-framework removal;
- isolated version-aware SteamKit `Process.StartTime` compatibility patch.

## Success / next boundary

If `STEAM CONNECTION PASS — 3/3` appears, Step 05 is complete and the next major step is authentication only.

If SteamKit still fails, the most important new section is:

```text
SteamKit post-connect exception logged: YES/NO
Connection-setup exception logged: YES/NO
SteamKit DebugLog:
...
```

That log should determine the next narrow compatibility change.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.14.ipa
```

Expected device header:

```text
STEP 05.14 — STEAMKIT INTERNAL ERROR CAPTURE
Version 0.0.20
```
