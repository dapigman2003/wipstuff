# Step 39.0 — Real SceneTree admission

Candidate: 0.0.166 (166).

Physical 0.0.165 closed Step 38.2 at 4/4: the verified iOS/off-tree-compatible `NGame._EnterTree()` returned once on a real FMOD-neutral `NGame`, remained off-tree, kept `OneTimeInitialization` at state 2, and produced zero resolver/initializer/rejected/native deltas.

Step 39 is the first real Godot lifecycle admission. It intentionally does **not** rerun Step 38 in the same process, because the synthetic Step-38 `_EnterTree()` established `NGame.Instance` without `_ExitTree`. Fresh-process authority is therefore Step 15 -> Step 35 -> Step 36 -> Step 37 -> Step 39, skipping Step 38.

Gate A re-verifies the exact Step-38.2 selected compatibility image and four hash-pinned PCK preflight resources. Gate B instantiates a fresh NGame off-tree, requires `NGame.Instance == null`, resolves the live `Engine.GetMainLoop()` / `SceneTree.Root`, enumerates the actual node hierarchy, and maps all selected-sts2 `_EnterTree`, `_Ready`, and `_Notification` implementations on each actual node type and its in-module managed base chain. Any direct OneTimeInitialization, GameStartup/platform/main-menu/deferred, FMOD, Spine, Sentry, or Steam reference is rejected before insertion.

Gate C performs exactly one `SceneTree.Root.AddChild(NGame)`. This intentionally admits Godot automatic enter/ready lifecycle for the audited hierarchy while `GameStartupWrapper` remains the verified inert `Task.CompletedTask` body. The gate requires `IsInsideTree == true`, exact `NGame.Instance`, exact SceneTree-root parent, non-null `NGame._window` as `_Ready` evidence, state 2, and zero initializer/rejected/native escape.

Immediately when Gate C returns, the iOS caller synchronously invokes the already-proven Step-15 `StopRendering()` bridge and never restarts rendering. Gate D requires the frozen inserted hierarchy to remain authoritative. Step 39 does not RemoveChild, Free, call `_ExitTree`, restart rendering, enter GameStartup/InitializePlatform/LaunchMainMenu/ExecuteDeferred, initialize Steam, load native game GDExtensions, or enter gameplay.
