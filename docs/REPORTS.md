# Reports

Step 35.0.14 uses immutable same-run output-only telemetry. Preserve the matching report set from each physical run: `Step35-CurrentRun.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`, `Step35-GodotNativeReconnaissance-<RunId>.txt`, `Step35-LastCheckpoint.txt`, and the normal Step-35 result report.

`Step35-ExecuteVeryEarly-StaticMap-*` remains derived from the already verified exact transformed `sts2` source before CLR admission. It includes `[NULL PLATFORM CTOR IL]`, `[COMMAND LINE HELPER CCTOR IL]`, and `[COMMAND LINE HELPER TRYGETVALUE IL]` for correlation with corrected NP and stack-neutral critical markers.

`Step35-GodotNativeReconnaissance-*` is new in the rebuilt 0.0.137. It is written before Gate B and contains the selected exact GodotSharp IL/native-boundary map, P/Invoke/calli/callback-field inventory, bounded read-only Mach-O inventory, and the complete `INMETHOD_GS...` runtime entry-marker plan. It is output only and is never consumed as trusted runtime input.

NATURAL and COMPAT are separate fresh-process runs. The mode is written into the run journal and reconnaissance report so artifacts cannot be confused. A final resolver line does not prove causation: correlate runtime PRE/POST/GS markers with the same-run exact-source and GodotSharp maps. A 0.0.137 4/4 in either mode is diagnostic evidence only; Step 35 remains OPEN.
