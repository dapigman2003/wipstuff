# Step 08 physical-iPhone test — depot / manifest discovery only

## Purpose

Prove that the authenticated account which already passed Step 07 can retrieve Steam PICS product metadata for **Slay the Spire 2, App ID 2868840** and enumerate depot IDs plus visible branch manifest IDs without requesting any depot key, manifest body, CDN resource, chunk, or file.

## Preconditions

- Install Step 08 over the existing app if possible so the proven saved Keychain session remains available.
- Step 07 must already have passed for this account.
- If the saved session is absent, use **Authenticate + Save Session** and complete Steam Guard as needed.

## Test order

1. Launch the app and allow automatic saved-session restore to finish.
2. Confirm `AUTO SESSION PASS — authenticated`.
3. Optional but useful: tap **Verify Slay the Spire 2 Ownership** and confirm the Step 07 regression still passes.
4. Tap **Discover StS2 Depots + Manifests**.
5. Wait for the complete Step 08 result.
6. Tap **Run Foundation 5/5 Regression** as the final device regression.

## Required Step 08 pass

The headline must be:

```text
DISCOVERY PASS — <non-zero> depots / <non-zero> manifests
```

The detail should show all of these gates:

```text
Target AppID: 2868840
Saved session found: YES
CM connected: YES
LoggedOnCallback: YES
Logon result: OK
Stored/returned identity match: YES
Step 07 ownership callback: YES
Step 07 ownership result: OK
Step 07 ownership ticket bytes: <non-zero>
Step 07 ownership re-proven: YES
PICS access-token callback: YES
PICS product-info callback: YES
PICS target app found: YES
PICS reports missing token: NO
Depot count: <non-zero>
Visible branch manifest count: <non-zero>
```

Then the app should list one or more entries in this shape:

```text
Depot <id> — oslist=<value>, osarch=<value>, language=<value>
  public: <manifest id>
```

Platform fields are optional in Steam metadata, so a depot may show only its ID. Branch names may include values other than `public`.

The bottom of the result must explicitly remain:

```text
Ownership ticket payload display/logging/persistence: NONE
PICS access-token value display/logging/persistence: NONE
Depot decryption key request: NOT RUN
Manifest body request: NOT RUN
CDN server/token/chunk/file request: NOT RUN
```

## Final regression

After discovery, the existing device regression must still report:

```text
FOUNDATION PASS — 5/5
```

## What to send back

Send:

- the green Codemagic Step 08 build result;
- screenshots of the complete Step 08 discovery result (overlapping screenshots are fine if the depot list is long);
- the final `FOUNDATION PASS — 5/5` result.

Do **not** send Steam credentials, refresh tokens, Guard codes, ownership-ticket payloads, or any access-token value.

## Failure evidence

If Step 08 does not pass, send the complete discovery detail, especially:

- logon / identity gates;
- ownership gates;
- PICS token callback and whether a token was returned (not its value);
- product-info callback;
- target app found / missing-token state;
- PICS change number;
- depot and manifest counts;
- error text and endpoint.

Do not proceed to a manifest-body or file-download step until this metadata-only boundary is proven.
