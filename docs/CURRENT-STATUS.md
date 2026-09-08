# Current status

## Active candidate — Step 38.1 / 0.0.164 (164)

Physical **0.0.159** closed unchanged `ExecuteEssential()` at Step 36.0.5 4/4. Its ModelDb compatibility plan is the proven `1624/56/47/41` graph and remains unchanged in the new derivative.

Physical **0.0.161** closed Step 37.0.1 at **4/4**. The exact 10,414-byte `game.tscn` authority loaded through the copied FMOD-neutral derivative, and a real managed `NGame` hierarchy instantiated off-tree with zero resolver/native escape.

Physical **0.0.162** stopped safely in Step 38 Gate A because Cecil's default/immediate reader tried to resolve GodotSharp. Physical **0.0.163** corrected that to deferred rejecting-resolver metadata-only reading and exposed the actual lifecycle boundary before any invocation: exact `NGame._EnterTree()` directly calls `NGame.GameStartupWrapper()`.

**0.0.164 / Step 38.1** does not weaken that guard. Instead it extends the already selected ahead-of-load compatibility derivative by rewriting only `NGame.GameStartupWrapper()` to the exact body `call Task.get_CompletedTask; ret`. `_EnterTree` itself is unchanged and must still contain exactly one direct call to the wrapper. The serialized derivative is reopened and fails closed unless the inert wrapper is exactly two instructions with no locals/handlers and the previously proven ModelDb rewrite remains valid.

Gate A then maps `_EnterTree`, `_Ready`, `GameStartupWrapper`, `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, and `LoadDeferredStartupAssetsAsync`. The verified inert wrapper is the only later-named method allowed in `_EnterTree`'s same-`NGame` closure; `_Ready`, `GameStartup`, platform/main-menu/deferred startup, and OneTimeInitialization re-entry remain hard failures.

Gate B re-instantiates the physically proven FMOD-neutral scene off-tree. Gate C invokes exact unchanged `_EnterTree()` once while `IsInsideTree == false`. Gate D re-proves off-tree/state-2 confinement and zero initializer-bearing/rejected/native escape, then releases the node without `_ExitTree`.

## Physical sequence for 0.0.164

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → 4/4. The selected derivative now also contains the separately verified inert startup wrapper.
3. Step 36.0.5 → 4/4.
4. Step 37.0.1 → 4/4.
5. Step 38.1 A-D once. Do not retry in-process after Gate C starts.
6. Preserve Step35/Step36/Step37/Step38 run-correlated reports.

Still forbidden: SceneTree insertion, `_Ready`, `_ExitTree`, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization, native FMOD/Spine/Sentry extensions, gameplay, broad native compatibility, state reset, and mutation of the trusted Step-12 install.
