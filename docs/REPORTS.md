# Reports

## Step 40.0 current evidence contract

Physical 0.0.170 is the authority closing Step 39.2 at 4/4. Step 40 must follow Step 39 in the same process because Step 39 intentionally retains the real NGame in-tree with rendering frozen. The required chain is Step 15 → Step 35 → Step 36 → Step 37 → skip Step 38 → Step 39 4/4 → Step 40 once.

Step 40 emits four run-correlated text artifacts under `Documents/StS2Launcher/Reports`:

- `Step40-CrashCheckpoint-<RunId>.txt` — synchronously flushed gate/pulse journal.
- `Step40-RenderPulse-StaticMap-<RunId>.txt` — actual in-tree hierarchy plus frame/input Cecil map, written before `StartRendering`.
- `Step40-LastCheckpoint.txt` — overwrite-on-checkpoint convenience pointer to the run journal/static map and latest durable boundary.
- `Step40-TransformedRealStS2RenderPulse.txt` — final 4-gate summary/detail report.

Critical pulse checkpoints are `J_C_PULSE_ARMED`, `J_C_START_RENDERING_CALL`, `J_C_START_RENDERING_RETURNED`, `J_C_STOP_RENDERING_CALL`, and `J_C_STOP_RENDERING_RETURNED`. A crash/watchdog between these points is interpreted only up to the last durable checkpoint. Once `J_C_PULSE_ARMED` exists, never retry Step 40 in-process.

Successful Gate C evidence must show rendering active after start, inactive after stop, observed elapsed <=500 ms, and zero initializer-bearing/rejected/native escape. Gate D must preserve the same authority with rendering still frozen. Step 40 never restarts rendering after its stop.

Historical reports and design records remain under `docs/history/`.
