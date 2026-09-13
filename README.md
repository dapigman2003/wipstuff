# StS2 Launcher — Steps 58–62 real character-select ownership / visible-render trial

Active candidate: **0.0.194 (194)**. Physical runtime authority is closed through **Step 57 4/4/frozen**. Physical 0.0.193 additionally closed Step 58 4/4 and reached the real Step-59 handler, localizing a missing logical stack binding left by the earlier direct single-player-open shortcut. **Step 63 is unopened.**

Use **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** from a fresh process. It remains capped at Step 52 and ends frozen. Then manually reconstruct **53 → 54 → 55 → 56 → 57**, followed by **58 → 59 → 60 → 61 → 62**, stopping at the first failure.

The 0.0.194 block keeps the real-game ownership pivot and repairs only the missing navigation lifecycle exposed physically:

- **58 — runtime ownership audit:** bind/audit exact real `OpenCharacterSelect(NButton)` and observe the actual game-owned character-select cache/tree/logical-stack state without mutation.
- **59 — game-owned stack repair + real handler transition:** if the transition is already visibly and logically complete, adopt it. Otherwise inspect the retained real `NSingleplayerSubmenu._stack`. A foreign stack fails. If null, invoke exact audited `NSubmenuStack.Push(NSubmenu)` once on the retained stack with the retained real submenu and require exact logical binding. Then invoke original `OpenCharacterSelect(NButton)` once with the retained real `_standardButton`. No launcher code writes `_stack` directly.
- **60 — active-screen surface audit:** audit the actual visible/in-tree character-select frame/input callbacks and persist a connection inventory from the exact receipt-backed character-select TSCN. No rendering or interaction.
- **61 — short visible render:** one bounded **2-second** real render residency followed by synchronous refreeze. No intentional interaction.
- **62 — sustained visible render:** one bounded **10-second** real render residency followed by synchronous refreeze. No intentional interaction. If this closes physically, the next candidate may consider continuous/interactive Godot ownership rather than another screen-by-screen ladder.

Step **59** is one-shot once navigation repair/handler transition is armed; Steps **61/62** are one-shot after render arm. Character choice/confirm/embark/run-start, whole `GameStartup`, original `LaunchMainMenu`, cloud/migration/platform/Steam/deferred startup, native game GDExtensions, trusted-install mutation, and **Step 63+** remain unopened.

Physical Step 57 authority remains exact resource `res://scenes/screens/character_select_screen.tscn`, 16,015 bytes, SHA-256 `3d2305fdddae6f432e52ac4b8ea5035c5cecfd26fbc30c319f09b1cd4a05eb1e`, with zero textual Spine/FMOD/GDExtension/dylib/dll risk tokens.

Authoritative status/device sequence: `docs/CURRENT-STATUS.md`.
