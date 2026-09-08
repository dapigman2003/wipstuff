# Step 38.1 — inert `GameStartupWrapper` compatibility + controlled `_EnterTree`

Physical 0.0.163 corrected the Step-38 Cecil reader and then stopped safely in Gate A before lifecycle execution. The exact selected compatibility image proved a direct same-`NGame` edge:

`NGame._EnterTree() -> NGame.GameStartupWrapper()`

`GameStartupWrapper` is documented by the game as the wrapper that begins asynchronous game startup and displays an error if startup fails. Therefore unchanged `_EnterTree` cannot be invoked under the earlier policy without immediately authorizing a later startup boundary.

## Compatibility decision

Do not weaken the Gate-A closure policy and do not runtime-detour the method. Extend the already selected ahead-of-load ModelDb compatibility derivative by rewriting **only** `NGame.GameStartupWrapper()` to the exact body:

```text
call System.Threading.Tasks.Task::get_CompletedTask()
ret
```

The transform reuses an existing `Task.get_CompletedTask` MemberRef already present in the exact sts2 module. `_EnterTree` itself is not rewritten and must retain exactly one direct call to `GameStartupWrapper`.

The serialized derivative is reopened under rejecting Cecil resolution and fails closed unless:

- `GameStartupWrapper` remains the same instance, parameterless `Task` method and the same metadata token;
- its body contains exactly two instructions: `call Task.get_CompletedTask`, `ret`;
- it has no locals and no exception handlers;
- `_EnterTree` still contains exactly one direct call to `GameStartupWrapper`;
- the previously proven ModelDb `1624/56/47/41` graph and VAR(0)/VAR(1) Dictionary encoding remain valid;
- exact Step-32 assembly identity/MVID and constant metadata remain unchanged.

## Step-38 execution policy

Gate A may traverse the verified inert `GameStartupWrapper` in the same-`NGame` call closure. It still fails closed if `_EnterTree` reaches `_Ready`, `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, `LoadDeferredStartupAssetsAsync`, or OneTimeInitialization `ExecuteVeryEarly`/`ExecuteEssential`/`ExecuteDeferred`/`PrewarmJit`.

Gate B re-instantiates the physically proven FMOD-neutral game scene off-tree. Gate C invokes exact unchanged `_EnterTree()` once. Gate D requires `IsInsideTree=false`, OneTimeInitialization state `2`, and zero initializer-bearing/rejected/native escape before releasing the node without `_ExitTree`.

SceneTree insertion, `_Ready`, `GameStartup`, platform initialization, main-menu launch, `ExecuteDeferred`, Steam initialization, FMOD/Spine/Sentry native GDExtensions, gameplay, retry after Gate-C invocation begins, and state reset remain outside this step.
