# StS2 Launcher iOS — Step 11

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 11 — interrupted-download resume for one selected direct public Slay the Spire 2 depot.**

Steps 01–10 are treated as closed regressions. Step 10 proved that one complete selected public depot can be queued, downloaded into temporary staging, SHA-1 verified file-by-file, cancelled cleanly, and atomically committed on a physical iPhone. Step 11 adds only safe reuse of interrupted staging data.

The Step 11 operation:

1. restores the saved Keychain Steam session;
2. requires a matching returned SteamID;
3. re-proves Step 07 ownership for App ID `2868840`;
4. re-runs Step 08 PICS metadata discovery;
5. selects one direct depot with a visible `public` manifest using the existing macOS-first policy;
6. requests the depot key, one manifest request code, and a bounded CDN server set;
7. downloads the selected manifest in memory and builds the same safe Step 10 file plan;
8. uses a deterministic resume path tied to the selected depot + manifest;
9. reuses a complete staged file only after its full manifest SHA-1 passes;
10. scans an interrupted `.step11.part` file chunk-by-chunk using each Steam manifest chunk's Adler-32 checksum;
11. downloads only chunks that are missing or fail that local checksum;
12. flushes each completed chunk to improve process-interruption durability;
13. requires the complete reconstructed file to pass the manifest SHA-1 before it becomes a completed staged file;
14. preserves resume staging on user cancellation, timeout, transient failure, or force-quit/process termination;
15. after every file is verified, validates that staging contains only expected manifest paths and atomically renames it to the final directory.

Paths are isolated beneath:

```text
Documents/StS2Launcher/Step11-ResumableDepot/.resume/<depot>-<manifest>/...
Documents/StS2Launcher/Step11-ResumableDepot/complete/<depot>/<manifest>/...
```

No separate resume journal containing Steam keys/tokens is written. The local staged bytes themselves are revalidated against the current manifest.

## Explicitly not included

Step 11 does **not** add:

- installed-manifest state;
- updating an already committed depot;
- old-manifest/new-manifest delta migration;
- repair orchestration;
- multi-depot app installation;
- background downloader service;
- Godot/game runtime integration;
- Steam Cloud;
- Workshop.

## Secret handling

Persisted in the device-bound Keychain:

- Steam account name;
- SteamID64;
- Steam refresh token.

Never displayed/logged/persisted by the Step 11 result path:

- Steam password;
- Steam Guard secret/code;
- raw ownership-ticket bytes;
- PICS access-token value;
- depot decryption-key bytes;
- manifest request-code value;
- CDN auth-token value;
- raw manifest body;
- raw chunk buffers.

## Proven foundation retained

- UIKit launch/lifecycle;
- Core self-test 12/12;
- real iOS Keychain regression 7/7;
- SteamKit2 3.4.0;
- WebSocket-only CM connection;
- `SocketsHttpHandler` for `HttpClientPurpose.CMWebSocket`;
- `TrimMode=full` with `SteamKit2`, `protobuf-net`, `protobuf-net.Core` trim roots;
- generated `DiskArbitration` framework removal;
- isolated/version-aware `Process.StartTime` SteamKit iOS compatibility patch;
- Steps 06–06.3.1 authentication / Guard / saved-session behavior;
- Step 07 ownership proof;
- Step 08 depot/manifest discovery;
- Step 09 controlled single-file CDN proof;
- Step 10 complete one-depot queue/cancel/staging/atomic-commit proof.

See `docs/STEP-11-TEST.md` for the physical-device completion gate.
