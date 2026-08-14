# Step 06 — Steam authentication session

## Scope

Step 06 adds exactly one new runtime boundary on top of the completed Steps 01–05 foundation: a modern SteamKit credential authentication session.

It does **not** check Slay the Spire 2 ownership, request app/depot metadata, download content, launch Godot, persist tokens, or handle Steam Guard codes/approvals.

## Security behavior

- Username/password are entered at runtime only.
- The password field is cleared from the visible UIKit control when the attempt begins.
- Passwords, refresh tokens, access tokens, and guard data are not written to Keychain, files, logs, build artifacts, or result objects.
- `IsPersistentSession = false`.
- `GuardData = null`.
- If Steam Guard is required, Step 06 stops before submitting a code or accepting a mobile confirmation.

## Device test

1. Optionally run **Run Foundation 5/5 Regression** first. It must remain 5/5.
2. Enter the Steam account name and password.
3. Tap **Start Step 06 Authentication**.
4. Keep the app in the foreground until a result appears.

### Direct success (account does not require a guard challenge)

Expected:

```text
STEAM AUTH PASS — authenticated
CM connected: YES
Auth session started: YES
LoggedOnCallback: YES
Logon result: OK
Account name: <Steam account name>
SteamID64: <non-empty value>
Credential persistence: NONE
Ownership request: NOT RUN
```

### Expected guarded-account boundary

If Steam requires Steam Guard, Step 06 should report one of:

```text
STEAM GUARD REQUIRED — mobile-app confirmation
STEAM GUARD REQUIRED — authenticator code
STEAM GUARD REQUIRED — email code
```

Also expected:

```text
CM connected: YES
Auth session started: YES
LoggedOnCallback: NO
Credential persistence: NONE
Ownership request: NOT RUN
```

This is useful Step 06 evidence, not a reason to broaden Step 06. The exact challenge becomes Step 06.1.

## Send back

A screenshot of the full Step 06 result. If it fails, include any `Error:` line exactly.
