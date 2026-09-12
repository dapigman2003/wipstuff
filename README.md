# StS2 Launcher — Steps 58–62 real character-select ownership / visible-render trial

Active candidate: **0.0.193 (193)**. Physical runtime authority is closed through **Step 57 4/4/frozen**. Physical 0.0.190 proved the real character-select cache may already exist; physical 0.0.191 proved that same real `NCharacterSelectScreen` may already be inside the live SceneTree before Step 58 performs any new operation. This candidate adopts that game-owned state instead of recreating its lifecycle. **Step 63 is unopened.**

Use **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** from a fresh process. It remains capped at Step 52 and ends frozen. Then manually reconstruct **53 → 54 → 55 → 56 → 57**, followed by **58 → 59 → 60 → 61 → 62**, stopping at the first failure.

The 0.0.193 block intentionally pivots away from launcher-owned create/init/push micromanagement:

- **58 — runtime ownership audit:** bind the exact real `OpenCharacterSelect(NButton)` handler, prove from selected IL that its button parameter is unused, audit the whole handler frontier under the retained runtime guards, and observe the real `_characterSelectSubmenu` as absent/off-tree/in-tree without mutation.
- **59 — real handler transition:** if the exact screen is already visible/in-tree, adopt it and do not invoke the handler. Otherwise invoke the exact real `OpenCharacterSelect(NButton)` **once** while rendering is frozen, letting the game own its normal cache/create → `InitializeSingleplayer()` → `Push()` transition. Success requires the exact cached `NCharacterSelectScreen` visible/in-tree under the retained submenu stack with zero drift.
- **60 — active-screen surface audit:** audit the actual visible/in-tree character-select frame/input callbacks and persist a connection inventory from the exact receipt-backed character-select TSCN. No rendering or interaction.
- **61 — short visible render:** one bounded **2-second** real render residency followed by synchronous refreeze. No intentional interaction.
- **62 — sustained visible render:** one bounded **10-second** real render residency followed by synchronous refreeze. No intentional interaction. If this closes physically, the next candidate may consider continuous/interactive Godot ownership rather than another screen-by-screen ladder.

Steps **59, 61, and 62** are one-shot only when their respective action is actually armed; never retry an armed boundary in-process. Character choice/confirm/embark/run-start, whole `GameStartup`, original `LaunchMainMenu`, cloud/migration/platform/Steam/deferred startup, native game GDExtensions, trusted-install mutation, and **Step 63+** remain unopened.

Physical Step 57 authority remains exact resource `res://scenes/screens/character_select_screen.tscn`, 16,015 bytes, SHA-256 `3d2305fdddae6f432e52ac4b8ea5035c5cecfd26fbc30c319f09b1cd4a05eb1e`, with zero textual Spine/FMOD/GDExtension/dylib/dll risk tokens.

Authoritative status/device sequence: `docs/CURRENT-STATUS.md`.
