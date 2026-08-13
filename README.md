# StS2 Launcher iOS — Step 05.13

Step 05.13 localizes the remaining iOS `PlatformNotSupported_ReflectionEmit` failure to a specific SteamKit lifecycle stage. It keeps SteamKit2 **3.4.0** from Step 05.12 and does not add another networking, protobuf, authentication, or IL workaround.

## Evidence entering Step 05.13

Physical-iPhone Step 05.12 established:

- SteamKit2 3.4.0 still fails before the public `ConnectedCallback`;
- `Outgoing ClientHello: NO` and no Steam messages reach `IDebugNetworkListener`;
- `PlatformNotSupported_ReflectionEmit` still appears;
- the exact SteamKit-selected CM endpoint still completes the WebSocket HTTP upgrade outside SteamKit through the proven `SocketsHttpHandler` / custom-invoker path;
- changing SteamKit2 3.3.1 -> 3.4.0 therefore did not remove the AOT boundary.

Step 05.11 had already shown `RuntimeTypeModel.Default.AutoCompile` was `False -> False`, so the retained protobuf setting is not treated as a fix.

## Single Step 05.13 diagnostic change

The Steam connection probe now timestamps the active stage before each synchronous boundary:

1. protobuf-net AOT configuration
2. `SteamConfiguration.Create`
3. `SteamClient` constructor
4. attach `IDebugNetworkListener`
5. `CallbackManager` constructor
6. subscribe `ConnectedCallback`
7. subscribe `DisconnectedCallback`
8. `SteamClient.Connect` call
9. post-Connect callback/state pump
10. disconnect/result formatting

When a first-chance Reflection.Emit exception appears, the app records:

- elapsed milliseconds;
- the active stage name;
- managed thread ID;
- `IsConnected` at throw;
- `CurrentEndPoint` at throw;
- `RuntimeFeature.IsDynamicCodeSupported`;
- `RuntimeFeature.IsDynamicCodeCompiled`;
- the existing best-effort caller stack.

The completed SteamKit result also displays the full stage timeline and a compact `ReflectionEmit observed stage(s)` section before the longer exception dump.

## Preserved regression boundaries

Step 05.13 retains:

- SteamKit2 3.4.0;
- WebSocket-only SteamKit connection;
- no authentication;
- `HttpClientPurpose.CMWebSocket` -> `SocketsHttpHandler`;
- native CM HTTPS/DNS/TCP/WebSocket checks;
- exact `SocketsHttpHandler` + custom-invoker WebSocket isolation;
- exact SteamKit-selected endpoint replay;
- metadata-only `IDebugNetworkListener` / ClientHello observation;
- the Step 05.2 generated `DiskArbitration` linker-framework removal;
- the isolated version-aware SteamKit `Process.StartTime` compatibility patch;
- the retained protobuf `AutoCompile=false` regression setting.

## Interpretation

- Reflection.Emit at `SteamConfiguration.Create` or `SteamClient constructor` means the AOT issue is initialization-time and is earlier than CM transport/message serialization.
- Reflection.Emit at a `CallbackManager`/subscription stage means callback setup itself needs an iOS-safe path.
- Reflection.Emit during `SteamClient.Connect call` means it is synchronous inside the connect entry path.
- Reflection.Emit during `post-Connect callback/state pump`, especially with `IsConnected=True`, puts it inside asynchronous connection/post-connect processing.
- `Outgoing ClientHello: YES` means initial Steam message serialization completed and the boundary moved later.
- `STEAM CONNECTION PASS — 3/3` completes Step 05 and allows Step 06 authentication-only work.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.13.ipa
```

Expected device header:

```text
STEP 05.13 — REFLECTION.EMIT STAGE LOCALIZATION
Version 0.0.19
```
