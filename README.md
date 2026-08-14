# StS2 Launcher iOS — Step 05.16 Finalization

> **05.16.1 source hotfix:** the first Codemagic attempt stopped at host-test compilation because MSTest 4 removed `Assert.ThrowsException`; the parameterized-test attribute also produced an MSTest 4 obsolescence warning. This source hotfix uses `Assert.ThrowsExactly` and `TestMethod` + `DataRow`. The runtime app remains Step 05.16 / version 0.0.22 because no app behavior changed.


Step 05.15 reached the physical-device success gate:

```text
STEAM CONNECTION PASS — 3/3
```

Step 05.16 is the final cleanup release for the Step 01–05 foundation. It adds no new launcher capability and no authentication. Its purpose is to preserve the fixes that actually proved necessary, remove the temporary diagnostics used to discover them, and make automated tests a required build gate.

## What remains in the runtime

The final Step 05 runtime keeps only the proven foundation:

- UIKit app/scene startup and lifecycle wiring;
- the platform-neutral launcher Core/state machine;
- the iOS Keychain credential-store implementation, exercised only with harmless sentinels;
- SteamKit2 3.4.0, WebSocket-only, unauthenticated CM connection;
- `SocketsHttpHandler` only for `HttpClientPurpose.CMWebSocket`;
- `TrimMode=full` with `SteamKit2`, `protobuf-net`, and `protobuf-net.Core` rooted for reflection-based Steam protobuf serialization;
- the generated `DiskArbitration` linker-framework filter;
- the isolated, version-aware SteamKit `Process.StartTime` iOS compatibility patch.

The Step 05.8–05.14 endpoint replay, raw network, handler-isolation, first-chance exception, SteamKit DebugLog, and metadata network tracing probes are removed. They were diagnostic scaffolding, not final runtime behavior.

## Automated test gate

Codemagic now runs host unit tests before installing the iOS workload or starting the iOS publish. A failing test stops the build.

The unit tests cover:

- launcher state transitions, reset behavior, snapshots, UI text, and the existing Core 12/12 regression self-test;
- credential-store set/get/overwrite/delete semantics and cleanup using an in-memory test store;
- the Steam CM HTTP-handler policy and SteamKit version pin;
- the 3/3 connection-result contract;
- the five-gate final foundation result, including UI-startup and lifecycle requirements.

UIKit, real iOS Keychain, native linking/AOT, and live Steam CM connectivity are platform/integration boundaries and cannot be proven by host unit tests. They remain covered by repository/build validation plus the single physical-device verification button in the app. See `docs/TESTING.md`.

## Final device check

Build/install the IPA and tap:

```text
Run Steps 01–05 Device Verification
```

Expected result:

```text
FOUNDATION PASS — 5/5
CORE SELF-TEST PASS — 12/12
CREDENTIAL STORE PASS — 7/7
STEAM CONNECTION PASS — 3/3
ConnectedCallback: YES
DisconnectedCallback: YES
CMWebSocket factory used: YES
```

The Keychain sentinel is deleted before the final check completes.

## Scope boundary

Step 05.16 still sends no login, password, Steam Guard code, refresh token, ownership request, depot request, game content, or Godot traffic.

After this finalization build passes on the physical iPhone, the next major project step is **Step 06 — Steam authentication session only**. Ownership remains a later step.

Expected Codemagic artifact:

```text
artifacts/StS2-Launcher-Step-05.16.ipa
```

Expected device header:

```text
STEP 05.16 — FOUNDATION FINALIZATION
Version 0.0.22
```
