# Step 12.3 physical-iPhone completion record

This gate was completed successfully on the physical iPhone with Step 12.3 / `0.0.37 (37)`, closing Step 12. Step 12.4 / `0.0.38` was subsequently exercised as the stabilization baseline. Step 12.4.1 / `0.0.39` adds only cache-maintenance/fresh-download regression controls; see `STEP-12.4.1-CACHE-TEST.md`.

Step 12.1's source-generated receipt JSON and Step 12.2/12.2.1's bounded iOS CDN timeout failover/catch-order fix remain enabled. Step 12.3 adds independently verified current-manifest cache reuse, better source-progress/cancellation telemetry, and a stronger deterministic update capability test.

## Gate A — Install + no-op

Tap **Inspect + Install / Update / Repair** with no existing Step 12 managed install.

Require:

```text
INSTALL PASS — <files> files (<bytes> bytes)
State before: NotInstalled
Action taken: Install
State after: UpToDate
Previous install preserved until commit: YES
Atomic commit completed: YES
Staging absent after result: YES
Backup absent after result: YES
```

Run the manager again without changing anything and require:

```text
INSTALL MANAGER PASS — up to date
State before: UpToDate
Action taken: None
State after: UpToDate
```

## Gate B — Repair

Tap **Prepare Repair Test (Corrupt One Managed File)**, then run the manager.

Require:

```text
REPAIR PASS — <non-zero> files restored
State before: RepairNeeded
Action taken: Repair
State after: UpToDate
Atomic commit completed: YES
```

## Gate C — Update capability with verified existing source

Tap **Prepare Update Test (Stale Receipt + One Changed File Identity)**. This modifies only the project-owned receipt: the manifest ID becomes stale and one smallest non-empty file identity gets a synthetic different SHA-1. Actual managed game bytes are not modified.

Run **Inspect + Install / Update / Repair**.

Require:

```text
UPDATE PASS — manifest <current Steam manifest>
State before: UpdateAvailable
Action taken: Update
State after: UpToDate
Installed manifest after: <same as current public manifest>
Source cache reverified against current Steam manifest: YES
Source bytes downloaded this manager run: 0
Replaced files/bytes: <at least 1 file> / <positive bytes>
Atomic commit completed: YES
```

This gate proves the actual update manager path rather than only rewriting a receipt: Steam's real current manifest is rediscovered; the existing Step 11 final cache must independently pass exact current-manifest path/size/SHA-1 verification; at least one synthetic changed-file identity must be supplied from that verified source; the complete replacement must be verified; and the current receipt must become live only through the atomic commit.

If Steam has published a genuinely new manifest since the prior source cache was created, `Source bytes downloaded this manager run` may legitimately be non-zero because the new manifest-specific Step 11 source does not yet exist. In that case the update capability is still valid if the normal source acquisition, staging, verification, atomic commit, and final `UpToDate` gates pass.

## Gate D — cancellation telemetry (quick regression)

Only if you want to verify the UI fix: start a manager run that enters source acquisition and cancel after the current manifest/plan is visible. The final result should retain non-zero `Planned files` / `Planned bytes` once the plan had already been obtained. Do not use this as a substitute for Gates A–C.

## Gate E — foundation regressions

Run **Foundation 5/5 Regression** and require `FOUNDATION PASS — 5/5`.

**Completion status: PASS.** Gates A, B, C, and E were reported successful on the physical iPhone, so Step 12 is closed. Gate D remained optional telemetry coverage. Step 12.4 and Step 12.4.1 do not reopen the capability boundary; they are stabilization/test-maintenance releases only.
