# StS2 Launcher iOS — Step 05.12

Step 05.12 is a controlled dependency comparison. It changes the SteamKit2 package from 3.3.1 to 3.4.0 while preserving the already-proven iOS networking work and the Step 05.10/05.11 diagnostics.

The completed Step 05.11 physical-iPhone run established that the protobuf `AutoCompile` experiment was not the fix:

- `RuntimeTypeModel.Default.AutoCompile` was already `False` before the Step 05.11 assignment (`False -> False`);
- `PlatformNotSupported_ReflectionEmit` still appeared;
- in that run it appeared very early (~5 ms), before a SteamKit endpoint had been reported;
- `Outgoing ClientHello: NO` and no Steam messages reached `IDebugNetworkListener`;
- SteamKit disconnected non-user-initiated;
- replaying SteamKit's selected WebSocket CM still succeeded outside SteamKit.

Therefore Step 05.12 does **not** add another serializer workaround. It answers the narrower question: does the newer SteamKit2 release change this iOS/AOT connection behavior on the otherwise same test surface?

## Single Step 05.12 dependency change

Core now references:

```xml
<PackageReference Include="SteamKit2" Version="3.4.0" />
```

Everything else remains intentionally comparable to Step 05.11:

- WebSocket-only SteamKit connection; no authentication;
- `HttpClientPurpose.CMWebSocket` still receives `SocketsHttpHandler`;
- native CM HTTPS/DNS/TCP/WebSocket regression checks remain;
- exact `SocketsHttpHandler` + custom-invoker WebSocket isolation remains;
- exact SteamKit-selected endpoint replay remains;
- metadata-only `IDebugNetworkListener` remains;
- `RuntimeTypeModel.Default.AutoCompile = false` remains as a harmless regression setting (Step 05.11 proved it was already false on device);
- the generated `DiskArbitration` linker-framework filter remains;
- the isolated SteamKit constructor compatibility patch remains conditional: if SteamKit2 3.4.0 still contains exactly one `Process.StartTime` call it is replaced with `DateTime.UtcNow`; if 3.4.0 no longer contains that unsupported call, the patcher verifies the call is absent and leaves the assembly untouched. More than one match is a hard failure.

No authentication, Steam Guard, ownership, depot, Godot, RuntimePatch, or game code is added.

## Interpretation

- `STEAM CONNECTION PASS — 3/3` means the newer SteamKit eliminated the current Step 05 boundary and Step 06 can begin with authentication only.
- `Outgoing ClientHello: YES` but connection still fails means the dependency upgrade moved the boundary beyond initial message construction/serialization.
- `Outgoing ClientHello: NO` with the same Reflection.Emit behavior means the SteamKit upgrade alone does not solve the iOS AOT issue, and the next step should identify/replace the exact emit-dependent component rather than return to networking.
- A build-time failure in the compatibility patch is also useful evidence: it means SteamKit 3.4.0 changed the constructor surface and the patch must be re-audited before any device conclusion.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.12.ipa
```

Expected device header:

```text
STEP 05.12 — STEAMKIT 3.4.0 COMPARISON
Version 0.0.18
```
