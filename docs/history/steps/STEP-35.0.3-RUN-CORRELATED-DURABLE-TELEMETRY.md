# Step 35.0.3 — Run-Correlated Durable Telemetry

Candidate: **0.0.126 (126)**

## Why this revision exists

Physical 0.0.125 reproduced the same main-thread PC=`0x0`, `CODESIGNING / Invalid Page` hard-kill family as 0.0.124, but its available static-map artifact and crash report were from different process runs. The expected fixed-name crash checkpoint for the crash-report process was absent. Because the 0.0.125 checkpoint/static-map writers swallowed filesystem exceptions to `Console.Error`, a failed write could be invisible on-device while execution continued.

This is an evidence-quality problem, not a new compatibility result. Step 35 remains OPEN and the 0.0.124 invocation frontier remains authoritative.

## Change in 0.0.126

Before Gate A, the launcher now creates a run identity containing UTC, PID and a GUID and durably establishes:

- `Step35-CurrentRun.txt` — current-run manifest containing Run ID/PID and exact artifact filenames;
- `Step35-LastCheckpoint.txt` — independently flushed overwrite-on-each-checkpoint convenience file;
- `Step35-CrashCheckpoint-<RunId>.txt` — immutable run-specific append journal;
- `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt` — same-run static wrapper/MoveNext IL map.

The journal header, every journal record, the current-run manifest, last-checkpoint file, and static map all carry the same Run ID/PID. Cross-run correlation is therefore explicit rather than inferred from timestamps.

If the initial run journal cannot be created and flushed, the UI reports **TELEMETRY FAIL / NOT RUN** and Gate A is not entered. If Gate A compatibility checks pass but the same-run static map cannot be durably written, the run stops before Gate B and reports a diagnostic telemetry stop. Neither condition is classified as a compatibility failure.

## Runtime authority remains frozen

This revision does not alter transformed bytes, target selection, target invocation count, resolver allowlist/refusal behavior, Task timeout, or later startup boundaries. Gate B remains exact transformed-primary admission. Gate C remains one reflected invocation of exact static parameterless Task-returning `OneTimeInitialization::ExecuteVeryEarly()` followed by a <=60-second await if the invocation returns a Task.

`ExecuteEssential`, `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry-point execution, Harmony/MonoMod patching, initializer-bearing `0Harmony 2.4.2.0`, unplanned managed/native loading, and Godot/game startup remain forbidden.

## Physical acceptance

Run exact 0.0.126 from a fresh process. After a hard termination, preserve `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, the run-specific crash journal named by the current-run manifest, the run-specific static map named by the manifest, and the matching `.ips` before any retry. The next execution discriminator must be based only on same-Run-ID evidence.
