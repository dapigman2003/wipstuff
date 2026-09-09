# Current status

## Active candidate — Step 42.0 / 0.0.175 (175)

**Codemagic 0.0.174 compile failure — NO RUNTIME CONCLUSION.** Host compilation failed in `TransformedRealStS2GameStartupInitPools.cs` because the new Step-42 code referenced non-existent `Step35ExecutionLoadContext` members `InitializerLoads`, `RejectedLoads`, and `NativeLoads`, plus a non-existent `BuildFailureDiagnostic` helper. 0.0.175 is a compile-only correction: those references are replaced with the existing proven members `InitializerBearingRequests`, `RejectedManagedRequests`, `NativeLoadAttempts`, and `FormatExceptionDiagnostic`. Gate semantics, exact `InitPools()` audit/invocation, one-shot behavior, frozen rendering, and all forbidden boundaries are unchanged. 0.0.174 produced no device evidence and must not be treated as a Step-42 runtime result.

**Physical 0.0.173 / Step 41 — CLOSED POSITIVE 4/4.** Exact `NGame.GameStartup()` token `0x06001BD1` and compiler state-machine `<GameStartup>d__117::MoveNext()` token `0x06001C31` were mapped without invocation. The device map recorded 691 transitive same-sts2 startup methods and 46 classified boundary references: `LAUNCH_MAIN_MENU=1`, `ONE_TIME_INITIALIZATION=2`, `PLATFORM=36`, `SENTRY_INERT_WRAPPER=5`, `STEAM=2`; unresolved same-sts2 references and external Cecil resolution were both zero. Gate D preserved frozen real-NGame/state-2 authority with zero resolver/host/private/initializer/rejected/native deltas. GameStartup remained uninvoked and rendering remained stopped.

Physical **0.0.171 / Step 40** remains CLOSED POSITIVE 4/4. Physical **0.0.170 / Step 39** remains CLOSED POSITIVE 4/4. Physical **0.0.159**, **0.0.161**, and **0.0.165** remain Step 36/37/38 authorities. Step 38 must still be skipped in the process used for Steps 39–42.

## Step 42.0 boundary

Gate A requires same-process Step 41 4/4 with the real NGame retained in-tree, renderer frozen, exact selected compatibility-image SHA-256 unchanged, OneTimeInitialization state 2, and the exact inert `GameStartupWrapper` still serialized in the private image.

Gate B is pre-invocation and fail-closed. It locates exact instance `NGame.InitPools()` (`void`, zero arguments), emits its direct IL, traverses the transitive same-sts2 closure using the Step-41 deferred/rejecting Cecil resolver and boundary classifier, and requires **zero** classified platform/Steam/main-menu/deferred/FMOD/Spine/Sentry/native/OneTimeInitialization references, **zero** unresolved same-sts2 references, and zero external Cecil resolution. The verified map is durably written before Gate C.

Gate C is the first and only newly executed startup primitive. It binds exact `InitPools()` by reflection on the retained real NGame, requires the runtime metadata token to equal the Gate-B token, invokes it **once**, and requires it to return normally with OneTimeInitialization state still 2 and resolver/host/private/initializer/rejected/native deltas all zero. Once Gate C is armed the process is lifecycle-mutated for Step 42; do not retry in-process even on failure.

Gate D keeps rendering frozen and proves retained NGame singleton/parent/`_window`/state authority, exact compatibility bytes, inert `GameStartupWrapper`, and unchanged resolver/host/private/initializer/rejected/native counters. Step 42 never calls GameStartup itself.

## Physical sequence for 0.0.175

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → require 4/4.
3. Step 36.0.5 → require 4/4.
4. Step 37.0.1 → require 4/4.
5. **Skip Step 38 in this process.**
6. Step 39.0 A-D once → require 4/4 and renderer frozen.
7. Without relaunching, Step 40.1 A-D once → require 4/4 and renderer frozen again.
8. Without relaunching, Step 41.0 A-D once → require 4/4; GameStartup remains uninvoked.
9. Without relaunching, run Step 42.0 A-D once. If Gate C is armed, preserve reports and relaunch afterward; never retry Step 42 in-process.

Step 42 reports:
- `Step42-CrashCheckpoint-<RunId>.txt`
- `Step42-InitPools-StaticMap-<RunId>.txt`
- `Step42-LastCheckpoint.txt`
- `Step42-TransformedRealStS2GameStartupInitPools.txt`

Still forbidden: invoking `GameStartup`, account/profile migration, cloud sync, `InitializePlatform`, platform identity, Steam initialization/native Steam APIs, `LaunchMainMenu`, `ExecuteDeferred`, FMOD/Spine/native game extensions, gameplay startup, explicit `_ExitTree`, RemoveChild/Free, state reset, mutation of the trusted Step-12 install, and restarting rendering during Step 42.

