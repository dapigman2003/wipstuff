# 0.0.207 viability-hardened PropertyTweener frontier

**Active candidate: 0.0.207 (207).** Step 52 remains the current architecture baseline for Step 58; legacy Steps 53–57 are optional. Codemagic for 0.0.206 compiled `StS2Launcher.Core` successfully, proving the current Core runtime source compiles. The pipeline then stopped on exactly one host-test compile error: `Assert.Greater` is not available in the pinned MSTest 4.3.2 framework. No iOS/device run occurred.

0.0.207 therefore freezes the 0.0.206 Core runtime byte-for-byte and changes only the host test, release identity, validation, and history. Phone sequence remains: fresh process through Step 52 → Step 58 → **59T CONTROL**; then fresh-process **59W WRAPPER-ONLY**; then **59X FULL REPAIR** if needed. If 59X passes, use fresh processes for 59E and real Step 59; if real Step 59 passes, continue 60/61/62 in that same process. Step 63 remains unopened.

## Retained historical invariants used by validation

The historical 0.0.201 status was: **Active candidate — Steps 58–62 NConfirmButton OnEnable preflight / visible-render trial / 0.0.201 (201)**. That candidate's one-button convenience path was capped at Step 52 before the active ownership pivot. Physical authority through **Step 57 4/4/frozen** remains retained.

**What physical 0.0.198 changed:** `SceneState` still reported the serialized `unique_name_in_owner` truth while fresh off-tree instantiated-subscene roots did not, which led to the private `PackedScene.Instantiate` compatibility correction.

**What physical 0.0.200 changed:** corrected instanced roots remained `unique_name_in_owner=true`, and the next real transition failure moved to `NConfirmButton.OnEnable()` with `_outline`, `_buttonImage`, `_viewport`, and `_hotkeys` populated.

Historical rung names retained for evidence compatibility: **Step 59.0 — forensic prerequisites + one-shot real OpenCharacterSelect transition**; **Step 60.0 — actual active character-select surface audit**; **Step 61.0 — short visible character-select render residency**; **Step 62.0 — sustained visible character-select render residency**. Step 63 remains unopened. The convenience path continues to **skip Step 38** and does not auto-run **Step 15 Gate D**.

Earlier physical Step 39/40/41 authorities remain closed. **GameStartup remains uninvoked** by the engineering ladder.

**Physical 0.0.175 / Step 42 — CLOSED POSITIVE 4/4** retained the exact **22-method zero-boundary closure**, left the **renderer frozen**, and recorded **zero resolver/host/private/initializer/rejected/native deltas**.
