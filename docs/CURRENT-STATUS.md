# Current project status

Date: 2026-08-13

## Proven on physical iPhone

Step 05.15 reached:

```text
STEAM CONNECTION PASS — 3/3
```

This closes the unauthenticated SteamKit CM connection boundary. The final working combination is:

- SteamKit2 3.4.0;
- WebSocket-only Steam CM transport;
- dedicated `SocketsHttpHandler` for `HttpClientPurpose.CMWebSocket`;
- full trimming with `SteamKit2`, `protobuf-net`, and `protobuf-net.Core` rooted;
- generated DiskArbitration framework removal;
- isolated version-aware Process.StartTime compatibility patch.

## Current source

Step 05.16 / version 0.0.22 is a cleanup and regression release only. It removes the temporary diagnostics from Steps 05.8–05.14, adds host unit tests as a Codemagic build gate, and condenses the physical-device checks into one Steps 01–05 verification action.

It adds no authentication or later-stage subsystem.

## Gate to close Step 05 completely

Run the Step 05.16 IPA on the physical iPhone and obtain:

```text
FOUNDATION PASS — 5/5
```

After that final cleanup smoke test, proceed to **Step 06 — Steam authentication session only**. Do not add ownership in Step 06.
## 05.16.1 test-gate hotfix

The first Step 05.16 Codemagic run reached the host unit-test gate and stopped before the iOS build because MSTest 4.3.2 no longer exposes `Assert.ThrowsException`. It also reported the `DataTestMethod` obsolescence warning. The 05.16.1 source hotfix changes only test code (`Assert.ThrowsExactly`, `TestMethod` + `DataRow`) and strengthens repository validation against those legacy APIs. Runtime Step 05.16 remains version 0.0.22 and still awaits the final physical-device `FOUNDATION PASS — 5/5` verification.
