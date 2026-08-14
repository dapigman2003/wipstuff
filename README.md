# StS2 Launcher iOS — Step 06.1

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 06.1 — Steam Guard mobile approval only**

Steps 01–05 are closed and physically verified. Step 06 also passed its intended boundary: the launcher successfully began a real Steam credential-authentication session and Steam issued a mobile Steam Guard approval notification.

Step 06.1 adds one capability: when Steam chooses `DeviceConfirmation`, the launcher tells SteamKit to keep the same auth session alive and poll while the user approves the sign-in in the Steam mobile app. After approval, the launcher completes the transient Steam logon and displays the authenticated account identity.

Authenticator-code and email-code entry are intentionally not implemented here. Ownership checking is also not implemented.

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
- Step 06 modern credential authentication session

## Step 06.1 Steam Guard policy

- mobile-app approval: **supported**
- SteamKit `AcceptDeviceConfirmationAsync()`: returns `true`, so SteamKit polls the same session until approval
- device/authenticator code: observation only; no code submitted
- email code: observation only; no code submitted
- password/token/guard-data persistence: **none**
- ownership/content request: **none**
- timeout: 3 minutes
- user cancellation: supported

The launcher may be temporarily backgrounded while the user switches to the Steam app. After approving the Steam notification, return to StS2 Launcher and allow the same authentication attempt to complete.

See `docs/STEP-06.1-TEST.md` for the physical-device test.
