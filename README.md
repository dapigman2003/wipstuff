# StS2 Launcher — Steps 58–62 character-select forensic localization / visible-render trial

Active candidate: **0.0.200 (200)**. Physical runtime authority remains formally closed through **Step 57 4/4/frozen**. Physical **0.0.198** proves the parent-scene `unique_name_in_owner=true` value survives TSCN parsing and `SceneState`, but is lost by `PackedScene.Instantiate()` specifically on roots of instanced subscenes. **Step 63 is unopened.**

Use **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** from a fresh process. It remains capped at Step 52 and ends frozen. Then manually reconstruct **53 → 54 → 55 → 56 → 57 → 58 → 59**, stopping at the first failure. Steps 60–62 are reached only if Step 59 closes cleanly.

0.0.200 retains the 0.0.199 global scene-compatibility correction and fixes its host-regression fixture/evidence contract before any device reproof. A private, hash-pinned GodotSharp derivative wraps `PackedScene.Instantiate()` and restores only parent-scene `unique_name_in_owner=true` overrides that belong to roots of instanced subscenes. The original TSCN/PCK and trusted GodotSharp bytes remain unchanged:

- **58 — runtime ownership audit:** bind/audit exact real `OpenCharacterSelect(NButton)` and observe the game-owned character-select preload state without mutation. Static analysis now treats hidden + in-tree + logical `_stack == null` as the normal pre-push state created by `NMainMenuSubmenuStack._Ready()`.
- **59 — forensic prerequisites + one real handler transition:** before invoking anything, require/prove `NCharacterSelectScreen.IsNodeReady()`, critical `_Ready`-bound fields, `NAscensionPanel.IsNodeReady()`, NGame hotkey/input/remote-cursor/reaction/timeout services, existing SaveManager `Progress/Epochs/EncounterStats`, and record `RootSceneContainer.CurrentScene` identity. If the original handler throws, durably checkpoint its inner `TargetSite` and original stack plus a post-failure lobby/player/screen snapshot. The existing exact game-owned `NSubmenuStack.Push` repair remains available only if the retained single-player logical stack is actually null; no field is written directly.
- **60 — active-screen surface audit:** available only after a clean original handler transition.
- **61 — short visible render:** bounded **2-second** real render residency + refreeze.
- **62 — sustained visible render:** bounded **10-second** real render residency + refreeze.

Step **59** is one-shot after handler transition arm. Do not retry it in-process. Character choice/confirm/embark/run-start, whole `GameStartup`, original `LaunchMainMenu`, cloud/migration/platform/Steam/deferred startup, native game GDExtensions, trusted-install mutation, and **Step 63+** remain unopened.

Physical Step 57 authority remains exact resource `res://scenes/screens/character_select_screen.tscn`, 16,015 bytes, SHA-256 `3d2305fdddae6f432e52ac4b8ea5035c5cecfd26fbc30c319f09b1cd4a05eb1e`, with zero textual Spine/FMOD/GDExtension/dylib/dll risk tokens.

Authoritative status/device sequence: `docs/CURRENT-STATUS.md`.


The retained 0.0.199 runtime correction never writes `_ascensionPanel`, `_actDropdown`, `TimeoutOverlay`, or any other game field, and never manually calls `_Ready()`. It changes only the missing Godot node metadata that the scene itself explicitly requested, before the instantiated scene is returned to its caller. Step 59 then retains the forensic snapshot as the post-correction proof before any real handler is armed.
