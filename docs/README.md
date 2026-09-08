# StS2 Launcher — Step 39.0

Active candidate: **0.0.167 (167)** — first real Godot `SceneTree` admission; compile-only correction to the 0.0.166 Step-39 candidate.

Physical Codemagic **0.0.166** passed 1051/1051 static checks, 233/233 host tests, and the Step-15 native-link preflight, then stopped at iOS C# compile because the new Step-39 RootViewController partial omitted `using StS2Launcher.iOS.Platform;`. No Step-39 runtime boundary was reached. **0.0.167** adds only that import plus provenance/regression guards; SceneTree semantics are unchanged.

Physical **0.0.159** closed Step 36.0.5 at 4/4. Physical **0.0.161** closed Step 37.0.1 at 4/4. Physical **0.0.165** closed Step 38.2 at 4/4: the iOS/off-tree-compatible `NGame._EnterTree()` returned with state 2 and zero resolver/initializer/rejected/native deltas.

Step 39.0 re-verifies the exact Step-38.2 compatibility image and four sealed PCK preflight resources, creates a fresh real `NGame` with `NGame.Instance == null`, resolves the live `SceneTree.Root`, and audits the actual instantiated hierarchy's selected-sts2 `_EnterTree`, `_Ready`, and `_Notification` implementations plus their in-module managed helper/base-chain closure. Any later-startup/native/platform edge fails closed before insertion.

Gate C performs exactly one real `SceneTree.Root.AddChild(NGame)`, authorizing only the audited automatic Godot enter/ready lifecycle while `GameStartupWrapper` remains the verified inert `Task.CompletedTask` body. Immediately when Gate C returns, the launcher synchronously stops the Godot render loop and never restarts it during Step 39. Gate D verifies the attached hierarchy while frozen. The inserted NGame is intentionally retained in-tree.

`RemoveChild`, `Free`, explicit `_ExitTree`, render restart, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization, native FMOD/Spine/Sentry GDExtensions, and gameplay remain forbidden.

## Physical test sequence

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 **MODEL-BOOTSTRAP** once; require 4/4.
3. Step 36.0.5 once; require 4/4.
4. Step 37.0.1 once; require 4/4.
5. **Skip Step 38 in this process.**
6. Step 39.0 Gates A-D once. After Gate C starts, preserve reports and relaunch rather than retrying.
7. Preserve Step35/Step36/Step37/Step39 run-correlated reports.

Codemagic workflow key remains `ios-canonical`; existing NuGet, .NET, Godot Step-15, and iOS intermediate cache paths are unchanged.
