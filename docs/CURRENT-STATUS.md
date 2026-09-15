# 0.0.210 scripted-object property bridge frontier

**Active candidate: 0.0.210 (210).** Physical 0.0.209 / 59N disproved the remaining Tween-state and creation-path explanations. Fresh Node/SceneTree Tweens were valid and running, and `TweenInterval()` returned valid `IntervalTweener` objects through Node, SceneTree, and SceneTree+BindNode. The old retained tween was distinct from every new tween.

The target/property matrix was decisive: `TweenProperty()` returned native null for `NConfirmButton.position`, `NConfirmButton.scale`, and `NConfirmButton.modulate`, including current-value type matches and SceneTree-created Tweens. The same `TweenProperty()` machinery succeeded for `Outline.modulate` on the native `TextureRect` child. The active frontier is therefore **generic native property resolution on C#-scripted game objects**.

0.0.210 keeps the physically viable Baseline / Wrapper / Full GodotSharp derivative builder byte-for-byte unchanged from 0.0.209 and adds **59P / 59I / 59Q** only in the Step-59 runtime/UI layer. 59P compares direct managed access against Godot generic Get/GetIndexed/Set/SetIndexed on two game-scripted Controls and one native child. 59I compares native-pointer-to-managed identity through private GodotSharp. 59Q is a targeted progression fallback: it pre-enables the retained embark button with required direct managed state plus best-effort button-fidelity stages and then runs the original `OpenCharacterSelect`; if successful, it closes Step 59 and unlocks 60–62 without another compilation.

Phone plan: **fresh FULL closed path → 58 → 59P**; preserve all files. Then **fresh FULL → 58 → 59I**. Then **fresh FULL → 58 → 59Q**. If 59Q closes Step 59 4/4, continue **60 → 61 → 62 in that same process**. Older 59N/Y/Z are now retained controls rather than preferred next-value runs.

## Retained historical invariants used by validation

The historical 0.0.201 status was: **Active candidate — Steps 58–62 NConfirmButton OnEnable preflight / visible-render trial / 0.0.201 (201)**. That candidate's one-button convenience path was capped at Step 52 before the active ownership pivot. Physical authority through **Step 57 4/4/frozen** remains retained.

**What physical 0.0.198 changed:** `SceneState` still reported the serialized `unique_name_in_owner` truth while fresh off-tree instantiated-subscene roots did not, which led to the private `PackedScene.Instantiate` compatibility correction.

**What physical 0.0.200 changed:** corrected instanced roots remained `unique_name_in_owner=true`, and the next real transition failure moved to `NConfirmButton.OnEnable()` with `_outline`, `_buttonImage`, `_viewport`, and `_hotkeys` populated.

Historical rung names retained for evidence compatibility: **Step 59.0 — forensic prerequisites + one-shot real OpenCharacterSelect transition**; **Step 60.0 — actual active character-select surface audit**; **Step 61.0 — short visible character-select render residency**; **Step 62.0 — sustained visible character-select render residency**. Step 63 remains unopened. The convenience path continues to **skip Step 38** and does not auto-run **Step 15 Gate D**.

Earlier physical Step 39/40/41 authorities remain closed. **GameStartup remains uninvoked** by the engineering ladder.

**Physical 0.0.175 / Step 42 — CLOSED POSITIVE 4/4** retained the exact **22-method zero-boundary closure**, left the **renderer frozen**, and recorded **zero resolver/host/private/initializer/rejected/native deltas**.
