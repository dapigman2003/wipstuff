# StS2Launcher — Steps 58–62

Active candidate: **0.0.210 (210)**. The active architecture still uses **Step 52 as the prerequisite baseline for Step 58**; legacy Steps 53–57 remain optional regression/history diagnostics.

Physical **0.0.209 / 59N** materially narrowed the Step-59 blocker. Fresh Node/SceneTree Tweens are valid and `TweenInterval()` succeeds before and after killing the retained old tween. `TweenProperty()` still returns native null for inherited `position`, `scale`, and `modulate` when the target is the game-scripted `NConfirmButton`, while the same operation succeeds on the plain Godot `Outline` `TextureRect`. SceneTree creation and `BindNode` do not change that result. The active hypothesis is therefore a **generic native property-access problem on C#-scripted game objects**, not Tween state, Tween creation, or PropertyTweener wrapping.

0.0.210 deliberately **does not change the physically viable 0.0.208/0.0.209 Baseline / Wrapper / Full GodotSharp derivative builder**. It adds three FULL-profile Step-59 branches:

- **59P — generic property-resolution matrix.** Compares direct managed `Position`/`Scale`/`Modulate` access with Godot `Get`, `GetIndexed`, same-value `Set`, and `SetIndexed` on the scripted `NConfirmButton`, another game-scripted Control (`NActDropdown`), and the native `Outline` child.
- **59I — managed/native identity matrix.** Compares retained objects with private-GodotSharp `InteropUtils.UnmanagedGetManaged(NativePtr)`, instance IDs, script identity, and the underlying script-instance/instance-binding callback signatures.
- **59Q — direct-managed compatibility + original transition.** Preconditions the critical embark bypass state with direct managed setters, attempts hotkey/controller/visual/focus fidelity as best-effort stages, skips the rejected native property tween, and then invokes the original game-owned `OpenCharacterSelect` exactly once. If it closes Step 59, continue directly through **60 → 61 → 62 in the same IPA**.

Recommended first use: fresh **FULL** closed path → Step 58 → **59P**. Then a fresh FULL run for **59I**. After preserving both reports, use a fresh FULL run for **59Q**; if 59Q closes 4/4, continue immediately to 60/61/62. Existing 59D/R/H/U/T/W/X/N/Y/Z/E and real 59 remain compiled as controls/history. Never retry a mutating Step-59 branch in-process.

The source archive intentionally contains no proprietary StS2 game payload.
