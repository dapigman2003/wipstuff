# StS2 Launcher — Step 38.0

Active candidate: **0.0.162 (162)** — controlled `NGame._EnterTree()` execution after physical Step-37 closure.

Physical **0.0.161** closed Step 37.0.1 at **4/4**. The exact receipt-backed `res://scenes/game.tscn` matched its 10,414-byte SHA-256/MD5 seal, the private three-edit FMOD-neutral derivative loaded as `Godot.PackedScene`, and the real managed `MegaCrit.Sts2.Core.Nodes.NGame` hierarchy instantiated off-tree. `IsInsideTree` remained false and Gate C/D recorded zero resolver/host/private/initializer/rejected/native deltas before normal `RUN_END`.

Step 38 freezes that scene authority and advances only one lifecycle method. Gate A maps exact `NGame._EnterTree`, `_Ready`, `GameStartupWrapper`, `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, and `LoadDeferredStartupAssetsAsync` IL/callsites from the selected ModelDb-bootstrap compatibility image. Execution is refused if `_EnterTree`'s same-`NGame` call closure reaches a later startup/deferred method or OneTimeInitialization re-entry.

Gate B re-instantiates the proven FMOD-neutral scene off-tree. Gate C invokes exact `NGame._EnterTree()` **once by MethodInfo while the node is still off-tree**. Gate D requires the node to remain off-tree, OneTimeInitialization state to remain `2`, and initializer-bearing/rejected/native activity to remain zero before releasing the temporary node.

`SceneTree.AddChild`, automatic/direct `_Ready`, `_ExitTree`, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization, FMOD/Spine/Sentry native GDExtensions, gameplay, runtime Harmony/MonoMod, arbitrary resolver fallback, retry after Gate-C invocation starts, and state reset remain forbidden.

## Physical test sequence

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 **MODEL-BOOTSTRAP** once; require 4/4.
3. Step 36.0.5 once; require 4/4.
4. Step 37.0.1 once; require 4/4.
5. Step 38.0 Gates A-D once. If Gate C begins and fails, use a fresh process before retry.
6. Preserve the Step38 checkpoint/lifecycle-static-map/final-report artifacts.

Codemagic workflow key remains `ios-canonical`; existing NuGet, .NET, Godot Step-15, and iOS intermediate cache paths are unchanged.
