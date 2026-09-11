# Reports

## Step 40.1 current evidence contract

`Step40-CrashCheckpoint-<RunId>.txt` is the durable run journal. `Step40-RenderPulse-StaticMap-<RunId>.txt` is written before `StartRendering`. `Step40-LastCheckpoint.txt` points at the current run. `Step40-TransformedRealStS2RenderPulse.txt` is the four-gate final summary.

Gate C requests a 100 ms delay but does not claim that UIKit can preempt an in-progress Godot frame. On the first managed continuation, `StopRendering()` must be called before checkpoint/file I/O. After it returns, the journal records first-continuation elapsed, final observed elapsed, and overshoot. Success requires rendering active after start, inactive after stop, observed first-stop opportunity 100–2000 ms, and zero initializer-bearing/rejected/native escape. Gate D preserves the same authority with rendering frozen.

If the main thread never yields, no managed timing ceiling can force `StopRendering()`. Preserve the last durable start checkpoint, force-quit/relaunch, and treat it only as a lower-bound render-hang localization.

## Step 50.0 current evidence contract

`Step50-CrashCheckpoint-<RunId>.txt`, `Step50-MainMenuFrameInput-StaticMap-<RunId>.txt`, `Step50-LastCheckpoint.txt`, and `Step50-TransformedRealStS2MainMenuRenderPulse.txt` remain the authoritative Step-50 evidence surfaces. Gate C still requests 100 ms and calls `StopRendering()` at the first managed continuation before post-stop telemetry/file I/O. 0.0.183 uses a Step-50-specific **4000 ms** post-stop evidence ceiling because the supplied first 0.0.182 attempt physically observed a successful start/refreeze at ~2526 ms. This does not delay or schedule StopRendering; it changes only post-stop classification. Historical Step 40 remains 100–2000 ms and Step 52 remains 1500/6000 ms.

`PhysicallyClosedPath-ToStep52.txt` is a convenience-runner report only. It never replaces the individual numbered-step reports/checkpoints.

## Steps 53–57 evidence contract

Physical 0.0.184 reached Step 53 Gate A and then failed safely at Gate B because the candidate assumed a `void` return. The actual zero-argument method returns exact `NSingleplayerSubmenu`; no handler was invoked, rendering remained frozen, and Step 54 was never armed. 0.0.185 pins that return type and requires Step 54 invocation-return identity to match the retained `_singleplayerSubmenu`.

Step 53: `Step53-CrashCheckpoint-<RunId>.txt`, `Step53-SingleplayerOpen-StaticMap-<RunId>.txt`, `Step53-LastCheckpoint.txt`, `Step53-TransformedRealStS2SingleplayerOpenFrontier.txt`.

Step 54: `Step54-CrashCheckpoint-<RunId>.txt`, `Step54-SingleplayerSubmenuOpen-StaticMap-<RunId>.txt`, `Step54-LastCheckpoint.txt`, `Step54-TransformedRealStS2SingleplayerSubmenuFrozenOpen.txt`. Gate C is one-shot after the binding map is durable.

Step 55: `Step55-CrashCheckpoint-<RunId>.txt`, `Step55-SingleplayerSubmenuFrameInput-StaticMap-<RunId>.txt`, `Step55-LastCheckpoint.txt`, `Step55-TransformedRealStS2SingleplayerSubmenuRender.txt`. Requested residency is 750 ms with 5000 ms post-stop evidence ceiling; `StopRendering()` is first managed-continuation work before telemetry.

Step 56: `Step56-CrashCheckpoint-<RunId>.txt`, `Step56-CharacterSelectFrontier-StaticMap-<RunId>.txt`, `Step56-LastCheckpoint.txt`, `Step56-TransformedRealStS2CharacterSelectFrontier.txt`. No character-select invocation.

Step 57: `Step57-CrashCheckpoint-<RunId>.txt`, `Step57-CharacterSelectResource-StaticMap-<RunId>.txt`, `Step57-LastCheckpoint.txt`, `Step57-TransformedRealStS2CharacterSelectResourcePreflight.txt`. Exact PCK bytes are read-only evidence; no ResourceLoader/PackedScene/character-select execution.
