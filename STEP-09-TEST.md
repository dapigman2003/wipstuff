# Step 09 physical-iPhone test

## Boundary

Prove exactly one controlled small Slay the Spire 2 file can be retrieved from Steam and verified on a physical iPhone. Do not use this step as a general downloader.

## Build gate

Run the Codemagic workflow:

```text
ios-step-09
```

Required CI results:

```text
Steps 01-05 foundation validation: PASS
Step 09 source validation: PASS
Host unit tests: PASS
Step 09 IPA verification passed.
Version: 0.0.30 (30)
```

## Device procedure

1. Install and launch the Step 09 IPA.
2. Confirm the saved session automatically authenticates. If the device is intentionally signed out, use the existing Authenticate + Save Session flow first.
3. Optionally run **Discover StS2 Depots + Manifests** to reconfirm Step 08 independently.
4. Tap **Download One Small StS2 File** once.
5. Do not terminate the app while that single bounded operation is running.
6. Capture the complete Step 09 result detail.
7. Run **Foundation 5/5 Regression** and capture that result too.

## Required Step 09 success result

The headline must be:

```text
SINGLE-FILE PASS — <filename> (<bytes> bytes)
```

The detail must show all of the following:

```text
Target AppID: 2868840
Saved session found: YES
CM connected: YES
LoggedOnCallback: YES
Logon result: OK
Stored/returned identity match: YES
Ownership callback: YES
Ownership result: OK
Ownership ticket bytes: > 0
Step 07 ownership re-proven: YES
PICS access-token callback: YES
PICS access token received: YES
PICS product-info callback: YES
PICS target app found: YES
PICS reports missing token: NO
Selected depot: <non-zero>
Selected manifest: <non-zero>
Selected branch: public
Depot key requested: YES
Depot key result: OK
Depot key received: YES
Manifest request code requested: YES
Manifest request code received: YES
Eligible CDN servers: > 0
Manifest downloaded: YES
Selected file: <safe relative path>
Selected file bytes: > 0 and <= 2097152
Selected file chunks: > 0
Chunks downloaded: same count as selected file chunks
Downloaded uncompressed bytes: same as selected file bytes
File SHA-1 matches manifest: YES
Final verified file written: YES
Output relative path: Step09-SingleFile/...
```

The detail must also retain these negative-scope checks:

```text
Ownership ticket payload display/logging/persistence: NONE
PICS access-token value display/logging/persistence: NONE
Depot-key value display/logging/persistence: NONE
Manifest request-code value display/logging/persistence: NONE
CDN auth-token value display/logging/persistence: NONE
Manifest body persistence: NONE
Chunk cache/partial-file persistence: NONE
Full-depot queue: NOT IMPLEMENTED
Resume/update/install/repair: NOT IMPLEMENTED
```

A CDN auth token may legitimately say requested/received `YES` or `NO`; it is only requested when a selected CDN returns an authorization challenge. Its value must never be shown.

## Regression gate

After the Step 09 pass, run:

```text
FOUNDATION PASS — 5/5
```

The previously proven sub-results should remain intact, including Core 12/12, real Keychain 7/7, and Steam connection 3/3.

## Failure handling

If Step 09 fails, send the entire visible Step 09 detail and the Codemagic logs. Do not retry with a larger file cap or add multi-file behavior until the exact failing boundary is identified.

## Completion rule

Step 09 is complete only when the physical iPhone proves one SHA-1-verified file was written and the foundation regression remains green. The next boundary is Step 10 — minimal full-depot downloader.
