# StS2 Launcher — Steps 53–57 single-player submenu continuation

Active candidate: **0.0.188 (188)**. Physical runtime authority is now closed through **Step 56 4/4/frozen**; Step 57 is the active read-only character-select resource preflight and Step 58+ stays unopened.

Use the existing **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** button from a fresh process to reconstruct the known authority. It still stops at Step 52 with rendering frozen. Then run Steps **53 → 54 → 55 → 56 → 57** manually, stopping at the first failure.

The new ladder is intentionally narrow:

- **Step 53** isolates exact zero-arg `NMainMenu.OpenSingleplayerSubmenu()` returning `NSingleplayerSubmenu` and requires its execution-qualified frontier to be admissible without invoking it. The broader `SingleplayerButtonPressed` remains uninvoked.
- **Step 54** token-matches that exact runtime method, binds `NMainMenu.SubmenuStack` plus its lazy `NMainMenuSubmenuStack._singleplayerSubmenu` slot, writes the binding map, then invokes only `OpenSingleplayerSubmenu()` once while rendering is frozen. The returned `NSingleplayerSubmenu` must be the exact stack-retained object and become visible/in-tree with zero context/native drift.
- **Step 55** audits the actual visible submenu frame/input callbacks, writes the map, then performs one bounded **750 ms** render residency and synchronously refreezes before telemetry.
- **Step 56** maps exact `void NSingleplayerSubmenu.OpenCharacterSelect(NButton)` plus execution/deferred frontiers without invocation and retains the exact observed character-select scene hint (`screens/character_select_screen` or its exact full `res://` form). Physical 0.0.187 closed this rung 4/4/frozen before Step 57 began.
- **Step 57** canonicalizes only that exact managed scene identity to `res://scenes/screens/character_select_screen.tscn`, requires exactly one matching entry in the receipt-backed PCK directory, then reads only that exact entry, validates PCK MD5/SHA-256, lists referenced resources, and reports textual Spine/FMOD/GDExtension/native-risk tokens. Physical 0.0.187 localized the old full-path-only discovery bug at Gate A with `observed=none`; no `ResourceLoader`, `PackedScene`, character-select invocation, or rendering occurred.

The 0.0.183 Codemagic AOT-cache telemetry/caches are retained unchanged. If a future Codemagic run is available, its cache report can be compared without changing this runtime experiment.

Still forbidden here: whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, external/native Steam, deferred startup/`ExecuteDeferred`, character-select loading/instantiation/admission, native FMOD/Spine game extensions, and trusted-install mutation.

Authoritative status and device sequence: `docs/CURRENT-STATUS.md`.
