# Reports

## Step 40.1 current evidence contract

`Step40-CrashCheckpoint-<RunId>.txt` is the durable run journal. `Step40-RenderPulse-StaticMap-<RunId>.txt` is written before `StartRendering`. `Step40-LastCheckpoint.txt` points at the current run. `Step40-TransformedRealStS2RenderPulse.txt` is the four-gate final summary.

Gate C requests a 100 ms delay but does not claim that UIKit can preempt an in-progress Godot frame. On the first managed continuation, `StopRendering()` must be called before checkpoint/file I/O. After it returns, the journal records first-continuation elapsed, final observed elapsed, and overshoot. Success requires rendering active after start, inactive after stop, observed first-stop opportunity 100–2000 ms, and zero initializer-bearing/rejected/native escape. Gate D preserves the same authority with rendering frozen.

If the main thread never yields, no managed timing ceiling can force `StopRendering()`. Preserve the last durable start checkpoint, force-quit/relaunch, and treat it only as a lower-bound render-hang localization.
