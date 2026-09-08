# StS2 Launcher — Step 38.1

Active candidate: **0.0.164 (164)** — verified inert `NGame.GameStartupWrapper` plus controlled unchanged `NGame._EnterTree()` execution.

Physical **0.0.159** closed Step 36.0.5 at 4/4, including unchanged full `ExecuteEssential()`. Physical **0.0.161** closed Step 37.0.1 at 4/4: the sealed FMOD-neutral `game.tscn` loaded as `PackedScene` and the real managed `NGame` hierarchy instantiated off-tree with zero resolver/native escape.

Physical **0.0.163** corrected the Step-38 Cecil reader and then exposed the real lifecycle seam before any lifecycle method ran: exact `NGame._EnterTree()` directly calls `NGame.GameStartupWrapper()`.

Step 38.1 keeps `_EnterTree` unchanged and extends the already selected ahead-of-load compatibility derivative by rewriting **only** `GameStartupWrapper()` to:

```text
call System.Threading.Tasks.Task::get_CompletedTask()
ret
```

The transform reuses an existing exact sts2 MemberRef. The serialized derivative is reopened under rejecting resolution and fails closed unless the wrapper is exactly those two instructions with no locals/handlers, the wrapper metadata token is unchanged, `_EnterTree` still contains exactly one direct wrapper call, and the previously proven ModelDb compatibility plan remains valid.

Gate A verifies that inert wrapper and maps `_EnterTree`, `_Ready`, `GameStartupWrapper`, `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, and `LoadDeferredStartupAssetsAsync`. The verified inert wrapper is the only startup-named method permitted in `_EnterTree`'s same-`NGame` closure. Gate B re-instantiates the proven FMOD-neutral scene off-tree. Gate C invokes exact unchanged `_EnterTree()` **once**. Gate D requires `IsInsideTree=false`, OneTimeInitialization state `2`, and zero initializer-bearing/rejected/native escape before releasing the temporary node.

`SceneTree.AddChild`, `_Ready`, `_ExitTree`, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization, FMOD/Spine/Sentry native GDExtensions, gameplay, runtime Harmony/MonoMod, arbitrary resolver fallback, retry after Gate-C invocation starts, and state reset remain forbidden.

## Physical test sequence

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 **MODEL-BOOTSTRAP** once; require 4/4. This run creates the ModelDb + inert-startup-wrapper selected derivative.
3. Step 36.0.5 once; require 4/4.
4. Step 37.0.1 once; require 4/4.
5. Step 38.1 Gates A-D once. If Gate C begins and fails, use a fresh process before retry.
6. Preserve Step35/Step36/Step37/Step38 run-correlated reports.

Codemagic workflow key remains `ios-canonical`; existing NuGet, .NET, Godot Step-15, and iOS intermediate cache paths are unchanged.
