# Step 12 / 12.1 physical-iPhone completion gate

Use Codemagic workflow `ios-step-12-2-1`, install version `0.0.36 (36)`, and keep the same legitimate saved Steam session used by the previous steps.

Step 12.2.1 is the current compile-hotfix build; Step 12.2 runtime behavior is retained. Step 12.1 fixed the AOT receipt-JSON failure from `0.0.33`; the next device run then exposed an iOS `TimeoutException: The request timed out.` while the Step 11 resumable source path was materializing `Slay the Spire 2.pck`. Step 12.2 keeps the same manager gates and adds only bounded CDN-server failover for that iOS timeout shape. The pass criteria below are otherwise unchanged.

## Gate A — Install

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

Run the manager once more without changing anything and require:

```text
INSTALL MANAGER PASS — up to date
State before: UpToDate
Action taken: None
State after: UpToDate
```

## Gate B — Repair

Tap **Prepare Repair Test (Corrupt One Managed File)**, then run the manager again.

Require:

```text
REPAIR PASS — <non-zero> files restored
State before: RepairNeeded
Action taken: Repair
State after: UpToDate
Atomic commit completed: YES
```

## Gate C — Update-state branch

Tap **Prepare Update-State Test (Stale Local Receipt Only)**, then run the manager again.

Require:

```text
UPDATE PASS — manifest <current Steam manifest>
State before: UpdateAvailable
Action taken: Update
State after: UpToDate
Installed manifest after: <same as current public manifest>
```

This test changes only the project-owned receipt. It does not invent a Steam manifest. The manager must rediscover the real current public manifest and restore a correct receipt.

## Gate D — regressions

Run **Foundation 5/5 Regression** and require `FOUNDATION PASS — 5/5`.

Step 12/12.1/12.2/12.2.1 is complete only after Codemagic passes and Gates A–D pass on the physical iPhone.
