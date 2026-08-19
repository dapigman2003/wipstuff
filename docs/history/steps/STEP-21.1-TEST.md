# Step 21.1 — Physical Test

## Build

Codemagic workflow:

`ios-step-21-1`

Expected app:

- `STEP 21.1 — BINDING DIAGNOSTIC EXPORT`
- `Version 0.0.57`

## Preferred test: export the existing physically audited Step 21 plan

1. Install/update Step 21.1 over the existing launcher with the same bundle ID.
2. Launch the app.
3. Tap **Export Complete Step 21 Binding Diagnostics to Files**.
4. Expected result begins with `DIAGNOSTIC EXPORT: PASS` and should report the persisted blocker count.
5. Open iOS **Files**.
6. Navigate to **On My iPhone → StS2 Launcher → StS2Launcher**.
7. Confirm `Step21.1-RuntimeBindingDiagnostics.txt` exists.
8. Share/upload that text file for Step 22 analysis.

Do not edit other launcher files exposed in the Files tree.

## Fallback if the persisted plan did not survive the update

If the export button reports that `runtime-binding-plan.json` is missing:

1. Start from a fresh launcher process if required by the Step 15 Godot process-global rule.
2. Rerun Step 21 Gates A–D once.
3. The app automatically attempts to refresh the diagnostic text report after Gate D.
4. If needed, tap the export button manually afterward.
5. Retrieve the text file from Files as above.

## What to send back

Send the **text file itself**, not screenshots. The report should contain the full `BLOCKERS — COMPLETE (...)` section.

No real StS2 CLR load should be attempted in Step 21.1. `Runtime closure ready: NO` remains a hard stop for that boundary.
