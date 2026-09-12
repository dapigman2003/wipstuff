# Reports

## Step 40.1 current evidence contract

`Step40-CrashCheckpoint-<RunId>.txt` is the durable run journal. `Step40-RenderPulse-StaticMap-<RunId>.txt` is written before `StartRendering`. `Step40-LastCheckpoint.txt` points at the current run. `Step40-TransformedRealStS2RenderPulse.txt` is the four-gate final summary.

Gate C requests a 100 ms delay but does not claim that UIKit can preempt an in-progress Godot frame. On the first managed continuation, `StopRendering()` must be called before checkpoint/file I/O. After it returns, the journal records first-continuation elapsed, final observed elapsed, and overshoot. Success requires rendering active after start, inactive after stop, observed first-stop opportunity 100–2000 ms, and zero initializer-bearing/rejected/native escape. Gate D preserves the same authority with rendering frozen.

If the main thread never yields, no managed timing ceiling can force `StopRendering()`. Preserve the last durable start checkpoint, force-quit/relaunch, and treat it only as a lower-bound render-hang localization.

## Step 50.0 current evidence contract

`Step50-CrashCheckpoint-<RunId>.txt`, `Step50-MainMenuFrameInput-StaticMap-<RunId>.txt`, `Step50-LastCheckpoint.txt`, and `Step50-TransformedRealStS2MainMenuRenderPulse.txt` remain the authoritative Step-50 evidence surfaces. Gate C still requests 100 ms and calls `StopRendering()` at the first managed continuation before post-stop telemetry/file I/O. 0.0.183 uses a Step-50-specific **4000 ms** post-stop evidence ceiling because the supplied first 0.0.182 attempt physically observed a successful start/refreeze at ~2526 ms. This does not delay or schedule StopRendering; it changes only post-stop classification. Historical Step 40 remains 100–2000 ms and Step 52 remains 1500/6000 ms.

`PhysicallyClosedPath-ToStep52.txt` is a convenience-runner report only. It never replaces the individual numbered-step reports/checkpoints.

## Steps 53–57 evidence contract

Physical 0.0.184 reached Step 53 Gate A and then failed safely at Gate B because the candidate assumed a `void` return. The actual zero-argument method returns exact `NSingleplayerSubmenu`; no handler was invoked, rendering remained frozen, and Step 54 was never armed. 0.0.185 pinned that return type. Physical 0.0.185 then reached Step 54 Gate B and safely proved that `_singleplayerSubmenu` is not owned directly by `NMainMenu`; 0.0.186 corrected that ownership, then physically advanced through Step 55 4/4/refrozen and reached Step 56 Gate B. That Gate-B stop proved `OpenCharacterSelect` is `void OpenCharacterSelect(MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton)`, not zero-argument; no character-select invocation occurred. 0.0.187 pinned that exact signature, closed Step 56, and then Step 57 Gate A safely localized a separate scene-discovery mismatch with `observed=none`. 0.0.188 attempted to preserve the exact short/full hint but physical Step 57 again reported `observed=none`, proving no qualifying literal exists in the Step-56 immediate closure. 0.0.189 binds the actual retained `_characterSelectScreenScene : Godot.PackedScene`, uses only its existing `ResourcePath` for the receipt-backed PCK identity proof, and physically closed Step 57 4/4/frozen with exact 16,015-byte resource/risk evidence.

Step 53: `Step53-CrashCheckpoint-<RunId>.txt`, `Step53-SingleplayerOpen-StaticMap-<RunId>.txt`, `Step53-LastCheckpoint.txt`, `Step53-TransformedRealStS2SingleplayerOpenFrontier.txt`.

Step 54: `Step54-CrashCheckpoint-<RunId>.txt`, `Step54-SingleplayerSubmenuOpen-StaticMap-<RunId>.txt`, `Step54-LastCheckpoint.txt`, `Step54-TransformedRealStS2SingleplayerSubmenuFrozenOpen.txt`. Gate C is one-shot after the binding map is durable.

Step 55: `Step55-CrashCheckpoint-<RunId>.txt`, `Step55-SingleplayerSubmenuFrameInput-StaticMap-<RunId>.txt`, `Step55-LastCheckpoint.txt`, `Step55-TransformedRealStS2SingleplayerSubmenuRender.txt`. Requested residency is 750 ms with 5000 ms post-stop evidence ceiling; `StopRendering()` is first managed-continuation work before telemetry.

Step 56: `Step56-CrashCheckpoint-<RunId>.txt`, `Step56-CharacterSelectFrontier-StaticMap-<RunId>.txt`, `Step56-LastCheckpoint.txt`, `Step56-TransformedRealStS2CharacterSelectFrontier.txt`. Exact callback contract is one `NButton` parameter and `void` return. Resource literals/hints are diagnostic only. No character-select invocation.

