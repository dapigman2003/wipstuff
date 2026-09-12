# Step 58.0 runtime-ownership pivot — 0.0.192

Physical 0.0.191 (191) re-proved Step 57 authority and stopped safely at Step 58 Gate B before any new character-select operation. The exact pre-existing `_characterSelectSubmenu` was already inside the live `SceneTree`; rendering was frozen and Step 58 neither created character select nor restarted rendering.

That result invalidates the remaining 0.0.191 assumption that launcher-owned progress should be modeled as cache creation -> off-tree InitializeSingleplayer -> Push admission. 0.0.192 therefore pivots to the actual game-owned state:

- Step 58 accepts absent, off-tree, hidden in-tree, or already-visible exact character-select cache state and records it rather than predicting one state.
- Step 58 re-audits the exact original `OpenCharacterSelect(NButton)` handler and requires the selected IL to prove the `NButton` parameter is unused before any null-button invocation can be authorized.
- Step 59 invokes that original handler once only when the transition is not already visibly complete. If the real game already owns a visible/in-tree screen, Step 59 adopts it without duplicate invocation.
- Step 60 audits the actual active character-select subtree/frame/input surface and inventories exact TSCN signal-connection records.
- Step 61 renders the real screen for a short 2-second observation residency and synchronously refreezes.
- Step 62 renders the real screen for a sustained 10-second observation residency and synchronously refreezes.
- Deliberate character-select interaction, continuous ownership, character choice/confirm/embark/run start, and Step 63 remain unopened.

Physical authority remains closed through Step 57. The 0.0.191 Step-58 stop did not consume a mutating or one-shot boundary.
