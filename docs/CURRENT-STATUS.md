# Current status

## Active candidate — Step 41.0 / 0.0.173 (173)

**Physical 0.0.171 / Step 40 — CLOSED POSITIVE 4/4.** The successful run retained same-process Step-39 4/4 authority with rendering frozen, passed the real in-tree frame/input audit at 68 nodes / 23 selected sts2 managed types / 12 immediate callbacks / 447 transitive same-sts2 methods / zero forbidden / zero unresolved / zero external-resolution requests, then `StartRendering()` returned `True` and active. The first managed continuation arrived at 101.8 ms; `StopRendering()` returned `True`, rendering was inactive at 103.3 ms, Gate C passed with zero resolver/host/private/initializer/rejected/native deltas, and Gate D proved frozen NGame singleton/parent/`_window`/state-2 confinement. The operation ended normally with rendering still stopped.

The earlier physical 0.0.171 run that observed 523.5 ms and failed the old 500 ms classifier remains timing provenance only. It does not override the later 4/4 physical closure. Step 40 is now closed positive. The unrun 0.0.172 harness correction is retained in source ancestry but is not required as separate physical authority.

Physical **0.0.170 / Step 39 remains CLOSED POSITIVE 4/4**. Physical **0.0.159**, **0.0.161**, and **0.0.165** remain Step 36/37/38 authorities. Step 38 must still be skipped in the process used for Step 39/40/41.

## Step 41.0 boundary

Gate A requires same-process Step 40 4/4 with the real NGame retained in-tree, renderer frozen, exact selected compatibility-image SHA-256 unchanged, OneTimeInitialization state 2, and the exact inert `GameStartupWrapper` still serialized in the private image.

Gate B is metadata-only. It locates exact `NGame.GameStartup`, requires its `AsyncStateMachineAttribute`, resolves the compiler-generated state-machine type from the same module without external Cecil resolution, records state-machine fields, and emits the full `GameStartup` and `MoveNext` IL/token map. **GameStartup is not invoked.**

Gate C traverses the `MoveNext` transitive same-sts2 method closure using deferred/rejecting Cecil and records path-qualified boundary references for OneTimeInitialization, InitializePlatform, platform services, Steam, main-menu launch, deferred startup assets, FMOD, Spine, Sentry, and native-extension surfaces. Same-sts2 references must resolve inside the selected module and external Cecil resolution must remain zero. Boundaries are mapped, not executed.

Gate D proves no-invocation confinement: rendering still stopped, real NGame singleton/parent/`_window` authority intact, state 2 unchanged, inert `GameStartupWrapper` still present, and zero initializer-bearing/rejected/native runtime escape.

## Physical sequence for 0.0.173

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → require 4/4.
3. Step 36.0.5 → require 4/4.
4. Step 37.0.1 → require 4/4.
5. **Skip Step 38 in this process.**
6. Step 39.0 A-D once → require **4/4** and renderer frozen.
7. **Without relaunching**, run Step 40.1 A-D once → require **4/4** and renderer frozen again.
8. **Without relaunching**, run Step 41.0 A-D once. Step 41 never invokes GameStartup or restarts rendering.
9. Preserve Step41 reports/static map and relaunch after the map is captured.

Step 41 reports:
- `Step41-CrashCheckpoint-<RunId>.txt`
- `Step41-GameStartupFrontier-StaticMap-<RunId>.txt`
- `Step41-LastCheckpoint.txt`
- `Step41-TransformedRealStS2GameStartupFrontier.txt`

Still forbidden: invoking `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, `ExecuteDeferred`, Steam initialization/native Steam API loading, FMOD/Spine/native game extensions, gameplay startup, explicit `_ExitTree`, RemoveChild/Free, state reset, mutation of the trusted Step-12 install, and restarting rendering during Step 41.
