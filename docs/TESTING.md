# Testing — Step 35.0.10

Active candidate: Step 35.0.10 / `0.0.133 (133)`.

Release identity: IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, version `0.0.133 (133)`. The Codemagic workflow key remains `ios-canonical`.

Running Step 34 and then Step 35 in the same process is invalid because Step 34 leaves `sts2` resident in a non-collectible private context. Always force-quit before Step 35. Once Gate B begins, the 0.0.133 process is spent and must be force-quit before another run.

## Authority rule for 0.0.133

0.0.133 is a **diagnostic derivative**, not an exact Step-35 compatibility candidate. Gate A must re-create and verify the exact closed Step-32 transformed artifact, then create a separate instrumented clone. Gate B/C may CLR-admit and execute only that clone. A 4/4 result is localization evidence but **must not be recorded as Step-35 closure**.

Before build, run `bash scripts/validate.sh` and `bash scripts/test.sh`. Static validation must protect closed Step 29–34 manifests, current release identity, exact target tokens, writer ordering, `Action<string>::Invoke(!0)`, the corrected NP ordinal regression, CommandLine sweeps, branch-target ordinal preservation, and source-tree cleanliness. Host tests require `dotnet`.

No active text may describe 0.0.133 diagnostic 4/4 as exact Step-35 PASS/closure. The diagnostic must not add Godot startup, native loads, arbitrary managed fallback, initializer-bearing dependencies, later one-time initialization, or runtime Harmony/MonoMod patching.

## Durable telemetry

All Step-35 telemetry is output-only and never trusted as runtime input. The exact-source static map is written after Gate-A semantic verification and before Gate B. For 0.0.133 it must include:

- wrapper and MoveNext IL/callsites/await candidates;
- `[NULL PLATFORM CTOR IL]` with exact original constructor `CALLSITE#xxx` ordinals;
- `[COMMAND LINE HELPER CCTOR IL]` with exact cctor `CALLSITE#xxx` ordinals;
- `[COMMAND LINE HELPER TRYGETVALUE IL]` with exact method-body `CALLSITE#xxx` ordinals.

0.0.133 retains prior markers through `INMETHOD_024`, `INMETHOD_025/026`, `INMETHOD_180/181`, and `INMETHOD_182/183`. It adds `INMETHOD_027`, corrected `INMETHOD_NPxxx_PRE/POST`, `INMETHOD_CLxxx_PRE/POST`, and `INMETHOD_CLTVxxx_PRE/POST`.

The cctor CL plan must contain `Godot.OS.GetCmdlineArgs`; absence or unsupported branch-target placement fails Gate A before CLR admission. Injected diagnostic `Emit` calls must not consume CALLSITE ordinals. Unrelated branch-target callsites in the CommandLine sweeps may be left unwrapped while still consuming their exact-source ordinal.

## Physical run

Use a fresh process. Preserve `Step35-CurrentRun.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`, and `Step35-LastCheckpoint.txt` from the same Run ID/PID.

Interpretation:

- cctor entry with no CL PRE: failure before the first eligible cctor call marker or during marker/JIT setup;
- `INMETHOD_CLxxx_PRE` without POST: exact cctor outgoing call did not return; if the static map names `Godot.OS.GetCmdlineArgs`, that is the desired physical proof of the suspected Godot boundary;
- cctor CL markers all return and `INMETHOD_027` appears: type initialization completed and the actual `TryGetValue` body entered;
- `INMETHOD_CLTVxxx_PRE` without POST: actual method-body outgoing call failed;
- cancellation is INCONCLUSIVE, never PASS/FAIL.
