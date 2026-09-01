# Testing — Step 35.0.16

Active candidate: Step 35.0.16 / `0.0.139 (139)`, IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, workflow `ios-canonical`.

## Build/host prerequisites

1. `bash scripts/validate.sh` must pass.
2. `bash scripts/test.sh` must pass on a host with the pinned .NET SDK. 0.0.138 had at least 209 host tests; 0.0.139 adds managed-command-line and OS-cctor-closure regressions, so the passing total must be **210 or greater** unless tests are deliberately reorganized and documented.
3. iOS build and `scripts/verify-ipa.sh` must pass before physical evidence is accepted.
4. The exact Step-32 transformed source must requalify; protected Step 29–34 manifests must remain unchanged.

The source archive generated outside Codemagic may record that `dotnet` is unavailable. Static validation is not a substitute for the Codemagic host suite.

## Fresh-process rule

Once Step-35 Gate B begins, the process is spent. Every mode must therefore start after force-quit/relaunch. Do not run Step 34 first in the same process. Do not run multiple Step-35 modes in one process.

## Recommended physical order

The highest-value runs for 0.0.139 are:

1. **OS-RECON** — confirms/locates the inner `Godot.OS::.cctor()` boundary with the expanded GodotSharp closure. Expected historical prefix from 0.0.138: `CL_CRITICAL_001_PRE` → `CL_CRITICAL_001_POST` → `CL_CRITICAL_002_PRE` → OS cctor. New GS markers should identify whether termination occurs in StringName/type initialization, `OS.MethodName`, `ClassDB_get_method_with_compatibility`, or a specific NativeFuncs thunk.
2. force-quit/relaunch.
3. **FORWARD** — verifies the managed dictionary + managed empty-args path can pass `CL_CRITICAL_002_POST` and reveals the next startup frontier. High-value success markers include `CL_CRITICAL_002_POST`, `INMETHOD_027`, `NP002_POST`, and any later ExecuteVeryEarly markers.

**NATURAL is optional** for 0.0.139 unless regression confirmation is desired; 0.0.138 already captured the dictionary-native-thunk frontier cleanly.

## Evidence to preserve

For every physical run preserve the matching run-ID set:

- `Step35-CurrentRun.txt`
- `Step35-CrashCheckpoint-<RunId>.txt`
- `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`
- `Step35-GodotNativeReconnaissance-<RunId>.txt`
- `Step35-LastCheckpoint.txt`
- the normal Step-35 result report if one is produced
- any iOS `.ips` crash/termination record with matching time/process if available

Resolver events alone do not establish causation. Interpret them only with same-run PRE/POST/GS markers and static maps.

## Closure rule

All three 0.0.139 modes execute diagnostic derivatives. A 4/4 result means the selected diagnostic derivative survived its measured boundary; it cannot close exact Step 35. Cancellation is INCONCLUSIVE.
