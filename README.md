# StS2 Launcher iOS — Step 06

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 06 — Steam authentication session only**

Steps 01–05 are closed and physically verified. This source retains the proven iOS SteamKit compatibility work and adds a single new capability: begin modern Steam credential authentication and, when no Steam Guard challenge blocks the flow, log on and display the authenticated Steam account identity.

If Steam Guard is required, Step 06 reports the exact challenge type and stops without supplying a code or accepting a mobile confirmation. That interaction is Step 06.1.

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

## Step 06 credential policy

- runtime entry only
- no password/token/guard-data persistence
- no credentials in source or CI
- no raw auth payload logging
- no ownership or content request

See `docs/STEP-06-TEST.md` for the physical-device test.
