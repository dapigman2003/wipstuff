# Reports

Step 35.0.17 uses immutable same-run output-only telemetry. Preserve `Step35-CurrentRun.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`, `Step35-GodotNativeReconnaissance-<RunId>.txt`, `Step35-LastCheckpoint.txt`, and the normal Step-35 result report for each fresh-process run.

`Step35-ExecuteVeryEarly-StaticMap-*` is generated from the already verified exact transformed sts2 image before CLR admission and is never consumed as runtime input. Its CommandLine sections remain the authority for correlating `CL_CRITICAL_001/002` with the exact natural instructions even when a diagnostic derivative substitutes those calls afterward.

`Step35-GodotNativeReconnaissance-*` remains read-only and pre-CLR. In 0.0.140 the selected managed map/entry-marker closure is expanded around `Godot.OS::.cctor`, `Godot.OS/MethodName::.cctor`, StringName, ClassDB, and relevant NativeFuncs callback thunks in addition to the Dictionary/GetCmdlineArgs chain. It also retains P/Invoke/calli/callback-field inventory and bounded Mach-O dependency/rpath/symbol/string inspection. No native image is loaded or executed by this report.

The mode name is written into the journal/reconnaissance output. NATURAL, OS-RECON (`ManagedDictionaryCompatibility`), and FORWARD (`ManagedCommandLineCompatibility`) must never be compared without their Run IDs because their sts2 derivative hashes intentionally differ.

Historical physical 0.0.138 NATURAL/COMPAT evidence is summarized in `docs/history/reports/STEP-35.0.15-PHYSICAL-NATURAL-COMPAT-CALLBACK-BOUNDARIES-0.0.138.txt`. The 0.0.137 Codemagic verifier failure remains separate CI evidence. The 0.0.139 Codemagic 209/210 stale-summary failure is recorded separately and produced no IPA/device evidence.

A final resolver line remains contextual rather than causal. A 0.0.140 4/4 in any mode is diagnostic only; Step 35 remains OPEN.
