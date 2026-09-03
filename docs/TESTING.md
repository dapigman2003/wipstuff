# Testing — Step 35.0.23

Active candidate: Step 35.0.23 / `0.0.146 (146)`, IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, workflow `ios-canonical`.

## Build/host prerequisites

1. `bash scripts/validate.sh` must pass.
2. `bash scripts/test.sh` must pass on a host with the pinned .NET SDK. 0.0.142 proved **211/211 host tests PASS** after the callback-telemetry correction; 0.0.146 adds managed-plugin bootstrap reflection/native-bridge guards, so require every current host test to pass rather than assuming a fixed count.
3. Codemagic must successfully rebuild the pinned Godot 4.5.1 iOS static archive with `module_mono_enabled=yes`, pass the standalone native-link preflight including the new managed-callback adoption exports, then build and verify the IPA.
4. The exact Step-32 transformed source must requalify; protected Step 29–34 manifests must remain unchanged.

The source archive generated outside Codemagic may record that `dotnet` is unavailable. Static validation is not a substitute for the Codemagic host suite or the macOS/iOS native link.

## Process rules

NATURAL / OS-RECON / FORWARD retain the existing rule: use a fresh process with no Step-15 Godot session and no prior Step-35 Gate B.

CORE-HANDOFF intentionally uses a different sequence:

1. Force-quit/relaunch the launcher.
2. Run **Step 15 Gates A–C** until the embedded smoke engine is setup-complete.
3. **Do not force-quit.** Return directly to Step 35.
4. Run **CORE-HANDOFF** exactly once.

Do not run another Step-35 mode in that process after Gate B begins.

## 0.0.146 CORE-HANDOFF expected evidence

The run should first reproduce the physically proven 0.0.145 baseline after the 225-pointer handoff:

`CB_REVERSE_BINDING_STATE_BEFORE` with `csharpLanguage=True`, API cache/create-binding/reverse-binding/external bridge false, and Godot runtime initialized false.

Then the coordinated bootstrap should progress through:

`CB_GAME_PLUGIN_ENTRY_CONTRACT_PASS`
→ `CB_MANAGED_CALLBACKS_BIND_PASS`
→ `CB_MANAGED_CALLBACKS_CREATE_START`
→ `CB_MANAGED_CALLBACKS_CREATE_RETURNED`
→ `CB_MANAGED_CALLBACKS_CREATE_PASS`
→ `CB_SCRIPT_LOOKUP_START`
→ `CB_SCRIPT_LOOKUP_RETURNED`
→ `CB_REVERSE_PREP_PASS`
→ `CB_NATIVE_REVERSE_INSTALL_START`
→ `CB_NATIVE_REVERSE_INSTALL_RETURNED`
→ `CB_REVERSE_BINDING_STATE_AFTER_INSTALL`
→ `CB_REVERSE_CACHE_ADOPTION_PASS`
→ `CB_CORE_API_SIGNAL_START`
→ `CB_CORE_API_SIGNAL_RETURNED`
→ `CB_MANAGED_PLUGIN_BOOTSTRAP_PASS`
→ natural Gate C.

If the process hard-terminates after `CB_MANAGED_CALLBACKS_CREATE_START`, the private GodotSharp reverse unmanaged thunks could not even be produced under the iOS runtime. If it reaches `CB_NATIVE_REVERSE_INSTALL_RETURNED` but stops at `CB_CORE_API_SIGNAL_START`, callback creation/cache adoption worked but the first native→managed callback is not callable. If `CB_MANAGED_PLUGIN_BOOTSTRAP_PASS` appears, the original missing managed-plugin lifecycle has been crossed and the next natural Gate-C frontier is meaningful.

Godot `GDMono::is_runtime_initialized()` is intentionally expected to remain false in this experiment; 0.0.146 must not fake ownership of the launcher CLR.

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

All four 0.0.146 modes execute diagnostic derivatives. A 4/4 result cannot close exact Step 35. Cancellation is INCONCLUSIVE.
