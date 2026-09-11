# StS2 Launcher — Steps 53–57 single-player submenu continuation

Active candidate: **0.0.187 (187)**. Physical runtime authority is now closed through **Step 55 4/4/refrozen**; Step 56–57 remain the active non-invoking/read-only character-select frontier and Step 58+ stays unopened.

Use the existing **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** button from a fresh process to reconstruct the known authority. It still stops at Step 52 with rendering frozen. Then run Steps **53 → 54 → 55 → 56 → 57** manually, stopping at the first failure.

The new ladder is intentionally narrow:

- **Step 53** isolates exact zero-arg `NMainMenu.OpenSingleplayerSubmenu()` returning `NSingleplayerSubmenu` and requires its execution-qualified frontier to be admissible without invoking it. The broader `SingleplayerButtonPressed` remains uninvoked.
- **Step 54** token-matches that exact runtime method, binds `NMainMenu.SubmenuStack` plus its lazy `NMainMenuSubmenuStack._singleplayerSubmenu` slot, writes the binding map, then invokes only `OpenSingleplayerSubmenu()` once while rendering is frozen. The returned `NSingleplayerSubmenu` must be the exact stack-retained object and become visible/in-tree with zero context/native drift.
- **Step 55** audits the actual visible submenu frame/input callbacks, writes the map, then performs one bounded **750 ms** render residency and synchronously refreezes before telemetry.
- **Step 56** maps exact `void NSingleplayerSubmenu.OpenCharacterSelect(NButton)` plus execution/deferred frontiers and `res://` resource literals. It never invokes that method. Physical 0.0.186 proved the exact one-parameter callback shape before invocation.
- **Step 57** reads only the discovered exact character-select `.tscn` entry from the receipt-backed PCK, validates PCK MD5/SHA-256, lists referenced resources, and reports textual Spine/FMOD/GDExtension/native-risk tokens. No `ResourceLoader`, `PackedScene`, character-select invocation, or rendering occurs.

The 0.0.183 Codemagic AOT-cache telemetry/caches are retained unchanged. If a future Codemagic run is available, its cache report can be compared without changing this runtime experiment.

Still forbidden here: whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, external/native Steam, deferred startup/`ExecuteDeferred`, character-select loading/instantiation/admission, native FMOD/Spine game extensions, and trusted-install mutation.

Authoritative status and device sequence: `docs/CURRENT-STATUS.md`.