Step 57: `Step57-CrashCheckpoint-<RunId>.txt`, `Step57-CharacterSelectResource-StaticMap-<RunId>.txt`, `Step57-LastCheckpoint.txt`, `Step57-TransformedRealStS2CharacterSelectResourcePreflight.txt`. Gate A now records selected-metadata + retained-runtime `_characterSelectScreenScene : Godot.PackedScene`, its existing exact `ResourcePath`, and one exact PCK directory match before Gate B extracts bytes. Exact PCK bytes are read-only evidence; no `ResourceLoader`, `PackedScene.Instantiate`, or character-select execution.


Physical 0.0.185 Step 54 localization evidence is retained as `STEP-54.0-PHYSICAL-0.0.185-FAIL-REPORT.txt`, `STEP-54.0-PHYSICAL-0.0.185-FAIL-CHECKPOINT.txt`, and `STEP-54.0-PHYSICAL-0.0.185-FAIL-LAST-CHECKPOINT.txt`.

Physical 0.0.186 Step 56 localization evidence is retained as `STEP-56.0-PHYSICAL-0.0.186-FAIL-REPORT.txt`, `STEP-56.0-PHYSICAL-0.0.186-FAIL-CHECKPOINT.txt`, and `STEP-56.0-PHYSICAL-0.0.186-FAIL-LAST-CHECKPOINT.txt`.

Physical 0.0.187 Step 57 localization evidence is retained as `STEP-57.0-PHYSICAL-0.0.187-FAIL-REPORT.txt`, `STEP-57.0-PHYSICAL-0.0.187-FAIL-CHECKPOINT.txt`, and `STEP-57.0-PHYSICAL-0.0.187-FAIL-LAST-CHECKPOINT.txt`.

Physical 0.0.188 Step 57 retained-hint localization evidence is retained as `STEP-57.0-PHYSICAL-0.0.188-FAIL-REPORT.txt`, `STEP-57.0-PHYSICAL-0.0.188-FAIL-CHECKPOINT.txt`, and `STEP-57.0-PHYSICAL-0.0.188-FAIL-LAST-CHECKPOINT.txt`.


## Steps 58–62 evidence contract

Physical 0.0.190 proved Step 58 may see a non-null game-owned character-select cache. Physical 0.0.191 then proved that exact cached `NCharacterSelectScreen` may already be **inside the live SceneTree** before Step 58 performs any new operation. Both runs ended frozen and Step 58 itself created nothing. 0.0.193 therefore adopts real runtime ownership instead of requiring launcher-controlled off-tree lifecycle milestones.

Physical 0.0.192 then localized one remaining ownership assumption: the cached in-tree screen can still have `NSubmenu._stack` unbound before the real push transition. 0.0.193 therefore separates SceneTree attachment from logical stack binding; null is allowed pre-push, while any foreign non-null stack remains a hard failure.

Each rung has a distinct report/static-map/checkpoint surface:

- Step 58: `Step58-CharacterSelectOwnership-StaticMap-<RunId>.txt`, `Step58-TransformedRealStS2CharacterSelectOwnership.txt`.
- Step 59: `Step59-RealOpenCharacterSelect-StaticMap-<RunId>.txt`, `Step59-TransformedRealStS2OpenCharacterSelectFrozen.txt`.
- Step 60: `Step60-CharacterSelectActiveSurface-StaticMap-<RunId>.txt`, `Step60-TransformedRealStS2CharacterSelectActiveSurface.txt`.
- Step 61: `Step61-CharacterSelectShortRender-StaticMap-<RunId>.txt`, `Step61-TransformedRealStS2CharacterSelectShortRender.txt`.
- Step 62: `Step62-CharacterSelectSustainedRender-StaticMap-<RunId>.txt`, `Step62-TransformedRealStS2CharacterSelectSustainedRender.txt`.

Step 58 is observation/audit only. Step 59 skips duplicate handler invocation when the exact screen is already visible/in-tree; otherwise its handler path is one-shot. Step 60 is audit only. Steps 61/62 must write their static evidence before `StartRendering`; `StopRendering()` must precede all post-stop telemetry. Step 61 targets 2 seconds with a 10-second evidence ceiling. Step 62 targets 10 seconds with a 30-second evidence ceiling. Neither render rung authorizes intentional interaction.

Physical 0.0.190 and 0.0.191 Step-58 failure artifacts are retained in `docs/history/reports/` as the provenance for this ownership pivot. Step 63 remains unopened.
