# Step 10 physical-iPhone test

## Boundary

Prove the launcher can download one complete selected direct public Slay the Spire 2 depot on a physical iPhone using a file queue, progress, cancellation-safe staging, per-file SHA-1 validation, and one atomic final-directory commit.

This is **one depot**, not yet a complete multi-depot app installation.

## Before running

A full depot can be much larger than the Step 09 test file. Ensure the iPhone has adequate free storage and use a stable network connection. The UI exposes **Cancel Current Steam Operation** while Step 10 runs.

## Build gate

Run the Codemagic workflow:

```text
ios-step-10
```

Required CI results:

```text
Steps 01-05 foundation validation: PASS
Step 10 source validation: PASS
Host unit tests: PASS
Step 10 IPA verification passed.
Version: 0.0.31 (31)
```

## Device procedure — cancellation + completion path

1. Install and launch the Step 10 IPA.
2. Confirm the saved session automatically authenticates. If intentionally signed out, use **Authenticate + Save Session** first.
3. Tap **Download One Full Public Depot**.
4. Wait until the live file/chunk/byte progress is non-zero, then tap **Cancel Current Steam Operation**.
5. Capture the cancellation result and require:

```text
DEPOT CANCELLED — no final commit
Final directory atomically committed: NO
Staging directory absent after result: YES
```

6. Without clearing the app, tap **Download One Full Public Depot** again. Because the cancelled run committed no final directory, this second run should be allowed to start from zero.
7. Let the second run complete without cancellation. Keep the app active for this first full-depot proof.
8. Capture the complete Step 10 success detail.
9. Run **Foundation 5/5 Regression** and capture that result too.

This order proves cancellation/cleanup first and then atomic completion without requiring a new manifest or manual output deletion.

## Required Step 10 completion result

The headline must be:

```text
DEPOT PASS — <completed>/<planned> files (<bytes> bytes)
```

The detail must show:

```text
Target AppID: 2868840
Saved session found: YES
CM connected: YES
LoggedOnCallback: YES
Logon result: OK
Stored/returned identity match: YES
Step 07 ownership re-proven: YES
Step 08 PICS access-token callback: YES
Step 08 PICS product-info callback: YES
Step 08 target app found: YES
Selected depot: <non-zero>
Selected branch: public
Selected manifest ID: <non-zero>
Depot key requested/received: YES / YES
Manifest request code requested/received: YES / YES
Eligible CDN servers: > 0
Manifest downloaded: YES
Queued files: > 0
Queued chunks: > 0
Queued uncompressed bytes: > 0
Completed files: same as queued files
SHA-1 verified files: same as queued files
Downloaded chunks: same as queued chunks
Downloaded uncompressed bytes: same as queued uncompressed bytes
Staging directory created: YES
Staging directory absent after result: YES
Final directory atomically committed: YES
Output relative path: Step10-FullDepot/<depot>/<manifest>
```

A CDN auth token may legitimately be requested/received `YES` or `NO`; it is only needed after an authorization challenge. Its value must never be displayed.

The detail must retain these scope/security checks:

```text
Ownership ticket payload display/logging/persistence: NONE
PICS access-token value display/logging/persistence: NONE
Depot-key value display/logging/persistence: NONE
Manifest request-code value display/logging/persistence: NONE
CDN auth-token value display/logging/persistence: NONE
Manifest body persistence: NONE
Chunk cache outside staging: NONE
Partial final-depot visibility: NONE — final directory appears only after atomic staging rename
Resume: NOT IMPLEMENTED
Update/install/repair orchestration: NOT IMPLEMENTED
Multi-depot app install: NOT IMPLEMENTED
```

## Cancellation rule

The cancellation run is part of the formal Step 10 completion gate, not optional. Cancel only after progress is non-zero so the test proves cleanup of an actually-created staging tree.

## Regression gate

After the Step 10 completion pass, run:

```text
FOUNDATION PASS — 5/5
```

The proven Core 12/12, real Keychain 7/7, Steam connection 3/3, and earlier authentication/ownership/content boundaries must remain intact.

## Failure handling

If Step 10 fails, capture the entire Step 10 detail and Codemagic logs. Do not add resume/update/repair logic to mask a failure in the basic full-depot queue.

Important failures to report exactly include:

- `DEPOT FAIL — manifest is unsafe/unsupported`;
- `DEPOT FAIL — file/chunk download failed`;
- `DEPOT FAIL — file SHA-1 mismatch`;
- `DEPOT FAIL — staging write failed`;
- `DEPOT FAIL — atomic directory commit failed`;
- staging cleanup reporting `NO`.

## Completion rule

Step 10 is complete when the physical iPhone proves one selected public depot completed with all queued files SHA-1 verified, the final directory was atomically committed, no partial staging data remained, and the foundation regression stayed green.

The next boundary is **Step 11 — interrupted-download resume**.
