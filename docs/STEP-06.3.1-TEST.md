# Step 06.3.1 — persistent saved-session fix

## Why this hotfix exists

Step 06.3 automatic launch restore authenticated successfully, but repeated physical-device testing exposed a timing/lifetime problem:

- an immediate manual retry could return `AccessDenied`;
- after a short period, manual or relaunch auto-login could work;
- after several minutes, the saved session could begin returning `AccessDenied`.

Review of the Step 06.3 source found a concrete mismatch: authentication requested a persistent auth session, but token logons used `ShouldRememberPassword=false`.

## Changes under test

Step 06.3.1 changes only authentication-session robustness:

- `IsPersistentSession=true` remains;
- token logons now set `ShouldRememberPassword=true`;
- every token logon uses a fresh non-secret Steam `LoginID`;
- successful verification disconnects without explicitly calling `SteamUser.LogOff()`;
- the UI displays only refresh-token `iat`/`exp` timing metadata, never the token itself;
- `AccessDenied` remains a non-destructive recovery result because it was observed transiently on-device;
- ownership/download scope remains absent.

## Preferred physical-device test

Because the old saved token was created/logged-on under the incorrect semantics, create a fresh session with 06.3.1.

### A. Create a fresh persistent session

1. If a saved session exists, tap **Sign Out / Clear Saved Session**.
2. Enter Steam credentials and tap **Authenticate + Save Session**.
3. Approve mobile Steam Guard if requested.

Expected:

```text
STEAM AUTH PASS — Guard approved + session saved
Persistent auth requested: YES
ShouldRememberPassword: YES
LoginID: <non-zero>
Session persisted to Keychain: YES
Refresh token expired at attempt: NO
Ownership request: NOT RUN
```

### B. Immediate relaunch

Force-close and reopen the launcher.

Expected:

```text
AUTO SESSION PASS — authenticated
ShouldRememberPassword: YES
LoginID: <non-zero>
Stored/returned identity match: YES
Refresh token expired at attempt: NO
Saved session cleared by recovery: NO
```

### C. Manual retry

After the auto result has finished, tap **Retry Saved Session Now (No Password)**.

Expected:

```text
SAVED SESSION PASS — authenticated
Logon result: OK
ShouldRememberPassword: YES
LoginID: <a fresh non-zero value>
Refresh token expired at attempt: NO
```

### D. Persistence over time

Leave the app closed for at least 10 minutes, then relaunch again.

Expected:

```text
AUTO SESSION PASS — authenticated
Logon result: OK
Refresh token expired at attempt: NO
```

The saved-session label should show a refresh-token expiration time in UTC. The raw token must never be displayed.

### E. Foundation regression

Run **Foundation 5/5 Regression** once.

Expected:

```text
FOUNDATION PASS — 5/5
```

## If a retry still returns AccessDenied

Do not clear the token automatically. Send the screenshot showing:

- `Logon result`
- `Extended result`
- `LoginID`
- `Refresh token expires (UTC)`
- `Refresh token expired at attempt`

That will distinguish a still-valid persistent JWT from a genuinely expired credential or another Steam session-policy issue.

## Scope

No ownership, PICS, depot, CDN, download, Godot, or game-content work is included.
