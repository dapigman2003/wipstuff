# StS2 Launcher iOS — Step 08

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 08 — discover depot IDs and visible branch manifest IDs for Steam App ID 2868840 only.**

Steps 01–07 are treated as closed regressions. Step 07 proved ownership of Slay the Spire 2 on the physical iPhone. Step 08 adds only Steam PICS metadata discovery after that same ownership gate is re-proven.

The new operation:

1. restores the saved Keychain Steam session;
2. requires matching returned SteamID;
3. re-runs `GetAppOwnershipTicket(2868840)` and requires the proven Step 07 success contract;
4. requests the PICS app access token for App ID `2868840`;
5. requests PICS product info for App ID `2868840` only;
6. parses the returned `depots` metadata;
7. displays numeric depot IDs, platform metadata such as `oslist` / `osarch` / `language`, and already-visible branch manifest IDs.

The PICS access-token value itself is never displayed, logged, or persisted.

## Explicitly not included

Step 08 does **not** add:

- depot decryption-key requests;
- depot manifest-body requests;
- CDN server discovery/authentication;
- chunk requests;
- file download or writes;
- install/update logic;
- Godot/game runtime integration.

Manifest IDs shown by Step 08 are metadata identifiers only. The corresponding manifest contents are not requested.

## Proven foundation retained

- UIKit launch/lifecycle
- Core self-test 12/12
- real iOS Keychain regression 7/7
- SteamKit2 3.4.0
- WebSocket-only CM connection
- `SocketsHttpHandler` for `HttpClientPurpose.CMWebSocket`
- `TrimMode=full` with `SteamKit2`, `protobuf-net`, `protobuf-net.Core` trim roots
- generated `DiskArbitration` framework removal
- isolated/version-aware `Process.StartTime` SteamKit iOS compatibility patch
- Steps 06–06.3.1 authentication / Guard / persistent saved-session behavior
- Step 07 exact-AppID, OK, non-empty ownership-ticket proof

## Security / scope

Persisted in the device-bound Keychain:

- Steam account name
- SteamID64
- Steam refresh token

Never persisted by Step 08:

- Steam password
- Steam Guard secret/code
- raw ownership-ticket bytes
- PICS app access-token value
- raw Steam protocol payloads
- game/depot files

See `docs/STEP-08-TEST.md` for the physical-device test.
