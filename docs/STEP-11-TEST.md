# Step 11 physical-iPhone test — interrupted-download resume

## Purpose

Prove that a real interrupted Step 11 depot transfer can survive process termination and continue without re-downloading data that still matches Steam's manifest.

## Build gate

Run Codemagic workflow:

```text
ios-step-11
```

Require the build to finish green and include:

```text
Step 11 source validation: PASS
Step 11 IPA verification passed.
Version: 0.0.32 (32)
```

## Device test — required interruption

1. Install/launch version `0.0.32`.
2. Confirm automatic saved-session authentication still succeeds, or authenticate once if needed.
3. Tap **Resume / Download One Public Depot**.
4. Wait until the live Step 11 progress shows **Chunks satisfied > 0** and **Bytes satisfied > 0**.
5. **Force-quit the launcher from the iOS app switcher.** Do not use the in-app cancel button for the primary Step 11 proof; Step 10 already proved controlled cancellation.
6. Relaunch the app.
7. Wait for the saved-session regression to authenticate again.
8. Tap **Resume / Download One Public Depot** again.
9. Let it finish.

## Required Step 11 success result

The second run must end with:

```text
RESUME PASS — <completed>/<planned> files; reused <non-zero> bytes
```

The details must show all of the following:

```text
Target AppID: 2868840
Saved session found: YES
CM connected: YES
LoggedOnCallback: YES
Logon result: OK
Stored/returned identity match: YES
Step 07 ownership re-proven: YES
Step 08 target app found: YES

Resume staging found at start: YES
Reused Adler-32-valid chunks: <greater than zero>
Reused bytes: <greater than zero>
New chunks downloaded this run: <less than planned chunks>
Satisfied chunks after resume/download: <planned>/<planned>
Satisfied bytes after resume/download: <planned>/<planned>
Completed files: <planned>/<planned>
SHA-1 verified files: <planned>/<planned>
Final directory atomically committed: YES
```

`Reused fully SHA-1-verified files` may be zero if the force-quit happened during the first file. That is acceptable as long as **reused chunks and reused bytes are non-zero**. If one or more complete files had already finished before the force-quit, those files should also be reused after full SHA-1 verification.

The final detail must continue to state that Steam secret values are not displayed/persisted and that update/install/repair, manifest-delta migration and multi-depot installation are not implemented.

## Regression gate

After Step 11 succeeds, tap:

```text
Run Foundation 5/5 Regression
```

Require:

```text
FOUNDATION PASS — 5/5
CORE SELF-TEST PASS — 12/12
CREDENTIAL STORE PASS — 7/7
STEAM CONNECTION PASS — 3/3
```

## Completion rule

Step 11 is complete only when the physical iPhone proves a process-interrupted transfer was discovered on relaunch, reused non-zero checksum-valid bytes/chunks, downloaded the remaining data, SHA-1 verified every file, atomically committed the final directory, and kept the foundation regression green.

Next boundary after Step 11: **Step 12 — install/update/repair manager**. Do not add it until this test passes.
