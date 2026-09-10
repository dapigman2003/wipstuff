# Reports

## Step 40.1 current evidence contract

`Step40-CrashCheckpoint-<RunId>.txt` is the durable run journal. `Step40-RenderPulse-StaticMap-<RunId>.txt` is written before `StartRendering`. `Step40-LastCheckpoint.txt` points at the current run. `Step40-TransformedRealStS2RenderPulse.txt` is the four-gate final summary.

Gate C requests a 100 ms delay but does not claim that UIKit can preempt an in-progress Godot frame. On the first managed continuation, `StopRendering()` must be called before checkpoint/file I/O. After it returns, the journal records first-continuation elapsed, final observed elapsed, and overshoot. Success requires rendering active after start, inactive after stop, observed first-stop opportunity 100–2000 ms, and zero initializer-bearing/rejected/native escape. Gate D preserves the same authority with rendering frozen.

If the main thread never yields, no managed timing ceiling can force `StopRendering()`. Preserve the last durable start checkpoint, force-quit/relaunch, and treat it only as a lower-bound render-hang localization.

## Step 50.0 current evidence contract

`Step50-CrashCheckpoint-<RunId>.txt`, `Step50-MainMenuFrameInput-StaticMap-<RunId>.txt`, `Step50-LastCheckpoint.txt`, and `Step50-TransformedRealStS2MainMenuRenderPulse.txt` remain the authoritative Step-50 evidence surfaces. Gate C still requests 100 ms and calls `StopRendering()` at the first managed continuation before post-stop telemetry/file I/O. 0.0.183 uses a Step-50-specific **4000 ms** post-stop evidence ceiling because the supplied first 0.0.182 attempt physically observed a successful start/refreeze at ~2526 ms. This does not delay or schedule StopRendering; it changes only post-stop classification. Historical Step 40 remains 100–2000 ms and Step 52 remains 1500/6000 ms.

`PhysicallyClosedPath-ToStep52.txt` is a convenience-runner report only. It never replaces the individual numbered-step reports/checkpoints.
