# StS2 Launcher — Step 39.0

Active candidate: **0.0.166 (166)** — first real Godot `SceneTree` admission.

Physical **0.0.159** closed Step 36.0.5 at 4/4 with unchanged full `ExecuteEssential()`. Physical **0.0.161** closed Step 37.0.1 at 4/4 with sealed FMOD-neutral `game.tscn` load and real off-tree `NGame` construction. Physical **0.0.165** closed Step 38.2 at 4/4: the verified iOS/off-tree-compatible `NGame._EnterTree()` returned once while `IsInsideTree == false`, state remained `2`, and resolver/initializer/rejected/native deltas were all zero.

Step 39.0 moves to the first **real** Godot lifecycle boundary. It re-verifies the Step-38.2 selected compatibility image and four exact PCK preflight resources, creates a fresh real `NGame` with `NGame.Instance == null`, resolves the live `SceneTree.Root`, enumerates the actual instantiated hierarchy, and Cecil-audits the selected-sts2 `_EnterTree`, `_Ready`, and `_Notification` implementations for every actual node type plus its in-module managed base chain. Any direct OneTimeInitialization, GameStartup/platform/main-menu/deferred, FMOD, Spine, Sentry, or Steam lifecycle edge is rejected before insertion.

Gate C performs exactly one `SceneTree.Root.AddChild(NGame)`. This admits Godot automatic enter/ready callbacks for the audited hierarchy while the proven `GameStartupWrapper` remains inert (`Task.CompletedTask`). The gate requires `IsInsideTree == true`, exact `NGame.Instance`, parent `SceneTree.Root`, a non-null `NGame._window` as `_Ready` evidence, state `2`, and zero initializer/rejected/native escape.

Immediately when Gate C returns, the iOS caller synchronously stops the Godot render loop and never restarts it in Step 39. Gate D verifies the inserted hierarchy while rendering is frozen. `RemoveChild`, `Free`, explicit `_ExitTree`, render restart, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization, native FMOD/Spine/Sentry GDExtensions, and gameplay remain forbidden.

## Physical test sequence

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 **MODEL-BOOTSTRAP** once; require 4/4.
3. Step 36.0.5 once; require 4/4.
4. Step 37.0.1 once; require 4/4.
5. **Skip Step 38 in this process.** It is prior physical evidence and its synthetic `_EnterTree()` intentionally leaves the singleton established without `_ExitTree`.
6. Step 39.0 Gates A-D once. After Gate C starts, preserve reports and relaunch rather than retrying.
7. Preserve Step35/Step36/Step37/Step39 run-correlated reports.

Codemagic workflow key remains `ios-canonical`; existing NuGet, .NET, Godot Step-15, and iOS intermediate cache paths are unchanged.
