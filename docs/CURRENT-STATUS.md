# Current status

## Active candidate — Step 40.0 / 0.0.171 (171)

**Physical 0.0.170 / Step 39.2 — CLOSED POSITIVE 4/4.** Gate A reverified the exact compatibility/PCK authority. Gate B mapped the real 69-node hierarchy with 23 selected sts2 managed node types, 26 immediate lifecycle callbacks, 557 transitive same-sts2 closure methods, zero forbidden references, zero unresolved same-sts2 references, and zero external Cecil resolution. Gate C performed the real `SceneTree.Root.AddChild(NGame)` and returned with `IsInsideTree=True`, exact `NGame.Instance`, non-null `_window`, exact `SceneTree.Root` parent, OneTimeInitialization state 2, and zero initializer-bearing/rejected/native escape. The iOS caller synchronously called `StopRendering()` immediately afterward. Gate D then passed with the inserted hierarchy retained in-tree, rendering frozen, and the same state/native confinement intact.

Physical **0.0.169 / Step 39.1** remains the first successful real `AddChild` evidence; its only Gate-D failure was launcher reflection bookkeeping (`MoreThanOneMatch`). 0.0.170 corrected only the exact non-generic root-compatible `GetParent()` selector and physically closed Gate D. Physical **0.0.168** remains the Sentry generic-delegate audit false-positive localization; physical **0.0.167** remains the pre-insertion Steam/Sentry lifecycle blocker localization. The Step-39.1 private compatibility image remains unchanged: exact Steam Cloud availability probes are forced false and the game-owned Sentry boundary is inert/serialization-verified.

Physical **0.0.159** remains Step 36.0.5 4/4 authority. Physical **0.0.161** remains Step 37.0.1 4/4 authority. Physical **0.0.165 / Step 38.2** remains Step 38 closed 4/4 authority. Step 38 must still be skipped in the process used for Step 39/40.

## Step 40.0 boundary

Step 40 opens only controlled render-loop resumption. It does **not** enable `GameStartup` or later startup.

Gate A requires same-process Step 39 4/4 with the real NGame retained in-tree and `GodotStep15NativeBridge.IsRenderingActive == false`. It rechecks singleton/parent/`_window`/state authority, the exact selected compatibility-image SHA-256, and the inert `GameStartupWrapper`.

Gate B keeps rendering frozen. It enumerates the actual in-tree hierarchy and audits sts2 managed `_Process`, `_PhysicsProcess`, `_Draw`, `_Input`, `_ShortcutInput`, `_UnhandledInput`, `_UnhandledKeyInput`, and `_GuiInput` callbacks through each in-module base chain using Cecil `ReadingMode.Deferred` plus the rejecting resolver. It reuses the Step-39 fail-closed startup/native/platform classifier. Step-39's already-proven branch-insensitive `_Notification` closure remains prerequisite authority. Any surviving forbidden or unresolved same-sts2 edge stops before `StartRendering()`.

Gate C is one-shot. The UI durably checkpoints immediately before one `StartRendering()` call, targets **100 ms**, and the first continuation performs no work before synchronously calling `StopRendering()`. Success requires start returned/active, stop returned/inactive, observed elapsed `>0` and `<=500 ms`, retained NGame authority/state 2, and zero initializer-bearing/rejected/native escape. Once Gate C is armed, never retry Step 40 in-process.

Gate D requires the renderer to remain frozen after the pulse and re-proves the same retained hierarchy/state/native confinement. No cleanup/restart is performed.

## Physical sequence for 0.0.171

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → require 4/4.
3. Step 36.0.5 → require 4/4.
4. Step 37.0.1 → require 4/4.
5. **Skip Step 38 in this process.**
6. Step 39.0 A-D once → require **4/4** and renderer frozen.
7. **Without relaunching**, run Step 40.0 A-D once.
8. If Step 40 Gate C is armed, preserve Step40 reports and relaunch after the run/failure; never retry the pulse in-process.

Step 40 reports:
- `Step40-CrashCheckpoint-<RunId>.txt`
- `Step40-RenderPulse-StaticMap-<RunId>.txt`
- `Step40-LastCheckpoint.txt`
- `Step40-TransformedRealStS2RenderPulse.txt`

Still forbidden: `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, `ExecuteDeferred`, Steam initialization/native Steam API loading, FMOD/Spine/native Sentry game extensions, gameplay startup, explicit `_ExitTree`, RemoveChild/Free, state reset, mutation of the trusted Step-12 install, and leaving rendering active after the Step-40 pulse.
