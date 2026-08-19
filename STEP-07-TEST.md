# Step 07 physical-iPhone test — ownership only

## Purpose

Prove that the authenticated Steam account can obtain an ownership ticket for **Slay the Spire 2, App ID 2868840**, without beginning any content/download work.

## Preconditions

- Install Step 07 over the existing app if possible so the proven saved Keychain session remains available.
- If there is no saved session, use **Authenticate + Save Session** first and complete Steam Guard as needed.

## Test order

1. Launch the app and allow the automatic saved-session regression to finish.
2. Confirm it reports `AUTO SESSION PASS — authenticated`.
3. Tap **Verify Slay the Spire 2 Ownership**.
4. Wait for the Step 07 result.
5. Run **Foundation 5/5 Regression** once as the final regression check.

## Ideal Step 07 result

```text
OWNERSHIP PASS — App 2868840 owned
Target AppID: 2868840
Saved session found: YES
CM connected: YES
LoggedOnCallback: YES
Logon result: OK
Stored/returned identity match: YES
Ownership callback: YES
Ownership result: OK
Ownership callback AppID: 2868840
Ownership ticket bytes: <non-zero>
Ownership proven: YES
Ownership ticket payload display/logging/persistence: NONE
PICS request: NOT RUN
Depot/manifest/CDN/download request: NOT RUN
```

The ticket byte count is expected to vary. Only **non-zero** matters.

## If it does not pass

Send the complete ownership section, especially:

- logon result / extended result;
- identity match;
- ownership callback YES/NO;
- ownership result;
- ownership callback AppID;
- ownership ticket byte count;
- error text;
- current endpoint.

Do not proceed to depot/manifest work until ownership is proven.
