# Testing — Step 35.0.14

Active candidate: Step 35.0.14 / `0.0.137 (137)`.

Release identity: IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, version `0.0.137 (137)`. The Codemagic workflow key remains `ios-canonical`.

Running Step 34 and then Step 35 in the same process is invalid. Always force-quit before Step 35. Once Gate B begins, the process is spent and must be force-quit before another Step-35 run.

## Authority rule

0.0.137 is a diagnostic compatibility derivative. Gate A must re-create and verify the exact closed Step-32 transformed artifact, write the same-run exact-source static map, then create a separate diagnostic clone. Gate B/C may CLR-admit and execute only that clone. A 4/4 result is evidence only and cannot be recorded as exact Step-35 closure.

## Required host regressions

Before build, run `bash scripts/validate.sh` and `bash scripts/test.sh` in a host with `dotnet`. Coverage must protect the corrected `Action<string>::Invoke(!0)` bridge, exact NP ordinals, stack-neutral CommandLine critical markers, absence of production live-stack CL/CLTV sweeps, and the managed dictionary compatibility rewrite. The rewrite regression must serialize/reopen a fixture and verify:

- exactly one existing `System.Collections` AssemblyRef is reused;
- `_args` becomes `System.Collections.Generic.Dictionary<string,string>`;
- `.ctor`, `set_Item(!0,!1)`, and `TryGetValue(!0,!1&)` use the BCL constructed generic declaring type with VAR(0)/VAR(1) signatures;
- the affected cctor/TryGetValue methods retain no Godot string-dictionary call references;
- `Godot.OS.GetCmdlineArgs()` remains exactly once and natural.

The older executable MaxStack regression remains because it protects the retired generic sweep helper from regressing if reused later.

## Durable telemetry

Preserve `Step35-CurrentRun.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`, `Step35-LastCheckpoint.txt`, and the normal Step-35 report from the same Run ID/PID.

The static map must remain exact-source and include `[NULL PLATFORM CTOR IL]`, `[COMMAND LINE HELPER CCTOR IL]`, and `[COMMAND LINE HELPER TRYGETVALUE IL]`. For 0.0.137, the expected high-value physical sequence is:

- `CL_CRITICAL_001_PRE` then `CL_CRITICAL_001_POST`: the managed dictionary substitution completed;
- `CL_CRITICAL_002_PRE` without POST: natural `Godot.OS.GetCmdlineArgs()` did not return;
- `CL_CRITICAL_002_POST`: the Godot command-line call returned and later markers become the next frontier;
- `INMETHOD_027`: cctor completed and actual `TryGetValue` entered;
- `NP002_POST`: `TryGetValue` returned to `NullPlatformUtilStrategy..ctor`.

Cancellation is INCONCLUSIVE. Resolver events alone do not establish root cause.
