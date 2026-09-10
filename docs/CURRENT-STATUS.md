# Current status

## Active candidate — Steps 43–52 physically closed stabilization / 0.0.183 (183)

Runtime authority now reaches **Step 52** on the current same-process direct-main-menu route. Physical 0.0.182 closed Step 49. The supplied first Step-50 attempt then passed Gate A/B, mapped 1,075 in-tree nodes / 94 managed node types / 24 frame-input roots / 149 immediate methods with zero forbidden/unresolved references, successfully started rendering, and synchronously stopped it with rendering inactive; the only rejection was a cold ~2526 ms first managed continuation beyond the old 2000 ms classification ceiling. The user subsequently reported a fresh-process rerun where **Steps 50, 51 and 52 each passed 4/4**. Successful rerun report files were not supplied, so that closure is recorded as user-reported physical authority rather than fabricated evidence.

**0.0.183 is stabilization only; Step 53 remains unopened.** It adds a fresh-process one-shot convenience runner over the already-closed route, a Step-50-specific 4000 ms evidence ceiling while preserving the 100 ms target and stop-before-telemetry/refreeze semantics, and a Codemagic AOT cache correction/telemetry package based on the supplied 3250-second build artifact.

The Codemagic artifact localizes **3195/3250 seconds** to iOS publish/package. Existing cache restoration was healthy (2.9 GB iOS obj and 1092 AOT outputs), but the MSBuild binlog reported missing `AOTCompileInputs.cache.uptodate` while LLVM `opt`/`llc` still ran broadly. 0.0.183 keeps workflow `ios-canonical`, M2, pinned .NET/Xcode, and existing caches, then adds only the two tiny AOT dependency sentinel files plus exact pre/post and opt/llc telemetry. The first 0.0.183 build seeds those new cache paths; the following warm build is the decisive performance test.

The multi-rung safety discipline remains unchanged: the convenience runner calls existing numbered methods rather than bypassing them, checks exact closure after each return, explicitly skips Step 38, stops on first failure, and never auto-runs Step 15 Gate D.

Retained prior physical authority: **Physical 0.0.175 / Step 42 — CLOSED POSITIVE 4/4.** Exact `NGame.InitPools()` retained its **22-method zero-boundary closure**, the **renderer frozen** throughout, with **zero resolver/host/private/initializer/rejected/native deltas**. Earlier physical Step 39/40/41 authorities remain closed; **GameStartup remains uninvoked**. The current same-process path must **skip Step 38** before Step 39.

## Step 43.0 — concrete Null platform authority

Requires Step 42 4/4 and frozen retained NGame/state 2. Maps and invokes only exact read-only `NullPlatformUtilStrategy` getters. Steam/native/FMOD/Spine/external-Sentry boundaries remain forbidden.

## Step 44.0 — legacy migration guard, read-only

Requires Step 43 4/4. Maps and invokes only the two `HasLegacyData()` probes. Any legacy-data-positive result stops before migration/archive mutation.

## Step 45.0 — local SaveManager initialization — physically closed

Requires Step 44 4/4/no legacy data. Uses exact static `_mockInstance`/`_instance` authority, bypasses `get_Instance()`/`ConstructDefault()`, and invokes `InitProfileId(null) → InitProgressData() → InitPrefsData()` exactly once. At most one paired, exact planned host-framework materialization is admissible; private/initializer/rejected/native escape remains zero. Physical 0.0.178 proves closure.

## Step 46.1 — execution-aware original LaunchMainMenu map, no invocation — physically closed

Requires Step 45 4/4. Maps exact original `LaunchMainMenu(bool)`, async state machine and immediate/deferred frontier without invoking it. Immediate opcodes are traversed; `ldftn`/`ldvirtftn`/`ldtoken` references are recorded but not recursively treated as execution. This is evidence only and never authorizes original `LaunchMainMenu`. Physical 0.0.179 passed this rung.

## Step 47.0 — exact main-menu resource preparation — physically closed

Requires Step 46.1 4/4. Uses exact receipt-backed `main_menu.tscn` (19,087 B, SHA-256 `402b03596092097ffd7742a482642d740a293aa68dad6645f8b0c5aeab3376c0`) and `main_menu_bg.tscn` (7,916 B, SHA-256 `133a2ce2e05fb8d2e72a8e2087019cb5405b105636e16889525e53cc379aeab7`), creates only the private deterministic Spine-neutral background derivative (6,375 B, SHA-256 `0cf0c664f97e36325ef1645986af4ef5efaeff41f0ef0ee8180d115c381b8f3f`), performs one-shot resource-cache takeover/load, and does not instantiate/render. Physical 0.0.179 passed this rung.

## Step 48.2 — guarded off-tree NMainMenu lifecycle authority — physically closed

Requires Step 47 4/4. Gate B retains the exact instantiation and lifecycle guard-shape audit. Gate C is one-shot and instantiates exact real `NMainMenu` off-tree. For tree-shape authority it now resolves the exact retained `/Game/RootSceneContainer` directly from the real NGame child graph, requiring exact path, exact `NSceneContainer` type, direct NGame parent, and `IsInsideTree=true`. The nullable `NGame.RootSceneContainer` property is observed but is **not** required to be non-null in Step 48; if non-null it must already reference that exact child.

