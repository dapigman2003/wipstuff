# Documentation index — active Steps 58–62 / 0.0.195

Active candidate: **0.0.195 (195)**. Physical authority is closed through Step 57. Physical **0.0.194** proved the retained single-player submenu was already logically bound and the real standard button was supplied before original `OpenCharacterSelect`, yet the original handler still threw `NullReferenceException`.

Static analysis now proves the cached `NCharacterSelectScreen` being hidden, in-tree, parented by the main-menu stack, and logically unbound is the game's normal preload state. 0.0.195 therefore adds no guessed character-select repair. Step 59 first checkpoints scene readiness, critical character/ascension fields, NGame service nodes, SaveManager progress internals, and RootSceneContainer ownership. On handler failure it records the original inner game TargetSite/stack and post-failure lobby/player/screen state. Steps 60–62 remain unavailable unless the real handler succeeds. Step 63 remains unopened.

Primary authority:

- `CURRENT-STATUS.md` — current physical frontier and exact device sequence.
- `MASTER-PLAN.md` — product objective, authority model, and roadmap.
- `TESTING.md` — static/Codemagic/device procedure for 0.0.195.
- `RELEASE-CHECKLIST.md` — release-quality and packaging requirements.
- `REPORTS.md` — evidence surface contracts.
- `REGRESSION-CONTRACTS.md` — protected historical contracts.
- `history/INDEX.md` — evidence/design provenance.

Physical 0.0.190–0.0.192 established that the real character-select cache may already exist, be in-tree, and still be logically unbound. Physical 0.0.193 then reached real `OpenCharacterSelect` and failed immediately after arm; the selected handler IL does not read the explicit button argument and begins by dereferencing `NSingleplayerSubmenu._stack`. 0.0.195 restores that skipped game-owned navigation state narrowly, then continues to active-screen audit and the 2-second/10-second visible render trials.
