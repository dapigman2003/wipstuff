# StS2 Launcher iOS — Step 05.11

Step 05.11 tests one narrow AOT compatibility hypothesis exposed by the completed Step 05.10 physical-iPhone run.

Step 05.10 proved on-device that:

- native CM networking still passes;
- the dedicated `SocketsHttpHandler` path passes;
- SteamKit's exact selected WebSocket endpoint can be replayed successfully outside SteamKit;
- SteamKit itself reaches an actually connected WebSocket (`IsConnected ever: True`);
- no Steam message reaches `IDebugNetworkListener`;
- `Outgoing ClientHello: NO`;
- `PlatformNotSupported_ReflectionEmit` appears while SteamKit is connected.

That strongly localizes the current failure to SteamKit's immediate post-WebSocket message construction/serialization boundary rather than iOS networking, endpoint selection, or the .NET WebSocket implementation.

## Single Step 05.11 change

Before constructing the SteamKit configuration, Step 05.11 configures the transitive protobuf-net runtime model with:

```csharp
RuntimeTypeModel.Default.AutoCompile = false;
```

The purpose is to test whether preventing protobuf-net from runtime-compiling serializers avoids the Reflection.Emit path on iOS AOT and allows SteamKit's initial `ClientHello` to serialize and send.

This is deliberately an experiment, not a broad protobuf replacement. SteamKit2 remains pinned to 3.3.1 and all previously proven iOS compatibility fixes remain intact.

The device UI reports the protobuf-net assembly version and the `AutoCompile` value before/after the change. The metadata-only `IDebugNetworkListener` from Step 05.10 remains so the test can directly report whether an outgoing `ClientHello` becomes visible. No raw Steam message payload is retained.

## Interpretation

- `Protobuf AOT mode: ... AutoCompile: True -> False` followed by `Outgoing ClientHello: YES` means this experiment moved the serializer boundary.
- `STEAM CONNECTION PASS — 3/3` means Step 05 is effectively complete and the next major step can be Steam authentication only.
- Reflection.Emit still occurring with `Outgoing ClientHello: NO` means disabling runtime compilation is insufficient; the next step should target an AOT/pre-generated serializer strategy rather than networking.

The native 4/4 checks, SocketsHttpHandler 2/2 isolation, and exact SteamKit-endpoint replay remain regression checks. No authentication, Steam Guard, ownership, depot, Godot, RuntimePatch, or game code is added.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.11.ipa
```

Expected device header:

```text
STEP 05.11 — PROTOBUF NO-EMIT TEST
Version 0.0.17
```
