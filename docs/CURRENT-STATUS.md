# Current status

## Active candidate — Step 39.0 / 0.0.166 (166)

Physical **0.0.159** closed Step 36.0.5 unchanged `ExecuteEssential()` at 4/4. Physical **0.0.161** closed Step 37.0.1 at 4/4: the exact 10,414-byte `game.tscn` authority loaded through the three-edit FMOD-neutral copy and a real managed `NGame` hierarchy instantiated off-tree with zero resolver/native escape.

Physical **0.0.165 / Step 38.2** is now closed **4/4**. The selected derivative retained the proven ModelDb bootstrap, exact inert `GameStartupWrapper`, and exact Sentry/file-drop-window `_EnterTree` suppressions. Gate A verified zero remaining Sentry/GetWindow/FilesDropped/Connect refs; Gate B recreated the real NGame off-tree; Gate C returned from the verified `_EnterTree()` with `IsInsideTree=false`, state `2`, and all resolver/host/private/initializer/rejected/native deltas zero; Gate D released the node without `_ExitTree`.

A targeted no-install Step-39 PCK preflight then sealed four small resources: `audio_manager_proxy.gd.remap`, `audio_manager_proxy.gdc`, `reaction_wheel.tscn`, and `multiplayer_timeout_overlay.tscn`. The Godot 4.5.1 tokenized GDScript is exact tokenizer version 101 with a 9,828-byte decompressed token buffer and 69 audited identifiers. Those identifiers are FMOD command-proxy surface but contain no `_enter_tree`, `_ready`, or `_process` lifecycle callback identifier. The two nested scenes contain no FMOD/Spine/Sentry/Steam/GDExtension declaration.

**0.0.166 / Step 39.0** therefore advances to one real `SceneTree.Root.AddChild(NGame)` after a pre-insertion audit of the actual hierarchy. Gate B requires `NGame.Instance == null`, then maps selected-sts2 `_EnterTree`, `_Ready`, and `_Notification` methods for actual node types and their in-module managed base classes; any direct later-startup/native/platform edge fails closed. Gate C authorizes only the real AddChild/automatic audited lifecycle and requires exact singleton/parent/window/state authority with zero initializer/rejected/native escape. The caller immediately invokes `StopRendering()` when Gate C returns. Gate D verifies the attached hierarchy while frozen and intentionally leaves it in-tree.

## Physical sequence for 0.0.166

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → 4/4.
3. Step 36.0.5 → 4/4.
4. Step 37.0.1 → 4/4.
5. **Skip Step 38 in this process.**
6. Step 39.0 A-D once. If Gate C starts, do not retry in-process.
7. Preserve Step35/Step36/Step37/Step39 reports and relaunch before unrelated testing.

Still forbidden: explicit `_ExitTree`, RemoveChild/Free, render restart, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization, native FMOD/Spine/Sentry extensions, gameplay, state reset, and mutation of the trusted Step-12 install.
