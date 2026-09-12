# Step 58.0 pre-existing character-select cache correction — 0.0.191

Physical 0.0.190 reached Step 58 Gate A from the fully closed Step-57 baseline, then stopped safely at Gate B because the launcher required `NMainMenuSubmenuStack._characterSelectSubmenu` to be null. The real retained stack instead already held a non-null character-select cache. No character-select factory invocation occurred, rendering remained stopped, and the run ended normally.

The game XML documentation describes `NMainMenuSubmenuStack` as lazily spawning submenus only when requested. Therefore a non-null `_characterSelectSubmenu` is runtime evidence that the real game has already requested/cached the character-select screen before Step 58; it is not authority to create a replacement.

0.0.191 corrects the continuation by treating the cache as state to characterize rather than requiring null:

- Step 58 still never invokes `GetSubmenuType<NCharacterSelectScreen>()`.
- If `_characterSelectSubmenu` is null, the original null-cache path remains admissible.
- If it is non-null, Step 58 accepts it only when it is the exact `NCharacterSelectScreen` from the selected private load context, still off-tree, and its inherited `NSubmenu._stack` field is reference-identical to the retained `NMainMenuSubmenuStack`.
- Step 59 then **reuses and audits** that exact off-tree cache without invoking the factory. The factory is one-shot only on the null-cache path.
- Steps 60–64 remain separately gated exactly as before: `InitializeSingleplayer` map/execution, `Push` map/execution, then bounded character-select rendering/refreeze.

This correction moves the launcher closer to normal game ownership: it adopts real game state instead of manufacturing a duplicate object merely to satisfy the harness. Physical authority remains closed through Step 57; Step 58 remains the active frontier until the corrected cache-state proof closes on-device.
