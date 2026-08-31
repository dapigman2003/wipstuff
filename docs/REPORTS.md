# Reports

Step 35.0.10 uses run-correlated output-only telemetry. Preserve the four matching files from each physical run: `Step35-CurrentRun.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`, and `Step35-LastCheckpoint.txt`.

The 0.0.133 static map contains `[NULL PLATFORM CTOR IL]`, `[COMMAND LINE HELPER CCTOR IL]`, and `[COMMAND LINE HELPER TRYGETVALUE IL]`. Dynamic markers use corrected exact-source ordinals: `INMETHOD_NPxxx`, `INMETHOD_CLxxx`, and `INMETHOD_CLTVxxx`.

A resolver event being the final durable line does not by itself prove causation. Correlate PRE/POST markers with the same-run exact-source map. A 0.0.133 4/4 report is diagnostic localization evidence only; Step 35 remains OPEN.
