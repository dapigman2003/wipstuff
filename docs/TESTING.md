# Testing — Step 35.0.19

Active candidate: Step 35.0.19 / `0.0.142 (142)`, IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, workflow `ios-canonical`.

## Build/host prerequisites

1. `bash scripts/validate.sh` must pass.
2. `bash scripts/test.sh` must pass on a host with the pinned .NET SDK. 0.0.141 executed **211** host tests: **210 passed / 1 failed** solely because the invalid-table regression expected zero checkpoints instead of the intentional one `CB_INITIALIZE_MANAGED_FAIL`. 0.0.142 corrects that assertion without changing runtime behavior, so expect **211 or greater** and require all to pass unless tests are deliberately reorganized and documented.
3. Codemagic must successfully rebuild the pinned Godot 4.5.1 iOS static archive with `module_mono_enabled=yes`, pass the standalone native-link preflight, then build and verify the IPA.
4. The exact Step-32 transformed source must requalify; protected Step 29–34 manifests must remain unchanged.

The source archive generated outside Codemagic may record that `dotnet` is unavailable. Static validation is not a substitute for the Codemagic host suite or the macOS/iOS native link.

## Process rules

NATURAL / OS-RECON / FORWARD retain the existing rule: use a fresh process with no Step-15 Godot session and no prior Step-35 Gate B.

CORE-HANDOFF intentionally uses a different sequence because its purpose is to test the legitimate Godot core callback state:

1. Force-quit/relaunch the launcher.
2. Run the existing **Step 15 Gates A–C** until the embedded smoke engine is started, setup-complete, and its normal Step-15 rendering/touch readiness is established.
3. **Do not force-quit.** Return directly to Step 35.
4. Run **CORE-HANDOFF** exactly once.

Do not run another Step-35 mode in that process after Gate B begins.

## CORE-HANDOFF expected evidence

Before the handoff the journal should show a readiness line equivalent to `CB_NATIVE_READY_RECHECK` with engine/setup/interop ready, `dotnetFeature=False`, and `godotDotNetInitialized=False`.

High-value progression is:

`CB_NATIVE_TABLE_REQUEST_START` → `CB_NATIVE_TABLE_REQUEST_RETURNED` with a nonzero pointer and positive pointer-aligned size → `CB_INIT_ENTRY` → private GodotSharp load/identity pass → `CB_NATIVEFUNCS_BIND_PASS` → `CB_INITIALIZE_INVOKE_START` → **GS025 `NativeFuncs.Initialize`** → `CB_INITIALIZE_INVOKE_RETURNED` → `CB_INITIALIZE_PASS` → Gate C natural path.

Do not hard-code an expected callback-table byte count in interpretation; accept the size reported by the exact source-built engine and verified by the managed receiver.

The key physical question is whether the natural dictionary path now advances beyond the old GS031 frontier and whether subsequent StringName/OS/DirAccess calls advance beyond GS024. A later new frontier is useful evidence; a managed 4/4 is still diagnostic only.

## Evidence to preserve

For every physical run preserve the matching run-ID set:

- `Step35-CurrentRun.txt`
- `Step35-CrashCheckpoint-<RunId>.txt`
- `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`
- `Step35-GodotNativeReconnaissance-<RunId>.txt`
- `Step35-LastCheckpoint.txt`
- the normal Step-35 result report if produced
- any iOS `.ips` crash/termination record with matching time/process if available

Resolver events alone do not establish causation. Interpret them with same-run PRE/POST/GS/CB markers and static maps.

## Closure rule

All four 0.0.142 modes execute diagnostic derivatives. A 4/4 result cannot close exact Step 35. Cancellation is INCONCLUSIVE.
