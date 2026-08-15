# StS2 Launcher iOS — Step 06.3.1

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 06.3.1 — persistent saved-session semantics fix**

Steps 01–05 are closed and physically verified. Step 06 credential authentication passed. Step 06.1 passed the mobile Steam Guard approval flow. Step 06.2 proved persistent refresh-token storage in the real iOS Keychain, password-free relaunch/resume, matching Steam identity, and explicit sign-out/clear.

Step 06.3 automatic restore passed initially, but repeated real-device retries exposed a persistence defect: the code requested `IsPersistentSession=true` during authentication while token logons still used `ShouldRememberPassword=false`. Step 06.3.1 corrects that mismatch, uses a fresh non-secret Steam `LoginID` per logon attempt to avoid rapid-retry session collisions, removes explicit `SteamUser.LogOff()` from successful verification, and shows only non-secret refresh-token JWT timing metadata.

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

Step 06.3.1 remains a verification-style login and closes the transport after proving the saved session and identity, but it no longer sends an explicit Steam `LogOff` for that successful persistent-session verification. A later downstream step may introduce a long-lived authenticated Steam client when ownership/content work needs it.

See `docs/STEP-06.3.1-TEST.md` for the physical-device test.
