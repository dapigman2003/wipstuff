# Step 06.1 — Steam Guard mobile approval

## Scope

Step 06.1 adds exactly one capability on top of the working Step 06 credential-auth session: **mobile Steam Guard approval**.

When Steam offers `DeviceConfirmation`, the launcher returns `true` from SteamKit's `IAuthenticator.AcceptDeviceConfirmationAsync()`. SteamKit then continues polling the same authentication session until the user approves the notification in the Steam mobile app. Once Steam returns the auth tokens, the launcher performs the same transient `SteamUser.LogOn` used in Step 06 and displays the authenticated account identity.

It does **not** enter a Steam Guard code, persist tokens, check Slay the Spire 2 ownership, request app/depot metadata, download content, or launch Godot.

## Security behavior

- Username/password are entered at runtime only.
- The password field is cleared from the visible UIKit control when the attempt begins.
- Passwords, refresh tokens, access tokens, and guard data are not written to Keychain, files, logs, build artifacts, or result objects.
- `IsPersistentSession = false`.
- `GuardData = null`.
- Mobile approval does not require the launcher to receive or store a Steam Guard secret/code.
- Authenticator-code and email-code methods remain observation-only and stop without submitting a code.

## Device test

1. Run **Run Foundation 5/5 Regression**. It should remain 5/5.
2. Enter the same Steam account name/password that reached the Step 06 Guard prompt.
3. Tap **Start Step 06.1 Authentication**.
4. Wait until the launcher reports that it is waiting for Steam Guard.
5. Open the Steam mobile app and approve the sign-in notification.
6. Return to StS2 Launcher.
7. Allow the current attempt to finish. Do not start a second authentication attempt unless the first one times out/fails.

## Expected success

```text
STEAM AUTH PASS — Steam Guard approved
CM connected: YES
Auth session started: YES
Mobile approval requested: YES
Mobile approval completed: YES
LoggedOnCallback: YES
Logon result: OK
Account name: <Steam account name>
SteamID64: <non-empty value>
Credential/token/Guard persistence: NONE
Ownership request: NOT RUN
```

The exact endpoint and elapsed time may vary.

## Other valid boundaries

If Steam chooses a code-based method instead of mobile approval, Step 06.1 intentionally reports:

```text
STEAM GUARD REQUIRED — authenticator code
```

or:

```text
STEAM GUARD REQUIRED — email code
```

and submits no code.

If the mobile confirmation is not completed within 3 minutes:

```text
STEAM AUTH TIMEOUT
```

If the user taps Cancel:

```text
STEAM AUTH CANCELLED
```

## Send back

A screenshot of the completed Step 06.1 result. If it fails, include the `Error:` line exactly.
