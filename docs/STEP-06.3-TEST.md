# Step 06.3 — automatic saved-session recovery

## Scope

Step 06.3 adds one capability on top of the physically proven Step 06.2 saved session: **automatic launch-time restore with conservative failure recovery**.

It does not add ownership checking, downloads, manual Guard-code entry, or new credential persistence.

## Preferred device test

Use the valid saved session that already passed Step 06.2. Install/upgrade to the Step 06.3 IPA without deleting the app or clearing its Keychain data.

### A. Automatic restore

1. Launch StS2 Launcher.
2. Do not enter a password and do not tap the manual resume button.
3. Wait for the scene to reach `Lifecycle: Active` and for the automatic result to finish.

Expected:

```text
AUTO SESSION PASS — authenticated
Outcome: Authenticated
Recovery action: KeepSavedSession
Saved session found: YES
CM connected: YES
LoggedOnCallback: YES
Logon result: OK
Stored/returned identity match: YES
Automatic launch restore: YES
Saved session cleared by recovery: NO
Password used: NO
New Steam Guard approval requested by launcher: NO
Ownership request: NOT RUN
```

The saved-session label should remain `Saved session: YES`, with the same account and SteamID64. No new Steam Guard notification should be required.

### B. Foundation regression

After automatic restore finishes, tap **Run Foundation 5/5 Regression**.

Expected:

```text
FOUNDATION PASS — 5/5
```

### C. Manual retry remains available

`Retry Saved Session Now (No Password)` remains a diagnostic/retry action. It should still produce `SAVED SESSION PASS — authenticated` when the saved token is healthy.

## Recovery behavior intentionally unit-tested rather than destructively forced on the real account

Step 06.3 has host unit tests for these policies:

- malformed local saved-session record -> clear and require interactive authentication;
- SteamID identity mismatch -> clear and require interactive authentication;
- `InvalidPassword`, `Revoked`, or `Expired` -> clear and require interactive authentication;
- `ServiceUnavailable`, `RateLimitExceeded`, `TryAnotherCM`, timeout, cancellation, and ordinary transport failures -> preserve saved session for retry.

Do not intentionally revoke or corrupt the user's real working Steam token just to prove these branches on-device.

## Send back

One completed screenshot showing **`AUTO SESSION PASS — authenticated`** plus the recovery/action details is the main Step 06.3 proof. A second screenshot of `FOUNDATION PASS — 5/5` is useful as the regression confirmation.
