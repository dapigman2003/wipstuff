# Reports

Step 35.0.11 uses run-correlated output-only telemetry. Preserve the four matching files from each physical run: `Step35-CurrentRun.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`, and `Step35-LastCheckpoint.txt`.

The 0.0.134 static map contains `[NULL PLATFORM CTOR IL]`, `[COMMAND LINE HELPER CCTOR IL]`, and `[COMMAND LINE HELPER TRYGETVALUE IL]`; the cctor section also records exact-source MaxStack. Dynamic markers use corrected exact-source ordinals: `INMETHOD_NPxxx`, `INMETHOD_CLxxx`, and `INMETHOD_CLTVxxx`, plus four `INMETHOD_CL_CRITICAL_*` stack-neutral boundaries.

A resolver event being the final durable line does not by itself prove causation. Correlate PRE/POST markers with the same-run exact-source map. Physical 0.0.133 is a diagnostic MaxStack instrumentation failure with normal `RUN_END`, not a Godot verdict. A 0.0.134 4/4 report is diagnostic localization evidence only; Step 35 remains OPEN.
