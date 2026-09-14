# Documentation — StS2 Launcher Steps 58–62 current-ownership / character-select continuation

Active candidate: **0.0.203 (203)**. Physical history remains closed through **Step 57**, but the **active architecture now uses Step 52 as the prerequisite baseline for Step 58**. Legacy Steps 53–57 remain available as optional regression/history diagnostics; they are no longer mandatory gameplay choreography.

Use **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** from a fresh process. It ends frozen. Then run **Step 58 directly**. Step 58 Gate A adopts the current game-owned single-player submenu if it already exists, or invokes the exact game-owned `NMainMenu.OpenSingleplayerSubmenu()` once only when the retained submenu must be materialized/activated. It validates the retained character-select `PackedScene` and proceeds directly into the existing ownership audit. Do not manually replay 53→57 unless you specifically want those legacy diagnostics.

0.0.203 deliberately restores the **physically proven 0.0.201 runtime derivative**. `TransformedRealStS2VeryEarlyInitialization.cs` is byte-identical to 0.0.201, so the successful 0.0.199/0.0.200 `PackedScene.Instantiate()` unique-name compatibility correction remains unchanged. **0.0.202 is rejected**: its runtime-wide Step-59 IL tracing modified shared sts2 methods and physical testing regressed the old Step-54 route at `NSingleplayerSubmenu.RefreshButtons()`.

The same 0.0.203 IPA is designed to maximize information per expensive compilation. After Step 58 closes, use separate fresh processes for whichever Step-59 branch is most useful:

- **59D** — deep read-only retained-button/scene/IL/frontier deck; no handler.
- **59R** — controller/hotkey singleton and icon-lookup rehearsal; observational.
- **59H** — invokes only inherited `NButton.RegisterHotkeys()` on the retained embark button.
- **59U** — invokes only inherited `NButton.UpdateControllerButton()`.
- **59T** — executes only the post-base `NConfirmButton.OnEnable` visual/tween tail with durable PRE/POST/FAIL checkpoints around `Modulate`, `Kill`, `CreateTween`, `TweenProperty`, `SetEase`, `SetTrans`, and `FromCurrent`.
- **59E** — invokes the retained embark `Enable()` outside `OpenCharacterSelect` to reproduce the complete button path in isolation.
- **59** — the real one-shot game-owned `OpenCharacterSelect` transition with the existing preflight and post-failure snapshots.

59H/59U/59T/59E and the real Step 59 transition are fresh-process experiments. **No shared sts2 method is runtime-instrumented in 0.0.203.** Steps 60–62 remain locked until the real Step 59 transition succeeds. Character choice, confirm/embark, run start, continuous interactive ownership, and **Step 63+ remain unopened**.

Physical 0.0.201 evidence remains important: the retained embark `NConfirmButton` is in-tree and ready with `_outline`, `_buttonImage`, `_viewport`, `_hotkeys`, `_controllerHotkeyIcon`, and `_moveTween` populated immediately before and after the reproducible `NConfirmButton.OnEnable()` `NullReferenceException`. The 0.0.203 micro-probes are intended to distinguish the base hotkey/controller path from the confirm-button tween tail without spending another compilation for each hypothesis.

Authoritative status/device sequence: `docs/CURRENT-STATUS.md`.
