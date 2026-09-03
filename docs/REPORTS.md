# Reports

Step 35.0.23 uses immutable same-run output-only telemetry. Physical 0.0.145 cleanly proved the missing reverse cache and stopped normally before Gate C. 0.0.146 adds generated-plugin bootstrap checkpoints including `CB_GAME_PLUGIN_ENTRY_CONTRACT_PASS`, `CB_MANAGED_CALLBACKS_CREATE_*`, `CB_SCRIPT_LOOKUP_*`, `CB_NATIVE_REVERSE_INSTALL_*`, `CB_REVERSE_BINDING_STATE_AFTER_INSTALL`, `CB_CORE_API_SIGNAL_*`, and `CB_MANAGED_PLUGIN_BOOTSTRAP_PASS`. Preserve `Step35-CurrentRun.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`, `Step35-GodotNativeReconnaissance-<RunId>.txt`, `Step35-LastCheckpoint.txt`, and the normal Step-35 result report.

The static map is derived from the already verified exact transformed sts2 image before CLR admission and is never runtime input. The Godot/native reconnaissance report is likewise read-only and pre-CLR; it retains GodotSharp P/Invoke/calli/callback-field inventory plus bounded Mach-O dependency/rpath/symbol/string inspection.

Physical 0.0.143 CORE-HANDOFF evidence is the current frontier: the 1,800-byte/225-pointer callback table is accepted, GS025 Initialize returns with initialized=true, and natural execution reaches `Godot.OS.GetCmdlineArgs()` → `Godot.OS.get_Singleton()`. Physical 0.0.140 evidence remains the three-control baseline: NATURAL reaches GS031; OS-RECON reaches OS cctor→StringName→GS024; FORWARD passes `CL_CRITICAL_002_POST` and `NP002_POST` then reaches GodotFileIo→DirAccess→StringName→GS024.

0.0.144 writes the selected mode into every same-run artifact. In CORE-HANDOFF, callback readiness/table/managed-initialization events are additionally journaled as `CB_*` checkpoints. The callback table itself is not dumped or persisted as trusted input; only its observed address/size and progression markers are diagnostic output.

Historical 0.0.137 verifier failure, physical 0.0.138 NATURAL/COMPAT evidence, and 0.0.139 Codemagic 209/210 stale-summary failure remain separate provenance records under `docs/history/`.

A final resolver line remains contextual rather than causal. A 0.0.144 4/4 in any mode is diagnostic only; Step 35 remains OPEN.
