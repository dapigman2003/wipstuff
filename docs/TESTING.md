# Testing — Step 35.0.11

Active candidate: Step 35.0.11 / `0.0.134 (134)`.

Release identity: IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, version `0.0.134 (134)`. The Codemagic workflow key remains `ios-canonical`.

Running Step 34 and then Step 35 in the same process is invalid because Step 34 leaves `sts2` resident in a non-collectible private context. Always force-quit before Step 35. Once Gate B begins, the 0.0.134 process is spent and must be force-quit before another run.

## Authority rule for 0.0.134

0.0.134 is a **diagnostic derivative**, not an exact Step-35 compatibility candidate. Gate A must re-create and verify the exact closed Step-32 transformed artifact, then create a separate instrumented clone. Gate B/C may CLR-admit and execute only that clone. A 4/4 result is localization evidence but **must not be recorded as Step-35 closure**.

Before build, run `bash scripts/validate.sh` and `bash scripts/test.sh`. Static validation must protect closed Step 29–34 manifests, current release identity, exact target tokens, writer ordering, `Action<string>::Invoke(!0)`, corrected NP ordinals, MaxStack reservation/post-write verification, critical CommandLine boundaries, CL/CLTV sweeps, branch-target ordinal preservation, and source-tree cleanliness. Host tests require `dotnet`.

The 0.0.133 regression is mandatory context: it physically returned managed `InvalidProgramException` before the CommandLine cctor entry marker and reached normal `RUN_END`. That result is an instrumentation failure, not a Godot compatibility verdict.

## MaxStack regression requirements

The host suite must include both structural and executable coverage:

- a real-shape CommandLine cctor fixture with Godot dictionary construction + `Godot.OS.GetCmdlineArgs`, stack-neutral critical markers, serialized/reopened MaxStack verification, and exact CL ordinals;
- a generated cctor whose original declared `MaxStack=3` calls a three-argument method. After the live-stack sweep it must serialize as `MaxStack=4`, be loaded into a CLR `AssemblyLoadContext`, and execute its type initializer successfully. Removing the MaxStack reservation should make this regression reproduce the 0.0.133 `InvalidProgramException` class of failure.

## Durable telemetry

All Step-35 telemetry is output-only and never trusted as runtime input. The exact-source static map is written after Gate-A semantic verification and before Gate B. For 0.0.134 it must include:

- wrapper and MoveNext IL/callsites/await candidates;
- `[NULL PLATFORM CTOR IL]` with exact original constructor `CALLSITE#xxx` ordinals;
- `[COMMAND LINE HELPER CCTOR IL]` with the exact-source cctor MaxStack and exact `CALLSITE#xxx` ordinals;
- `[COMMAND LINE HELPER TRYGETVALUE IL]` with exact method-body `CALLSITE#xxx` ordinals.

0.0.134 retains prior markers through `INMETHOD_024`, `INMETHOD_025/026`, `INMETHOD_180/181`, and `INMETHOD_182/183`. It retains corrected `INMETHOD_NPxxx_PRE/POST`, `INMETHOD_CLxxx_PRE/POST`, `INMETHOD_CLTVxxx_PRE/POST`, and `INMETHOD_027`. It adds four redundant stack-neutral critical markers:

- `INMETHOD_CL_CRITICAL_001_PRE` — before `_args` dictionary construction;
- `INMETHOD_CL_CRITICAL_001_POST` — after `_args` dictionary assignment;
- `INMETHOD_CL_CRITICAL_002_PRE` — before `Godot.OS.GetCmdlineArgs`;
- `INMETHOD_CL_CRITICAL_002_POST` — after its result is stored.

The cctor CL plan must contain `Godot.OS.GetCmdlineArgs`; absence or unsupported branch-target placement fails Gate A before CLR admission. Injected diagnostic `Emit` calls must not consume CALLSITE ordinals.

## Physical run

Use a fresh process. Preserve `Step35-CurrentRun.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`, `Step35-LastCheckpoint.txt`, and the normal Step-35 report from the same Run ID/PID.

Interpretation:

- cctor entry absent: invalid/JIT/type-init failure before first instrumented instruction;
- dictionary critical PRE without POST: dictionary construction/assignment boundary;
- dictionary POST + GetCmdlineArgs critical PRE without POST: `Godot.OS.GetCmdlineArgs` did not return;
- GetCmdlineArgs critical POST: the suspected Godot boundary returned and later CL markers become authoritative localization evidence;
- cctor finishes and `INMETHOD_027` appears: actual `TryGetValue` body entered;
- `INMETHOD_CLTVxxx_PRE` without POST: method-body outgoing call did not return;
- cancellation is INCONCLUSIVE, never PASS/FAIL.
