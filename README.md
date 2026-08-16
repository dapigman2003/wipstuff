# StS2 Launcher iOS — Step 12.4 stabilization

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–12 are complete on a physical iPhone.** Step 12.3 / `0.0.37` is the last physically proven baseline.

This archive is **Step 12.4 / `0.0.38`**, a post-completion cleanup and bug-hardening candidate. It adds no Step 13 functionality.

The current proven stack includes persistent Steam authentication in iOS Keychain, ownership verification, PICS depot/manifest discovery, controlled CDN access, complete resumable one-depot acquisition, independently verified source-cache reuse, and the Step 12 install/update/repair manager with AOT-safe receipt JSON, full SHA-1 staging verification, and rollback-safe replacement.

## Step 12.4 hardening

- malformed/corrupt local receipts are rejected cleanly and classified for repair;
- wrong App ID/depot/branch receipt identity is rejected;
- interrupted receipt JSON writes clean their `.tmp` file best-effort;
- manager cleanup/rollback errors no longer mask the original result;
- Step 11 partial-file resume accounting is committed only after a complete checksum scan;
- unreadable completed Step 11 caches are treated as untrusted and can be reacquired;
- the known iOS CDN `TimeoutException` failover behavior is backported to the still-exposed Step 09/10 regression download paths;
- stale startup/test/build labels were cleaned up;
- the Foundation 5/5 action cannot overlap another Steam operation through the UI.

All proven Step 05 iOS compatibility fixes remain unchanged: SteamKit2 `3.4.0`, WebSocket CM transport, CMWebSocket-only `SocketsHttpHandler`, full trimming with `SteamKit2`/`protobuf-net`/`protobuf-net.Core` rooted, the narrow DiskArbitration linker filter, and the isolated build-only Process.StartTime patch.

## Build

Use Codemagic workflow `ios-step-12-4`. The expected app version is `0.0.38 (38)` and the expected device header is:

```text
STEP 12.4 — POST-STEP-12 STABILIZATION
```

See `docs/STEP-12.4-STABILIZATION.md` for the short regression gate before adopting this as the new baseline.

## Scope boundary

No Step 13/offline-state work, multi-depot composition, compatibility inventory, Mono.Cecil preparation, Godot/runtime execution, Steam Cloud, or Workshop work is included in this archive.
