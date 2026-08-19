# Step 23 Physical Test

## Build

Codemagic workflow: `ios-step-23`

Expected app header: `STEP 23 — FIRST REAL STS2 CLR LOAD BOUNDARY`

Expected version: `0.0.65`

## Device procedure

1. Force-quit the launcher first. Do not start the Step 15 Godot host in this process.
2. Open the launcher.
3. Tap `Run Step 23 A–D — Preflight → Load sts2.dll → Resolve Managed Closure → Audit`.
4. Stop at the first failing gate.

### Gate A expectations

- OfflineReady exact tree = YES;
- runtime closure ready = YES;
- explicit blockers = 0;
- prepared private/game assemblies > 0;
- persisted plan exactly covers prepared AssemblyRef metadata = YES;
- module initializers found = **0**;
- prepared/private assemblies already loaded = 0;
- real StS2 CLR load = NO.

If module initializers are nonzero, stop. Send `Reports/Step23-FirstRealGameLoad.txt`; do not broaden the load behavior speculatively.

### Gate B expectations

- `FIRST REAL STS2 CLR LOAD SUCCEEDED`;
- loaded identity begins `sts2, Version=0.1.0.0` for the current depot;
- dedicated context = `StS2Launcher-Step23-Game`;
- game entry point invoked = NO;
- game type/member reflection = NO;
- native load attempts = 0.

### Gate C expectations

- every unique planned managed binding resolves;
- host framework bindings resolve from the default context;
- private prepared bindings resolve from the Step 23 context;
- strict resolver rejected requests = 0;
- native resolution attempts = 0;
- private context assembly count equals the prepared plan count.

### Gate D expectations

- Step 23 = 4/4;
- plan hash unchanged;
- prepared/live bytes unchanged;
- post-load OfflineReady = YES;
- exactly one real `sts2` assembly in the dedicated context;
- native load attempts = 0;
- game entry point/member/method/Godot initialization = NO.

## Closure regressions

After 4/4, run:

- `Verify Offline-Ready Install (Local Only)` → PASS;
- `Run Foundation 5/5 Regression` → PASS.

Do not rerun Step 21/22 in the same process after Gate B; force-quit first because Step 23 intentionally leaves the real game assembly process-resident.
