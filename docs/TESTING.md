# Testing — Step 35.0.15

Active candidate: Step 35.0.15 / `0.0.138 (138)` comprehensive GodotSharp/native reconnaissance. IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, workflow `ios-canonical`.

0.0.137 was stopped by Codemagic before iOS build: static validation passed and host tests were 208/209, with the sole failure caused by the GodotSharp entry-marker verifier checking the sts2 bridge type. 0.0.138 must first prove that host regression green before any device run is considered valid.

Running Step 34 and Step 35 in the same process is invalid. Once Gate B begins, the process is spent. **NATURAL and COMPAT must therefore be run as separate fresh-process runs**, but both modes are included in the same 0.0.138 IPA and require no rebuild between them.

## Authority rule

0.0.138 is diagnostic. Gate A must recreate/reverify the exact closed Step-32 transformed artifact and write its same-run exact-source map before CLR admission. Gate B/C execute only separately identified diagnostic derivatives. A 4/4 result is evidence only and cannot be recorded as exact Step-35 closure.

## Required host/static regressions

Run `bash scripts/validate.sh` and `bash scripts/test.sh` on a host with `dotnet`. Coverage must protect:

- exact target tokens/hash/semantic authority and strict Step-35 resolver/native prohibitions;
- ECMA-correct `Action<string>::Invoke(!0)` bridge encoding;
- corrected NP ordinals and four stack-neutral CommandLine critical markers;
- permanent absence of production live-stack CL/CLTV sweeps;
- dual modes: NATURAL preserves the original Godot string dictionary; COMPAT applies exactly four BCL dictionary substitutions and leaves `Godot.OS.GetCmdlineArgs()` natural;
- GodotSharp diagnostic clone preserves identity/MVID, reopens under rejecting resolution, and uses **entry-only** markers including Dictionary ctor, `OS.GetCmdlineArgs`, and `NativeCalls.godot_icall_0_108`;
- serialized entry-marker verification validates the derivative-specific bridge: sts2 markers use `ExecuteVeryEarlyCheckpointBridge`, GodotSharp markers use `GodotSharpCheckpointBridge`;
- GodotSharp source is unchanged after derivative creation;
- read-only reconnaissance recognizes a synthetic arm64 Mach-O without mutating it and emits managed IL, P/Invoke/calli/callback-field, Mach-O dependency/rpath/symbol/string sections.

The Codemagic run is not build-ready unless `step35.trx` reports all tests passed and the host text report ends with `HOST UNIT TESTS: PASS`.

## Physical runs

For each fresh-process mode preserve the normal report plus:

- `Step35-CurrentRun.txt`
- `Step35-CrashCheckpoint-<RunId>.txt`
- `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`
- `Step35-GodotNativeReconnaissance-<RunId>.txt`
- `Step35-LastCheckpoint.txt`

Recommended order: run **NATURAL first** to measure the original 0.0.136 failure with inner GodotSharp entries; force-quit; relaunch and run **COMPAT** to test whether the bounded BCL dictionary substitution reaches the untouched `Godot.OS.GetCmdlineArgs()`/native callback path.

High-value NATURAL outcome: after `CL_CRITICAL_001_PRE`, one or more `INMETHOD_GS...` markers identify the deepest GodotSharp managed method entered before termination. The separate reconnaissance file maps every GS marker to its exact method and provides the local IL/native-call context.

High-value COMPAT sequence: `CL_CRITICAL_001_PRE` → `CL_CRITICAL_001_POST` proves the BCL dictionary path survived; `CL_CRITICAL_002_PRE` without POST localizes the next natural interval to `Godot.OS.GetCmdlineArgs()`; deeper `INMETHOD_GS...` entries can then distinguish `OS`, `GodotObject.GetPtr`, `NativeCalls`, `NativeFuncs`, or related local callees where present in the bounded marker plan.

Cancellation is INCONCLUSIVE. Resolver events alone do not establish causation.
