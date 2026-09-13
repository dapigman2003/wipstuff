# Documentation index — active Steps 58–62 / 0.0.194

Active candidate: **0.0.194 (194)**. Physical authority is closed through Step 57, and physical 0.0.193 additionally closed Step 58 4/4 before Step 59 localized the missing logical submenu-stack binding left by the earlier direct Step-54 single-player-open shortcut. Steps 58–62 retain real game ownership and visible rendering; Step 63 remains unopened.

0.0.194 does not write `_stack` or reconstruct character-select internals. If the retained real `NSingleplayerSubmenu._stack` is null, Step 59 may invoke exact audited game-owned `NSubmenuStack.Push(NSubmenu)` once, require reference-identical binding to the retained `NMainMenuSubmenuStack`, then invoke original `OpenCharacterSelect(NButton)` with the retained real `_standardButton`. A foreign non-null stack is fatal.

Primary authority:

- `CURRENT-STATUS.md` — current physical frontier and exact device sequence.
- `MASTER-PLAN.md` — product objective, authority model, and roadmap.
- `TESTING.md` — static/Codemagic/device procedure for 0.0.194.
- `RELEASE-CHECKLIST.md` — release-quality and packaging requirements.
- `REPORTS.md` — evidence surface contracts.
- `REGRESSION-CONTRACTS.md` — protected historical contracts.
- `history/INDEX.md` — evidence/design provenance.

Physical 0.0.190–0.0.192 established that the real character-select cache may already exist, be in-tree, and still be logically unbound. Physical 0.0.193 then reached real `OpenCharacterSelect` and failed immediately after arm; the selected handler IL does not read the explicit button argument and begins by dereferencing `NSingleplayerSubmenu._stack`. 0.0.194 restores that skipped game-owned navigation state narrowly, then continues to active-screen audit and the 2-second/10-second visible render trials.
