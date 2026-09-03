# Testing — Step 35.0.25

Active candidate: Step 35.0.25 / `0.0.148 (148)`, IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, workflow `ios-canonical`.

## Build/host prerequisites

1. `bash scripts/validate.sh` must pass.
2. `bash scripts/test.sh` must pass on a host with the pinned .NET SDK. 0.0.142 proved **211/211 host tests PASS** after the callback-telemetry correction; 0.0.147 added the post-bootstrap resolver-baseline regression guard but Codemagic reached 212/213 host tests because one negative test required a stale `preflight` substring. 0.0.148 corrects only that message assertion; require all current host tests to pass.
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

## 0.0.148 CORE-HANDOFF expected evidence

The run should reproduce the full physically proven 0.0.146 bridge sequence through:

`CB_MANAGED_CALLBACKS_CREATE_PASS`
→ `CB_SCRIPT_LOOKUP_RETURNED`
→ `CB_REVERSE_PREP_PASS`
→ `CB_NATIVE_REVERSE_INSTALL_RETURNED`
→ `CB_REVERSE_BINDING_STATE_AFTER_INSTALL` with API cache/create-binding/reverse-binding/external bridge true
→ `CB_REVERSE_CACHE_ADOPTION_PASS`
→ `CB_CORE_API_SIGNAL_RETURNED`
→ `CB_MANAGED_PLUGIN_BOOTSTRAP_PASS`.

0.0.148 must then emit:

`CB_POST_BOOTSTRAP_RESOLVER_BASELINE_PASS` with `addedManaged=8`, `addedHost=8`, `addedPrivate=0`
→ `CB_POST_BOOTSTRAP_RESOLVER_BASELINE_RETURNED`
→ `C_ENTRY`
→ `C_RESOLVER_PRECHECK_PASS` stating that the **post-bootstrap resolver baseline** is intact
→ natural Gate-C binding/invocation markers.

If `CB_POST_BOOTSTRAP_RESOLVER_BASELINE_FAIL` appears, preserve the full resolver state: the generated bootstrap dependency closure drifted and Gate C must remain blocked. If the baseline passes but Gate C later observes another request before target binding, that new request is a separate resolver-boundary regression.

Godot `GDMono::is_runtime_initialized()` is intentionally expected to remain false in this experiment; 0.0.148 must not fake ownership of the launcher CLR.

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

All four 0.0.148 modes execute diagnostic derivatives. A 4/4 result cannot close exact Step 35. Cancellation is INCONCLUSIVE.
