# StS2 Launcher iOS — Step 07

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 07 — verify ownership of Slay the Spire 2 (Steam App ID 2868840) only.**

Steps 01–05 are closed and physically verified. Steps 06 through 06.3.1 are also physically proven: real credential authentication, mobile Steam Guard approval, device-bound Keychain refresh-token persistence, password-free relaunch/resume, sign-out, automatic restore, and corrected persistent-session semantics.

Step 07 adds one network capability after a saved-session logon with matching identity:

1. obtain the existing `SteamApps` handler;
2. call `GetAppOwnershipTicket(2868840)`;
3. wait for `AppOwnershipTicketCallback`;
4. count ownership as proven only when the callback is for App ID `2868840`, `Result == EResult.OK`, and the returned ticket is non-empty.

The ownership ticket payload itself is never displayed, logged, persisted, or passed into another subsystem. Only its byte length is shown as diagnostic evidence.

## Explicitly not included

Step 07 does **not** add:

- PICS app/package product-info requests;
- depot discovery;
- depot keys;
- manifest discovery;
- CDN access;
- file download;
- ownership-ticket parsing/decryption;
- Godot/game runtime integration.

A non-OK ticket response is reported as **ownership not proven**, not guessed to mean a specific license state.

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
- Step 06 credential authentication
- Step 06.1 mobile Steam Guard approval
- Step 06.2 refresh-token Keychain persistence/resume/sign-out
- Step 06.3.1 persistent-session correction (`ShouldRememberPassword=true`, fresh `LoginID`)

## Security

Persisted in the device-bound Keychain:

- Steam account name
- SteamID64
- Steam refresh token

Never persisted:

- Steam password
- Steam Guard secret/code
- raw Steam protocol payloads
- Step 07 ownership-ticket bytes

See `docs/STEP-07-TEST.md` for the physical-device test.
