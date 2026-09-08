# Current status

## Active candidate — Step 38.2 / 0.0.165 (165)

Physical **0.0.159** closed unchanged `ExecuteEssential()` at Step 36.0.5 4/4. Its proven ModelDb compatibility plan remains the `1624/56/47/41` graph.

Physical **0.0.161** closed Step 37.0.1 at **4/4**. The exact 10,414-byte `game.tscn` authority loaded through the copied FMOD-neutral derivative and a real managed `NGame` hierarchy instantiated off-tree with zero resolver/native escape.

Physical **0.0.162** stopped safely in Step 38 Gate A because Cecil tried to resolve GodotSharp. Physical **0.0.163** corrected that to deferred rejecting-resolver reading and proved exact `NGame._EnterTree()` directly reaches `GameStartupWrapper()`.

Physical **0.0.164 / Step 38.1** then proved the inert-startup-wrapper derivative itself is valid. Gate A passed with the wrapper serialized as exact `call Task.get_CompletedTask; ret`; Gate B recreated the real FMOD-neutral `NGame` off-tree; Gate C physically entered `_EnterTree()` and failed with a managed `NullReferenceException`. At failure `IsInsideTree` was still false and initializer-bearing/rejected/native deltas remained zero. The static map shows the two remaining environment-sensitive edges in the off-tree body: early `SentryService.Initialize()` and the later `GetWindow()` / `Window.SignalName.FilesDropped` / `GodotObject.Connect(...)` hookup.

**0.0.165 / Step 38.2** keeps the proven ModelDb rewrite, FMOD-neutral scene, and inert `GameStartupWrapper`. It adds only two exact stack-neutral `_EnterTree` compatibility suppressions before CLR admission:

1. replace the single `_EnterTree -> SentryService.Initialize()` call with `nop` because Sentry is intentionally disabled in the iOS compatibility host;
2. replace only the bounded `GetWindow -> FilesDropped -> Connect -> pop` block with `nop` instructions because the diagnostic instance is deliberately off-tree and therefore has no containing SceneTree Window.

The transform requires exactly one original Sentry call, exactly one original window/file-drop block, one `FilesDropped` signal field, and no external branch into the bounded block. Serialized verification requires the original `_EnterTree` instruction count to be preserved, the exact expected NOP delta, zero remaining Sentry/GetWindow/FilesDropped/Connect references, and one preserved direct call to the verified inert `GameStartupWrapper`.

Gate A re-verifies all of that with deferred rejecting Cecil. Gate B recreates the proven scene off-tree. Gate C invokes the verified Step-38.2 `_EnterTree` body once. Gate D re-proves off-tree/state-2 confinement and zero initializer-bearing/rejected/native escape, then releases the node without `_ExitTree`.

## Physical sequence for 0.0.165

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → 4/4. This run creates the ModelDb + inert-wrapper + Step-38.2 iOS/off-tree lifecycle derivative.
3. Step 36.0.5 → 4/4.
4. Step 37.0.1 → 4/4.
5. Step 38.2 A-D once. Do not retry in-process after Gate C starts.
6. Preserve Step35/Step36/Step37/Step38 run-correlated reports.

Still forbidden: real SceneTree insertion, `_Ready`, `_ExitTree`, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization, native FMOD/Spine/Sentry extensions, gameplay, broad native compatibility, state reset, and mutation of the trusted Step-12 install.
