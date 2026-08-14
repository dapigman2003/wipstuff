# StS2 Launcher iOS — Step 06.3

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 06.3 — automatic saved-session recovery on launcher startup**

Steps 01–05 are closed and physically verified. Step 06 credential authentication passed. Step 06.1 passed the mobile Steam Guard approval flow. Step 06.2 proved persistent refresh-token storage in the real iOS Keychain, password-free relaunch/resume, matching Steam identity, and explicit sign-out/clear.

Step 06.3 adds one lifecycle capability: when the app reaches its first Active state after launch, it automatically attempts to restore the saved Steam session. The launcher does not ask for or read a password on this path and does not start a new Steam Guard flow.

Ownership checking remains intentionally out of scope until Step 07.

## Recovery policy

The Keychain session is cleared automatically only when there is strong evidence it is unsafe or unusable:

- the local saved-session record cannot be decoded/validated;
- Steam successfully logs on but returns a different SteamID than the stored identity;
- Steam rejects the saved credential with `InvalidPassword`, `Revoked`, or `Expired`.

The saved session is **preserved** for timeout, cancellation, connection failure, and non-definitive/transient Steam results. A temporary Steam/network problem must not destroy a previously working login.

After a recovery clear, the launcher returns to interactive `Authenticate + Save Session`; it never invents credentials or silently starts ownership work.

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

## Security / persistence policy

Persisted in the device-bound Keychain:

- Steam account name
- SteamID64
- Steam refresh token

Never persisted:

- Steam password
- Steam Guard secret/code
- raw Steam protocol payloads

The refresh token is never displayed or logged.

## Runtime note

The Step 06.3 automatic restore is still a verification-style login: after proving the saved session and identity, the current `SteamSessionResumeAttempt` logs off/disconnects cleanly. A later step may introduce a long-lived authenticated Steam session when a downstream capability requires it.

See `docs/STEP-06.3-TEST.md` for the physical-device test.