The remaining rehearsal stays strict: zero Godot command-line args; exact production SaveManager `_instance` with null `_mockInstance`; exact getter returning that same instance; exact Null-platform routing; no-drift `SetRichPresence`; and exact off-tree `CheckCommandLineArgs()` with retained menu/root child counts, state 2, and zero resolver/host/private/initializer/rejected/native drift. Only `CheckCommandLineArgs`, `SaveManager.get_Instance`, and `PlatformUtil.SetRichPresence` may be runtime-guarded frontiers. No generic PLATFORM/STEAM whitelist exists. Step 48 also writes its preliminary Gate-B guard-shape map before the one-shot instantiation, so any later Gate-C failure retains useful static evidence.

Reports: `Step48-CrashCheckpoint-<RunId>.txt`, `Step48-MainMenuOffTree-StaticMap-<RunId>.txt`, `Step48-LastCheckpoint.txt`, `Step48-TransformedRealStS2MainMenuOffTreeInstantiation.txt`.

## Step 49.1 — one-shot frozen NMainMenu SceneTree admission with direct-child verification

Requires Step 48 4/4 retained guard authority. Gate B resolves the exact retained RootSceneContainer from **immediate NGame children only** and binds exact `NGame.set_RootSceneContainer(NSceneContainer)`. The runtime method token must match Cecil; the setter must have zero method calls, exactly one `NSceneContainer` field write on NGame, only trivial load/store/return opcodes, and zero external Cecil resolution.

In the one-shot frozen Gate C, if and only if the property is null, it assigns the exact retained immediate child once and verifies zero context/native drift. A mismatched non-null property fails closed. It then performs exact `RootSceneContainer.AddChild(NMainMenu)`. Durable checkpoints distinguish: AddChild returned; `NMainMenu.IsInsideTree=true`; exact parent passed; RootSceneContainer child count observed; property identity retained. No whole-NGame descendant traversal is used for root authority after admission.

Reports: `Step49-CrashCheckpoint-<RunId>.txt`, `Step49-MainMenuAdmission-StaticMap-<RunId>.txt`, `Step49-LastCheckpoint.txt`, `Step49-TransformedRealStS2MainMenuFrozenAdmission.txt`.

## Step 50.0 — in-tree menu frame/input audit + short bounded render pulse — physically closed

Requires Step 49 4/4. 0.0.183 retains the 100 ms requested stop and uses a Step-50-specific 4000 ms evidence ceiling; StopRendering remains first managed-continuation work before telemetry. Historical Step40 stays 2000 ms.  Reuses current Step-48 runtime guards for the actual in-tree callback audit. The exact map is durable before one-shot `StartRendering()`. It requests the proven 100 ms pulse and calls `StopRendering()` at the first managed continuation **before post-stop telemetry/file I/O**. Gate D requires rendering frozen again and retained menu/NGame authority.

## Step 51.0 — non-invoking single-player frontier map — physically closed

Requires Step 50 4/4/refrozen authority. Binds exact `NMainMenu.SingleplayerButtonPressed` and `NMainMenu.OpenSingleplayerSubmenu`, records direct IL, and builds an execution/deferred frontier using the retained runtime guards. This step **never invokes either handler and never renders**. Classified boundaries are retained as evidence for the next design decision; unresolved same-sts2 references or external Cecil resolution fail.

Reports: `Step51-CrashCheckpoint-<RunId>.txt`, `Step51-SingleplayerFrontier-StaticMap-<RunId>.txt`, `Step51-LastCheckpoint.txt`, `Step51-TransformedRealStS2SingleplayerFrontier.txt`.

## Step 52.0 — sustained real-menu render residency + synchronous refreeze — physically closed

Requires Step 51 4/4 durable map authority. Runs a fresh actual in-tree frame/input audit, durably writes it, then arms one render residency. `StartRendering()` runs once; target residency is **1500 ms** with a **6000 ms** post-stop evidence ceiling; the first managed continuation calls `StopRendering()` before any telemetry/file I/O. Success requires retained in-tree NMainMenu, zero initializer/rejected/native escape during the residency, and no further context drift after the captured post-pulse baseline. Step 52 always leaves rendering frozen.

Reports: `Step52-CrashCheckpoint-<RunId>.txt`, `Step52-MainMenuSustainedFrameInput-StaticMap-<RunId>.txt`, `Step52-LastCheckpoint.txt`, `Step52-TransformedRealStS2MainMenuSustainedRender.txt`.

## Physical sequence for 0.0.183

Preferred: fresh process → press the new **Physically Closed Path** button once. It executes:

1. Step 15 A–C only.
2. Step 35.0.32 MODEL-BOOTSTRAP.
3. Step 36.0.5.
4. Step 37.0.1.
5. **Skip Step 38.**
6. Step 39.
7. Step 40.1.
8. Step 41.
9. Step 42.
10. Steps 43 → 52 in order.

The runner verifies each existing exact pass/closure predicate before advancing, stops on first failure, preserves all normal reports, and leaves rendering frozen after Step 52. Step 15 Gate D is intentionally not run. Manual controls remain available for diagnosis using the same sequence.

Still globally forbidden in 0.0.183: Step 53+, whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, external Steamworks/native Steam APIs, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, FMOD/Spine native game extensions, explicit `_ExitTree`, `RemoveChild`/`Free`, state reset, trusted-install mutation, and any single-player handler invocation. Only physically closed Steps 50 and 52 may restart rendering, and both synchronously refreeze before success evaluation.
