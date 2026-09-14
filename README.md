# StS2Launcher — Steps 58–62

Active candidate: **0.0.205 (205)**. Physical history remains closed through **Step 57**, while the active architecture uses **Step 52 as the prerequisite baseline for Step 58**. Legacy Steps 53–57 remain optional regression/history diagnostics rather than mandatory gameplay choreography.

Use **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** from a fresh process. It ends frozen. Then run **Step 58 directly**. Step 58 adopts the current game-owned single-player submenu if it already exists, or invokes exact game-owned `NMainMenu.OpenSingleplayerSubmenu()` once only when the retained submenu must be materialized/activated. It validates the retained character-select ownership state without invoking `OpenCharacterSelect`.


Physical **0.0.204** repeated the 59T failure and proved the first null-only PropertyTweener fallback never activated (`fallbackCount=0`) while `TweenProperty()` still returned null. **0.0.205** therefore makes the same compiled IPA a control/repair toolbox: 59T runs with repair disabled and records native pointer + managed wrapper null/wrong-type identity; 59X enables repair for nonzero-pointer unusable wrappers and method-local fluent return repair. 59E and real Step 59 explicitly enable the same repair, and 60–62 remain available if the transition succeeds.

Physical **0.0.203** localized the current Step-59 failure sharply: **59U** proved `NButton.UpdateControllerButton()` returns normally; **59T** proved outline/image modulation, existing tween kill, and `Node.CreateTween()` all succeed, then `Tween.TweenProperty(...)` returns `null`; **59E** reproduced the same `NConfirmButton.OnEnable()` NRE by invoking the retained embark `Enable()` outside `OpenCharacterSelect`. The original game IL immediately chains `SetEase()` after `TweenProperty()`, so the null return explains the observed NRE.

0.0.204 keeps the physically proven `PackedScene.Instantiate()` instanced-root unique-name correction and adds a **narrow PropertyTweener managed-wrapper compatibility** inside the private GodotSharp derivative. Only the signature-specific `Godot.NativeCalls` helper used solely by `Tween.TweenProperty` is specialized. Normal managed returns are untouched. A true native null remains null. Only `managed == null && nativePtr != 0` constructs the exact internal `Godot.PropertyTweener(IntPtr)` wrapper, while recording a fallback count and last native pointer. Shared fluent helpers used by `SetEase`, `SetTrans`, and `FromCurrent` remain natural controls.

Because compilation is expensive, the same IPA retains the full Step-59 toolbox. Recommended progressive phone runs are:

- **59T first** — verifies the compatibility at the exact isolated tail and naturally continues through `SetEase`, `SetTrans`, and `FromCurrent`; its report records compatibility fallback state before/after.
- **59E second, fresh process** — tests the complete retained embark `Enable()` path.
- **real Step 59 third, fresh process** — tests actual game-owned `OpenCharacterSelect`. If it succeeds, continue to **60 → 61 → 62** without recompiling.
- **59R / 59H / 59U / 59D** remain available as controls if behavior diverges unexpectedly.

Step 35's heavy GodotSharp telemetry remains enabled for this candidate because it can still corroborate the wrapper boundary. It may make diagnostic runs laggy; that is currently a performance cost rather than the known Step-59 correctness blocker. We can reduce it after the compatibility path stabilizes.

59H/59U/59T/59E and the real Step 59 transition are fresh-process experiments. Never retry a mutating Step-59 branch in-process. Character choice, confirm/embark, run start, continuous interactive ownership, and **Step 63+ remain unopened**.

The source archive intentionally contains no proprietary StS2 game payload. Supply the previously required game files only through the established local/device workflow.
