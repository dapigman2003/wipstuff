# Steps 58–64 — character-select admission/render continuation

Candidate 0.0.190 packages seven separately gated rungs on top of physically closed Step-57 authority. **Historical note:** physical 0.0.190 later showed the Step-58 assumption that `_characterSelectSubmenu` would still be null was false. 0.0.191 supersedes only that cache/acquisition assumption: an exact pre-existing off-tree cache is adopted rather than recreated; the remaining rung boundaries stay separate. The design follows the exact Step-56 `OpenCharacterSelect(NButton)` IL but does **not** invoke that handler.

- **58 — factory frontier:** bind/map the exact closed generic `NSubmenuStack.GetSubmenuType<NCharacterSelectScreen>()`; no invocation.
- **59 — frozen off-tree creation:** one-shot invoke only that exact factory while rendering is stopped; require exact retained `NCharacterSelectScreen` identity and audit its actual off-tree node/lifecycle surface.
- **60 — initialization frontier:** bind/map exact zero-argument `void NCharacterSelectScreen.InitializeSingleplayer()`; no invocation.
- **61 — frozen initialization:** one-shot invoke only `InitializeSingleplayer()` on the retained off-tree screen; require it remains off-tree and confined with zero resolver/native escape.
- **62 — push frontier:** bind/map exact `void NSubmenuStack.Push(NSubmenu)` plus the initialized character-select tree-entry lifecycle; no push.
- **63 — frozen SceneTree admission:** one-shot invoke only exact `Push` with the retained initialized screen; require exact stack parent, visible/in-tree authority, renderer still stopped, and zero drift.
- **64 — render residency:** audit actual in-tree character-select frame/input callbacks, persist the map, then run one 750 ms bounded render residency and synchronously refreeze before telemetry. No character selection, confirm/embark, or run-start action is opened.

Every later rung requires prior same-process 4/4 authority. Steps 59, 61, 63, and 64 are one-shot and must never be retried in-process after arm. Step 65 is deliberately absent.
