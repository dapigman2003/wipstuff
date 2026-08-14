# StS2 Launcher iOS — Step 05.15

Step 05.15 tests one compatibility change: preserve the assemblies used by SteamKit's reflection-based protobuf serialization from iOS full trimming. SteamKit2 remains **3.4.0** and the connection test remains unauthenticated.

## Evidence entering Step 05.15

Physical-iPhone Step 05.14 finally exposed SteamKit's internally caught post-connect exception. The WebSocket connected successfully, then `CMClient.OnClientConnected()` attempted to serialize the initial `ClientHello`. The failure stack was:

```text
System.ArgumentException: Arg_GetMethNotFnd
  at System.Reflection.RuntimePropertyInfo.GetValue(...)
  at ProtoBuf.Meta.AttributeMap.ReflectionAttributeMap.TryGet(...)
  at ProtoBuf.Meta.MetaType.ApplyDefaultBehaviourImpl(...)
  at ProtoBuf.Meta.RuntimeTypeModel.GetSerializer[CMsgProtoBufHeader]()
  at ProtoBuf.Serializer.Serialize[CMsgProtoBufHeader](...)
  at SteamKit2.Internal.MsgHdrProtoBuf.Serialize(...)
  at SteamKit2.ClientMsgProtobuf<CMsgClientHello>.Serialize()
  at SteamKit2.Internal.CMClient.Send(...)
  at SteamKit2.Internal.CMClient.OnClientConnected()
```

That proves the CM network, WebSocket handler, and selected endpoint are not the failing boundary. The fatal exception is now in protobuf-net's reflection-based serializer while it inspects SteamKit-generated protobuf metadata.

## Single Step 05.15 compatibility change

The iOS project keeps `TrimMode=full`, but roots exactly these runtime assemblies:

```text
SteamKit2
protobuf-net
protobuf-net.Core
```

The intent is to preserve property accessors and metadata that protobuf-net reaches only through reflection. All other assemblies remain under full trimming. This is deliberately broader than preserving only `CMsgProtoBufHeader`/`CMsgClientHello`, because later Steam authentication and content messages use the same generated protobuf model and would otherwise fail one message type at a time.

The existing SteamKit `DebugLog` and metadata-only `IDebugNetworkListener` remain so the physical-device result is directly comparable with Step 05.14.

## Preserved regression boundaries

Step 05.15 retains:

- SteamKit2 3.4.0;
- WebSocket-only SteamKit connection;
- no authentication, password, Steam Guard, or token behavior;
- `HttpClientPurpose.CMWebSocket` -> `SocketsHttpHandler`;
- native CM directory HTTPS / DNS / raw TCP / raw WebSocket checks;
- exact `SocketsHttpHandler` + custom-invoker WebSocket isolation;
- exact SteamKit-selected endpoint replay;
- metadata-only `IDebugNetworkListener` / ClientHello observation;
- SteamKit internal `DebugLog` capture;
- Step 05.2 generated `DiskArbitration` linker-framework removal;
- isolated version-aware SteamKit `Process.StartTime` compatibility patch.

## Success / next boundary

The strongest success result is:

```text
STEAM CONNECTION PASS — 3/3
ConnectedCallback: YES
Outgoing ClientHello: YES
```

If that appears, Step 05 is complete and the next major step is Steam authentication only.

If the build fails, preserve the linker/AOT error because rooting SteamKit may expose another desktop-only native dependency that full trimming previously removed. If the app builds but SteamKit still fails, send the full `SteamKit DebugLog` stack so the next change stays on the exact serialization/AOT boundary.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.15.ipa
```

Expected device header:

```text
STEP 05.15 — PROTOBUF TRIM PRESERVATION
Version 0.0.21
TRIM ROOTS: SteamKit2 • protobuf-net • protobuf-net.Core
```
