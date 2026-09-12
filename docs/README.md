# Documentation index — active Steps 58–62 / 0.0.192

Active candidate: **0.0.192 (192)**. Physical authority is closed through Step 57. Physical 0.0.191 additionally proved that the real cached `NCharacterSelectScreen` can already be inside the live SceneTree before Step 58 performs a new operation. Steps 58–62 now test real game ownership and visible rendering; Step 63 remains unopened.

Primary authority:

- `CURRENT-STATUS.md` — current physical frontier and exact device sequence.
- `MASTER-PLAN.md` — product objective, authority model, and roadmap.
- `TESTING.md` — static/Codemagic/device procedure for 0.0.192.
- `RELEASE-CHECKLIST.md` — release-quality and packaging requirements.
- `REPORTS.md` — evidence surface contracts.
- `REGRESSION-CONTRACTS.md` — protected historical contracts.
- `history/INDEX.md` — evidence/design provenance.

Physical 0.0.189 closed Step 57 4/4/frozen. Physical 0.0.190 showed a pre-existing character-select cache; physical 0.0.191 showed that cache already in-tree while Step 58 itself created nothing and rendering remained frozen. 0.0.192 therefore delegates the transition to the real `OpenCharacterSelect(NButton)` handler only when needed, then audits and visibly renders the actual active screen rather than recreating its internal lifecycle.
