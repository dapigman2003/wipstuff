# StS2 Launcher — Steps 58–64 character-select admission/render continuation

Active candidate: **0.0.190 (190)**. Physical runtime authority is closed through **Step 57 4/4/frozen**. Steps 58–64 are the active character-select continuation; Step 65 is unopened.

Use **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** from a fresh process. It remains intentionally capped at Step 52 and ends frozen. Then run **53 → 54 → 55 → 56 → 57 → 58 → 59 → 60 → 61 → 62 → 63 → 64** manually, stopping at the first failure.

The new 0.0.190 block follows the exact Step-56 `OpenCharacterSelect(NButton)` body without invoking that handler:

- **58** maps exact `NSubmenuStack.GetSubmenuType<NCharacterSelectScreen>()`; no invocation.
- **59** one-shot invokes only that exact factory while frozen, retains the real screen off-tree, and audits its actual off-tree hierarchy/lifecycle.
- **60** maps exact zero-arg `void NCharacterSelectScreen.InitializeSingleplayer()`; no invocation.
- **61** one-shot invokes only `InitializeSingleplayer()` on the retained off-tree screen while frozen and requires zero escape/drift.
- **62** maps exact `void NSubmenuStack.Push(NSubmenu)` and the initialized screen's tree-entry lifecycle; no push.
- **63** one-shot invokes only exact `Push` while frozen and requires the retained character-select screen visible/in-tree under the exact stack.
- **64** audits the actual in-tree character-select frame/input surface, persists the map, performs one bounded **750 ms** render residency, and synchronously refreezes before success evaluation.

Steps **59, 61, 63, and 64 are one-shot**. Never retry an armed rung in-process. Original `OpenCharacterSelect`, whole `GameStartup`, original `LaunchMainMenu`, cloud/migration/platform/Steam/deferred startup, character choice/confirm/embark/run-start, native game extensions, trusted-install mutation, and Step 65 remain unopened.

Physical Step 57 evidence is preserved in `docs/history/reports/`: exact resource `res://scenes/screens/character_select_screen.tscn`, 16,015 bytes, SHA-256 `3d2305fdddae6f432e52ac4b8ea5035c5cecfd26fbc30c319f09b1cd4a05eb1e`, with zero textual Spine/FMOD/GDExtension/dylib/dll risk tokens.

Authoritative status/device sequence: `docs/CURRENT-STATUS.md`.
