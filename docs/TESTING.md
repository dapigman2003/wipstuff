# Testing — Step 23 First Real StS2 CLR Load Boundary

## Current principle

Text files are the primary handoff format for long diagnostics. Screenshots are useful for short UI state only.

## Static validation

```sh
bash scripts/validate.sh
```

Output:

`artifacts/reports/static-validation.txt`

The validator checks only authoritative current source/docs/tooling. It must **not** depend on `history.zip` or any legacy `StS2Launcher.Step05.iOS` path.

## Host unit tests

```sh
bash scripts/test.sh
```

Output:

`artifacts/reports/host-unit-tests.txt`

Detailed test results remain under `artifacts/test-results/`.

Step 23 host tests use synthetic project-owned IL and a collectible load context so the test process can return to a clean state. The production Step 23 context is intentionally non-collectible/process-resident.

## Godot native preflight

On macOS/Xcode:

```sh
bash scripts/build-godot.sh
bash scripts/preflight-godot-link.sh
```

Shareable preflight output:

`artifacts/reports/godot-native-preflight.txt`

## iOS build

On macOS/Xcode with the pinned .NET SDK/workload available:

```sh
bash scripts/build-ios.sh
```

Expected IPA:

`artifacts/StS2-Launcher-Step-23.3.ipa`

## IPA verification

```sh
bash scripts/verify-ipa.sh artifacts/StS2-Launcher-Step-23.3.ipa
```

Output:

`artifacts/reports/ipa-verification.txt`

## Codemagic

Workflow:

`ios-step-23-3`

Authoritative entry point:

```sh
bash scripts/codemagic.sh
```

The pipeline runs static validation, host tests, iOS workload setup, Godot build/preflight, iOS publish, and final IPA verification. Build/CI never contains or loads the proprietary game payload; the Step 23 real load occurs only from the user's receipt-backed on-device install.

## Physical acceptance for Step 23

Install version `0.0.68` and start from a fresh process. Do not start the Step 15 Godot host first.

Run Step 23 A–D and require:

1. Gate A = PASS;
   - OfflineReady exact-tree = YES;
   - runtime closure ready = YES;
   - explicit blockers = 0;
   - module initializers = 0;
   - prepared/live SHA-1s match;
   - persisted plan exactly covers prepared AssemblyRef metadata;
   - no prepared private/game assembly was already loaded;
2. Gate B = PASS;
   - first real `sts2.dll` CLR load succeeds;
   - exact `sts2` identity and dedicated `StS2Launcher-Step23-Game` context;
   - no entry-point/member/method invocation;
   - no native resolution;
3. Gate C = PASS;
   - all unique planned managed identities resolve;
   - host framework requirements resolve from `AssemblyLoadContext.Default`;
   - private requirements resolve only from the exact prepared set;
   - rejected/unplanned requests = 0;
   - native load attempts = 0;
4. Gate D = PASS;
   - plan/prepared/live bytes unchanged;
   - post-load OfflineReady = YES;
   - private context membership exactly matches the plan;
   - no native/game initialization occurred;
5. Step 23 summary = 4/4;
6. OfflineReady regression = PASS;
7. Foundation 5/5 regression = PASS.

Share `Reports/Step23-FirstRealGameLoad.txt` on any failure. After Gate B, force-quit before rerunning Step 21/22 pre-load gates.
