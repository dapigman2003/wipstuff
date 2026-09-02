# Reports

Step 35.0.18 uses immutable same-run output-only telemetry. Preserve `Step35-CurrentRun.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`, `Step35-GodotNativeReconnaissance-<RunId>.txt`, `Step35-LastCheckpoint.txt`, and the normal Step-35 result report.

The static map is derived from the already verified exact transformed sts2 image before CLR admission and is never runtime input. The Godot/native reconnaissance report is likewise read-only and pre-CLR; it retains GodotSharp P/Invoke/calli/callback-field inventory plus bounded Mach-O dependency/rpath/symbol/string inspection.

Physical 0.0.140 evidence is recorded separately from the 0.0.141 design: NATURAL reaches GS031; OS-RECON reaches OS cctor→StringName→GS024; FORWARD passes `CL_CRITICAL_002_POST` and `NP002_POST` then reaches GodotFileIo→DirAccess→StringName→GS024.

0.0.141 writes the selected mode into every same-run artifact. In CORE-HANDOFF, callback readiness/table/managed-initialization events are additionally journaled as `CB_*` checkpoints. The callback table itself is not dumped or persisted as trusted input; only its observed address/size and progression markers are diagnostic output.

Historical 0.0.137 verifier failure, physical 0.0.138 NATURAL/COMPAT evidence, and 0.0.139 Codemagic 209/210 stale-summary failure remain separate provenance records under `docs/history/`.

A final resolver line remains contextual rather than causal. A 0.0.141 4/4 in any mode is diagnostic only; Step 35 remains OPEN.
