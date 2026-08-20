# Step 23 Physical Test — Current 23.4 Boundary

## Build

Codemagic workflow: `ios-step-23-4`

Expected app header: `STEP 23.4 — FIRST REAL STS2 CLR LOAD BOUNDARY`

Expected version: `0.0.69`

## Device procedure

1. Force-quit the launcher first. Do not start the Step 15 Godot host in this process.
2. Open the launcher.
3. Tap `Run Step 23 A–D — Preflight → Load sts2.dll → Resolve Managed Closure → Audit`.
4. Stop at the first failing gate.

### Gate A expectations

- OfflineReady exact tree = YES;
- runtime closure ready = YES;
- explicit blockers = 0;
- persisted plan exactly covers prepared Cecil `AssemblyRef` metadata = YES;
- primary `sts2.dll` module initializers = **0**;
- initializer-bearing dependency count may be nonzero and is explicitly deferred;
- deferred initializer IL audit is written to `Reports/Step23-FirstRealGameLoad.txt`;
- no prepared private/game assembly is already loaded;
- real StS2 CLR load = NO.

A module initializer on the primary remains a hard stop. A module initializer on a dependency is not loaded in Step 23 and belongs to the Step 24 initialization boundary.

### Gate B expectations

- `FIRST REAL STS2 CLR LOAD SUCCEEDED`;
- loaded identity begins `sts2, Version=0.1.0.0` for the current depot;
- dedicated context = `StS2Launcher-Step23-Game`;
- deferred initializer-bearing dependencies loaded = 0;
- game entry point invoked = NO;
- game type/member reflection = NO;
- native load attempts = 0.

### Gate C expectations

- all planned host framework bindings resolve from the default context;
- all initializer-free private bindings resolve from the Step 23 context;
- initializer-bearing private requirements are counted as deferred and not loaded;
- strict resolver rejected/unplanned requests = 0;
- deferred-initializer resolver requests = 0;
- native resolution attempts = 0;
- private context membership exactly equals the initializer-free prepared set.

### Gate D expectations

- Step 23 = 4/4;
- plan hash unchanged;
- every prepared/live byte set unchanged;
- post-load OfflineReady = YES;
- exactly one real `sts2` assembly in the dedicated context;
- initializer-bearing prepared dependencies loaded = 0 / deferred count;
- native load attempts = 0;
- game entry point/member/method/Godot initialization = NO.

## Closure regressions

After 4/4, run:

- `Verify Offline-Ready Install (Local Only)` → PASS;
- `Run Foundation 5/5 Regression` → PASS.

Do not rerun Step 21/22 in the same process after Gate B; force-quit first because Step 23 intentionally leaves the real game assembly process-resident.
