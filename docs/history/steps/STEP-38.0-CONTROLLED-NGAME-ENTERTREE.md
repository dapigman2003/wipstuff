# Step 38.0 — controlled `NGame._EnterTree` entry

Physical 0.0.161 closes Step 37.0.1 at **4/4**. The exact receipt-backed `game.tscn` seal matched, the private three-edit FMOD-neutral derivative loaded as `Godot.PackedScene`, and the real managed `MegaCrit.Sts2.Core.Nodes.NGame` hierarchy instantiated off-tree. `IsInsideTree` remained false and the Step-37 run recorded zero resolver/host/private/initializer/rejected/native deltas before normal teardown.

Step 38 therefore advances only one lifecycle boundary while preserving the physically proven ModelDb/PCK/scene authority.

## Gate A — exact lifecycle static audit

Read the exact selected ModelDb-bootstrap compatibility image with Mono.Cecil and map:

- `NGame._EnterTree`
- `NGame._Ready`
- `NGame.GameStartupWrapper`
- `NGame.GameStartup`
- `NGame.InitializePlatform`
- `NGame.LaunchMainMenu`
- `NGame.LoadDeferredStartupAssetsAsync`

The exact `_EnterTree` method must be an instance, parameterless, managed-IL `void` method. Before any execution, Step 38 computes the same-`NGame` call closure reachable from `_EnterTree` and fails closed if it reaches `_Ready`, startup/platform/main-menu/deferred methods, or any `OneTimeInitialization.ExecuteVeryEarly/ExecuteEssential/ExecuteDeferred/PrewarmJit` re-entry.

## Gate B — fresh off-tree `NGame`

Reuse the already proven Step-37 `PackedScene` resource and instantiate a fresh copy with `GenEditState.Disabled`. Require the exact private-load-context `NGame` root and `IsInsideTree == false`, then bind the reflection `_EnterTree` token to the Cecil token from Gate A. No lifecycle method is called in Gate B.

## Gate C — one direct `_EnterTree` invocation

Invoke exactly `NGame._EnterTree()` once by `MethodInfo` while the instance is still off-tree. No `AddChild` or SceneTree insertion occurs, so Step 38 does not authorize Godot to invoke `_Ready` automatically. Capture complete nested exception/context telemetry on failure. On success require `IsInsideTree == false`, `OneTimeInitialization` state `2`, and zero initializer-bearing/rejected/native-load escape.

## Gate D — confinement and release

Re-prove the post-`_EnterTree` node remains off-tree and state remains `2`, then free the temporary node without calling `_ExitTree`.

## Still forbidden

`SceneTree.AddChild`, automatic `_Ready`, direct `_Ready`, `_ExitTree`, `GameStartup`, `GameStartupWrapper`, `InitializePlatform`, `LaunchMainMenu`, `ExecuteDeferred`, Steam initialization, FMOD/Spine/Sentry native GDExtension loading, gameplay, retry after Gate-C invocation begins, and any trusted Step-12 content mutation remain outside Step 38.
