# StS2 Launcher iOS — Step 09

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 09 — download exactly one controlled small Slay the Spire 2 file for Steam App ID 2868840.**

Steps 01–08 are treated as closed regressions. Step 08 proved depot/visible-manifest discovery on the physical iPhone. Step 09 adds one tightly bounded content-access proof after re-proving the saved Steam session, exact Steam identity, ownership, and PICS metadata.

The Step 09 operation:

1. restores the saved Keychain Steam session;
2. requires a matching returned SteamID;
3. re-runs the Step 07 ownership-ticket proof for App ID `2868840`;
4. re-runs the Step 08 PICS metadata discovery;
5. selects one **direct** depot with a visible `public` manifest, preferring macOS metadata and then language-neutral/English metadata;
6. requests exactly that depot's decryption key;
7. requests one short-lived manifest request code;
8. obtains a bounded set of Steam CDN/SteamCache endpoints;
9. downloads one depot manifest in memory;
10. selects the smallest safe, non-empty regular file no larger than **2 MiB**;
11. downloads only that file's chunks;
12. verifies the assembled file SHA-1 against the manifest;
13. atomically persists only the verified final file under the app Documents directory.

The final test file is written beneath:

```text
Documents/StS2Launcher/Step09-SingleFile/<depot>/<manifest>/<manifest path>
```

## Explicitly not included

Step 09 does **not** add:

- a full-depot or multi-file queue;
- concurrent chunk scheduling;
- interrupted-download resume;
- installed-manifest state;
- install/update/repair logic;
- manifest persistence;
- chunk caching or partial-file persistence;
- Godot/game runtime integration;
- Steam Cloud;
- Workshop.

## Secret handling

Persisted in the device-bound Keychain:

- Steam account name;
- SteamID64;
- Steam refresh token.

Never displayed/logged/persisted by the Step 09 result path:

- Steam password;
- Steam Guard secret/code;
- raw ownership-ticket bytes;
- PICS app access-token value;
- depot decryption-key bytes;
- manifest request-code value;
- CDN auth-token value;
- raw chunk payloads;
- raw manifest body.

Only the final file is persisted, and only after its assembled SHA-1 matches the Steam manifest.

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
- Steps 06–06.3.1 authentication / Guard / persistent saved-session behavior;
- Step 07 exact-AppID, OK, non-empty ownership-ticket proof;
- Step 08 PICS depot/visible-manifest discovery.

See `docs/STEP-09-TEST.md` for the physical-device test.
