# Step 59.7 — NConfirmButton OnEnable preflight + compilation-efficient diagnostic deck / 0.0.201

## Physical input

Physical 0.0.200 reaches Step 59 after the 0.0.199/0.0.200 global PackedScene instanced-root unique-name compatibility correction. The retained Step-59 provenance evidence now reports `unique_name_in_owner=true` for the audited character-select, game, and main-menu instanced roots in both fresh temporary and retained live instances. The previously missing character-select `_ascensionPanel` / `_actDropdown` and `NGame.TimeoutOverlay` fields are populated before transition arm.

The original one-shot `NSingleplayerSubmenu.OpenCharacterSelect(NButton)` then reaches `NSubmenuStack.Push` and `NCharacterSelectScreen.OnSubmenuOpened()`. The transition fails with `System.NullReferenceException` at `MegaCrit.Sts2.Core.Nodes.CommonUi.NConfirmButton.OnEnable()`, reached through `_embarkButton.Enable()`. The post-failure state proves the character-select logical stack was bound to the exact retained main-menu stack and a one-player `StartRunLobby` existed. Rendering remained frozen and the operation returned normally; an armed Step 59 must not be retried in-process.

## Trusted static localization

Selected `sts2.dll` IL localizes the next dependency chain without invoking it:

- `NCharacterSelectScreen.OnSubmenuOpened()` enables the retained `_embarkButton` before later transition work.
- `NConfirmButton._Ready()` binds `_outline` from child `Outline`, `_buttonImage` from child `Image`, records `_viewport = GetViewport()`, establishes positional state, connects viewport-size change handling, then disables the button.
- `NConfirmButton.OnEnable()` first enters the base `NButton.OnEnable()` path and then dereferences `_outline` and `_buttonImage`, handles `_moveTween`, creates a tween, and animates toward `_showPos`.
- the base `NButton` enable path registers hotkeys and therefore also depends on the retained `_hotkeys` state established by normal construction/scene lifecycle.

This evidence is sufficient to justify a narrower preflight, but not a speculative repair.

## 0.0.201 change

0.0.201 makes no new repair and leaves the 0.0.199/0.0.200 PackedScene compatibility runtime hook unchanged. Because compilation is expensive and device runs are cheap, the candidate is expanded into a multi-run diagnostic platform. A dedicated **Step 59D** operation executes only Step-59 Gates A+B, writes the complete forensic map, and returns without calling the Gate-C transition. The separate normal Step-59 operation remains available from the same compiled IPA but requires a fresh process after Step59D.

Step-59 Gate B now records, before any one-shot handler arm:

- retained `_embarkButton` exact runtime type, private load-context identity, `IsInsideTree()` and `IsNodeReady()`;
- `_outline`, `_buttonImage`, `_viewport`, `_hotkeys`, `_moveTween`, `_showPos`, `_isEnabled`, and `_controllerHotkeyIcon` values/types;
- exact selected `NConfirmButton._Ready`, `NConfirmButton.OnEnable`, `NButton.OnEnable`, and `NButton.RegisterHotkeys` IL, with static field-store/load assertions for the known Ready/OnEnable dependency surface;
- the retained embark-button live child graph;
- all serialized `SceneState` properties on the character-select embark instanced root;
- exact character-select TSCN context for that root and exact confirm-button TSCN context around `Outline` and `Image`.
- a full inherited-field matrix for the retained embark button, not just the four currently suspected prerequisites;
- a whole retained-SceneTree peer matrix for every live `NConfirmButton`, allowing healthy/failed lifecycle products to be compared in the same binary;
- observational `GetViewport`/tree/parent/owner accessors and a null-field descendant-candidate map;
- compact field/call ordering for `NConfirmButton._Ready`, `NConfirmButton.OnEnable`, `NButton.OnEnable`, `NButton.RegisterHotkeys`, `NClickableControl.Enable`, and `NCharacterSelectScreen.OnSubmenuOpened`;
- execution-qualified transitive frontiers for those methods under the already retained Step-48 runtime guards.

If a separate real-handler run still throws, 0.0.201 captures the post-failure inherited-field/peer/null-candidate runtime deck and appends it to the Step-59 static map before returning to the UI. This maximizes evidence per compilation while preserving the rule that an armed handler is never retried in-process.

If the embark button is not the selected `NConfirmButton`, is outside the selected private load context/tree, is not node-ready, or has null `_outline`, `_buttonImage`, `_viewport`, or `_hotkeys`, Gate B records the blocker and Gate C refuses to arm `OpenCharacterSelect`. No direct field write, manual `_Ready()`, `OnEnable()` bypass, scene-owner mutation, TSCN/PCK rewrite, or new gameplay action is authorized.

## Boundary

Formal physical authority remains through Step 57 4/4/frozen. Physical 0.0.200 is diagnostic evidence at Step 59, not closure. Steps 60–62 remain gated on a clean original Step-59 transition. Character selection/confirm/embark/run start, continuous interactive ownership, and Step 63+ remain unopened.
