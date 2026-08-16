# Step 06.2 — Keychain session persistence / relaunch / sign-out

## Scope

Step 06.2 adds exactly one capability on top of the proven Step 06.1 flow: **persist and reuse the minimum Steam session material needed for password-free relaunch**.

SteamKit credential authentication is started with `IsPersistentSession = true`. After the existing mobile Guard flow completes and `SteamUser.LogOn` returns `LoggedOnCallback` with `EResult.OK`, the launcher stores the returned refresh token together with account name and SteamID64 in the iOS Keychain.

It does **not** persist the Steam password, access token, Steam Guard secret/code, or raw protocol data. It does **not** check ownership or download content.

## Device test

### A. Foundation regression

1. Tap **Run Foundation 5/5 Regression**.
2. Expect `FOUNDATION PASS — 5/5`.

### B. Create the saved session

1. Enter the Steam account name/password that already passed Step 06.1.
2. Tap **Authenticate + Save Session**.
3. If Steam sends the mobile Guard notification, approve it in the Steam app and return to the launcher.
4. Wait for the result.

Expected:

```text
STEAM AUTH PASS — Guard approved + session saved
CM connected: YES
Auth session started: YES
Persistent auth requested: YES
Mobile approval requested: YES
Mobile approval completed: YES
LoggedOnCallback: YES
Logon result: OK
Session persisted to Keychain: YES
Account name: <account>
SteamID64: <non-empty value>
Password persistence: NONE
Steam Guard secret/code persistence: NONE
Refresh token display/logging: NONE
Ownership request: NOT RUN
```

The saved-session label should show:

```text
Saved session: YES
Account: <account>
SteamID64: <same SteamID>
Refresh token: PRESENT (not displayed)
```

### C. Prove relaunch persistence

1. Force-close StS2 Launcher completely.
2. Relaunch it.
3. Do **not** enter a Steam password.
4. Verify the saved-session label still reports `Saved session: YES` with the expected account/SteamID.
5. Tap **Resume Saved Session (No Password)**.

Expected:

```text
SAVED SESSION PASS — authenticated
Saved session found: YES
CM connected: YES
LoggedOnCallback: YES
Logon result: OK
Stored/returned identity match: YES
Account name: <same account>
SteamID64: <same SteamID>
Password used: NO
New Steam Guard approval requested by launcher: NO
Refresh token display/logging: NONE
Ownership request: NOT RUN
```

A new Steam Guard notification should not be necessary for this resume path.

### D. Prove sign-out/clear

1. Tap **Sign Out / Clear Saved Session**.
2. Expect status indicating the saved refresh token and identity were removed from Keychain.
3. Force-close and relaunch the app again.
4. Expect:

```text
Saved session: NONE
```

5. Tapping **Resume Saved Session (No Password)** should produce:

```text
SAVED SESSION — none
```

## Send back

The most useful evidence is three screenshots:

1. successful `Authenticate + Save Session` result;
2. successful saved-session resume **after force-close/relaunch**;
3. `Saved session: NONE` after sign-out and another relaunch.

If any stage fails, include the exact `Error:` line.
