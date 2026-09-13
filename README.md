# StS2 Launcher — Steps 58–62 character-select forensic localization / visible-render trial

Active candidate: **0.0.195 (195)**. Physical runtime authority is closed through **Step 57 4/4/frozen**. Physical **0.0.194** reached the original Step-59 `OpenCharacterSelect(NButton)` handler with the retained `NSingleplayerSubmenu` already bound to the exact stack and the real `_standardButton` supplied, yet the handler still raised `NullReferenceException`. **Step 63 is unopened.**

Use **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** from a fresh process. It remains capped at Step 52 and ends frozen. Then manually reconstruct **53 → 54 → 55 → 56 → 57 → 58 → 59**, stopping at the first failure. Steps 60–62 are reached only if Step 59 closes cleanly.

The 0.0.195 block is deliberately forensic rather than reparative:

- **58 — runtime ownership audit:** bind/audit exact real `OpenCharacterSelect(NButton)` and observe the game-owned character-select preload state without mutation. Static analysis now treats hidden + in-tree + logical `_stack == null` as the normal pre-push state created by `NMainMenuSubmenuStack._Ready()`.
- **59 — forensic prerequisites + one real handler transition:** before invoking anything, require/prove `NCharacterSelectScreen.IsNodeReady()`, critical `_Ready`-bound fields, `NAscensionPanel.IsNodeReady()`, NGame hotkey/input/remote-cursor/reaction/timeout services, existing SaveManager `Progress/Epochs/EncounterStats`, and record `RootSceneContainer.CurrentScene` identity. If the original handler throws, durably checkpoint its inner `TargetSite` and original stack plus a post-failure lobby/player/screen snapshot. The existing exact game-owned `NSubmenuStack.Push` repair remains available only if the retained single-player logical stack is actually null; no field is written directly.
- **60 — active-screen surface audit:** available only after a clean original handler transition.
- **61 — short visible render:** bounded **2-second** real render residency + refreeze.
- **62 — sustained visible render:** bounded **10-second** real render residency + refreeze.

Step **59** is one-shot after handler transition arm. Do not retry it in-process. Character choice/confirm/embark/run-start, whole `GameStartup`, original `LaunchMainMenu`, cloud/migration/platform/Steam/deferred startup, native game GDExtensions, trusted-install mutation, and **Step 63+** remain unopened.

Physical Step 57 authority remains exact resource `res://scenes/screens/character_select_screen.tscn`, 16,015 bytes, SHA-256 `3d2305fdddae6f432e52ac4b8ea5035c5cecfd26fbc30c319f09b1cd4a05eb1e`, with zero textual Spine/FMOD/GDExtension/dylib/dll risk tokens.

Authoritative status/device sequence: `docs/CURRENT-STATUS.md`.
