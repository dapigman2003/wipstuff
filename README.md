# StS2 Launcher iOS — Step 12.4.1 download-cache test control

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–12 are complete and closed on a physical iPhone.** Step 12.4 / `0.0.38` is the current stabilized baseline. Its short regression was reported working correctly on-device; the only path not freshly forced in that pass was CDN acquisition because the already-valid Step 11 cache was reused.

This archive is **Step 12.4.1 / `0.0.39`**, a maintenance/test-only candidate. It adds no Step 13 functionality.

The current proven stack includes persistent Steam authentication in iOS Keychain, ownership verification, PICS depot/manifest discovery, controlled CDN access, complete resumable one-depot acquisition, independently verified source-cache reuse, and the Step 12 install/update/repair manager with AOT-safe receipt JSON, full SHA-1 staging verification, and rollback-safe replacement.

## Step 12.4.1 addition

Two local maintenance/test controls are added:

- **Clear Download Cache Only (Keep Managed Install)** deletes only `Step11-ResumableDepot`, including completed and resumable source-cache data. It does not delete the Step 12 managed install and does not touch the saved Steam Keychain session.
- **Prepare Fresh Download Test (Force Update + Clear Cache)** first prepares the existing synthetic `UpdateAvailable` receipt state, then clears the Step 11 source cache. The next **Inspect + Install / Update / Repair** run must reacquire the current public depot from Steam, verify the source, exercise the normal Update path, atomically commit, and finish `UpToDate`.

The purpose is to force a real CDN regression without deleting or corrupting the stable managed game installation.

All Step 12.4 hardening remains unchanged, as do the proven Step 05 iOS compatibility fixes: SteamKit2 `3.4.0`, WebSocket CM transport, CMWebSocket-only `SocketsHttpHandler`, full trimming with `SteamKit2`/`protobuf-net`/`protobuf-net.Core` rooted, the narrow DiskArbitration linker filter, and the isolated build-only Process.StartTime patch.

## Build

Use Codemagic workflow `ios-step-12-4-1`. The expected app version is `0.0.39 (39)` and the expected device header is:

```text
STEP 12.4.1 — DOWNLOAD CACHE TEST CONTROL
```

See `docs/STEP-12.4.1-CACHE-TEST.md` for the fresh-download regression procedure.

## Scope boundary

No Step 13/offline-state work, multi-depot composition, compatibility inventory, Mono.Cecil preparation, Godot/runtime execution, Steam Cloud, or Workshop work is included in this archive.
