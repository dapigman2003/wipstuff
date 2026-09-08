# StS2 Launcher — Step 39.0

Active candidate: **0.0.166 (165)** — iOS/off-tree-compatible `NGame._EnterTree()` execution.

Physical **0.0.159** closed Step 36.0.5 at 4/4, including unchanged full `ExecuteEssential()`. Physical **0.0.161** closed Step 37.0.1 at 4/4: the sealed FMOD-neutral `game.tscn` loaded as `PackedScene` and the real managed `NGame` hierarchy instantiated off-tree with zero resolver/native escape.

Physical **0.0.164 / Step 38.1** passed the inert-wrapper static audit and off-tree recreation, then physically entered `NGame._EnterTree()` and failed with a managed `NullReferenceException` while `IsInsideTree == false`. The exact IL map leaves two environment-sensitive edges before the inert startup wrapper: `SentryService.Initialize()` and the `GetWindow()` / `FilesDropped` / `Connect(...)` block.

Step 39.0 preserves the proven ModelDb bootstrap, FMOD-neutral scene, and exact inert `GameStartupWrapper` body:

```text
call System.Threading.Tasks.Task::get_CompletedTask()
ret
```

It adds only two stack-neutral `_EnterTree` compatibility edits: NOP the single Sentry initialization call, and NOP the exact bounded `GetWindow -> FilesDropped -> Connect -> pop` block. All singleton/property/GetNode setup remains present. The serialized derivative is reopened under rejecting resolution and must preserve `_EnterTree` instruction count, exhibit the exact expected NOP delta, contain zero remaining Sentry/GetWindow/FilesDropped/Connect references, retain exactly one direct inert-wrapper call, and retain the proven ModelDb compatibility shape.

Gate A verifies that shape and maps lifecycle reachability. Gate B re-instantiates the proven scene off-tree. Gate C invokes the verified compatibility `_EnterTree()` once. Gate D requires `IsInsideTree=false`, OneTimeInitialization state `2`, and zero initializer-bearing/rejected/native escape before releasing the node.

`SceneTree.AddChild`, `_Ready`, `_ExitTree`, `GameStartup`, `InitializePlatform`, main-menu launch, `ExecuteDeferred`, Steam initialization, native FMOD/Spine/Sentry GDExtensions, gameplay, runtime Harmony/MonoMod, arbitrary resolver fallback, retry after Gate-C invocation starts, and state reset remain forbidden.

## Physical test sequence

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 **MODEL-BOOTSTRAP** once; require 4/4.
3. Step 36.0.5 once; require 4/4.
4. Step 37.0.1 once; require 4/4.
5. Step 39.0 Gates A-D once. If Gate C begins and fails, use a fresh process before retry.
6. Preserve Step35/Step36/Step37/Step38 run-correlated reports.

Codemagic workflow key remains `ios-canonical`; existing NuGet, .NET, Godot Step-15, and iOS intermediate cache paths are unchanged.
