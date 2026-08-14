# StS2 Launcher iOS — Step 06.2

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 06.2 — persist the minimum reusable Steam session in iOS Keychain, prove relaunch/resume, and prove sign-out clears it**

Steps 01–05 are closed and physically verified. Step 06 reached Steam's real authentication flow. Step 06.1 then completed the ideal mobile Steam Guard path and returned an authenticated account identity.

Step 06.2 adds one capability: after that already-proven authentication flow succeeds, request a persistent Steam auth session and save the returned refresh token plus account identity metadata in the device-bound iOS Keychain. A later app launch can use that token to log on without re-entering the password or starting a new Steam Guard approval.

Ownership checking is intentionally not implemented.

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

## Step 06.2 persistence policy

Persisted in iOS Keychain only after a successful `LoggedOnCallback` with `EResult.OK`:

- Steam account name
- SteamID64
- Steam refresh token

Not persisted:

- Steam password
- access token
- Steam Guard secret
- Steam Guard authenticator/email code
- raw Steam protocol payloads

The Keychain record uses `SecAccessible.AfterFirstUnlockThisDeviceOnly`, so it is device-bound and available for normal relaunch/resume after the device has been unlocked once after boot.

The session payload is a small versioned format rather than JSON so Step 06.2 does not introduce another reflection/trimming boundary.

## Saved-session resume

The **Resume Saved Session (No Password)** action:

1. loads the saved session from Keychain;
2. connects over the Step 05-proven WebSocket path;
3. calls `SteamUser.LogOn` with the saved refresh token;
4. requires `LoggedOnCallback` with `EResult.OK`;
5. verifies the returned SteamID64 matches the stored identity;
6. displays no refresh token and requests no password/Guard code.

The verification attempt logs off/disconnects after proving the boundary; Step 06.2 does not yet keep a long-lived Steam client session for launcher operation.

## Sign out

**Sign Out / Clear Saved Session** deletes the Keychain session and verifies it is absent. A subsequent app relaunch should show `Saved session: NONE`.

See `docs/STEP-06.2-TEST.md` for the physical-device test.
