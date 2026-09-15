# StS2Launcher — Steps 58–62

Active candidate: **0.0.209 (209)**. The active architecture still uses **Step 52 as the prerequisite baseline for Step 58**; legacy Steps 53–57 remain optional regression/history diagnostics.

Physical **0.0.208** resolved the current Step-59 ambiguity. Both 59W and 59X reached the isolated confirm-button tail with repair enabled. Before the failing call, the instrumented runtime had already observed valid nonzero native pointers becoming real `Godot.PropertyTweener` objects. The embark-button `TweenProperty(position, _showPos, 0.35)` call then incremented `nativeNulls`, recorded `lastNativePtr=0x0`, left `managedNulls=0`, `wrongTypes=0`, and `repairs=0`, and returned null. The active failure is therefore **native TweenProperty rejection**, not managed PropertyTweener wrapper loss.

0.0.209 deliberately **does not change the 0.0.208 Baseline / Wrapper / Full GodotSharp profile transform**. It adds exploratory Step-59 branches on the already-working FULL profile:

- **59N — native Tween rejection matrix.** Separate fresh Tweens compare Node vs SceneTree creation, optional `BindNode`, pre/post old-tween `Kill`, `IsValid` / `IsRunning` / native identity, `TweenInterval` vs `TweenProperty`, and `position` / `scale` / `modulate` target-value combinations. A failing row does not abort the matrix. Two SceneTree position rows run the complete `SetEase` → `SetTrans` → `FromCurrent` tail.
- **59Y — SceneTree alternative full tail.** Focused fresh-process confirmation of `SceneTree.CreateTween()` + exact embark position tween tail with repair disabled.
- **59Z — SceneTree + BindNode alternative full tail.** Same, but with `BindNode(embark)` before the exact tail.

Recommended first use: fresh **FULL** closed path → Step 58 → **59N**. Preserve all 59N files. Then use fresh FULL runs for **59Y** and/or **59Z** if the matrix identifies a path worth confirming. Existing 59D/R/H/U/T/W/X/E, real 59, and 60–62 remain in the same IPA. Do not retry a mutating Step-59 branch in-process.

The source archive intentionally contains no proprietary StS2 game payload.
