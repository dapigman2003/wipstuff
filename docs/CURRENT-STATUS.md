# Current status

## Active candidate — Steps 43–52 guarded direct main-menu ladder / 0.0.182 (182)

Runtime authority has advanced to **Step 48 closed 4/4**, with Step 49 localized to a launcher verification ceiling. Physical 0.0.181 Step 49 Gate A accepted same-process Step-48 4/4 authority. Gate B resolved the exact retained `/Game/RootSceneContainer`, observed the NGame property null, token-matched `set_RootSceneContainer` at `0x06001B9B`, and proved a four-instruction/zero-call/one-field trivial assignment. Gate C repaired the property to the exact retained child with zero context/native drift and invoked frozen `RootSceneContainer.AddChild(NMainMenu)`.

**Physical 0.0.181 / Step 48 — CLOSED POSITIVE 4/4.** Step 49 Gate A is the authoritative same-process proof: real `NMainMenu` was retained off-tree, rendering was stopped, NGame state was 2, and Step-48 closure was accepted before Step 49 began.

**Physical 0.0.181 / Step 49 — SAFE 2/4 POST-ADMISSION-DIAGNOSTIC STOP.** Gate A and Gate B passed. Gate C repaired `NGame.RootSceneContainer` successfully and then started the one-shot frozen `AddChild(NMainMenu)`. The failure stack is in the *post-AddChild* `RequireStartupLadderRootSceneContainerPropertyIdentity` call, whose old implementation traversed the whole NGame tree via `EnumerateStep39NodeGraph`. That historical helper aborts at 256 nodes. Because the source reaches this property check only after the reflected AddChild call, `NMainMenu.IsInsideTree=true`, and exact-parent validation, the physical result strongly localizes the failure to diagnostic verification after menu lifecycle expansion, not to the AddChild invocation itself. Rendering remained frozen and the UI operation ended normally.

**0.0.182 / Step 49.1 correction.** RootSceneContainer identity is now resolved only from immediate NGame children, requiring exactly one child named `RootSceneContainer`, exact managed `NSceneContainer` type, `IsInsideTree=true`, exact direct NGame parent, and property consistency. No admitted-menu descendants are traversed for this authority check. Gate C now emits durable checkpoints immediately after AddChild returns, after `IsInsideTree=true`, after exact-parent verification, and after the observed RootSceneContainer child count. Steps 50 and 52 use a separate menu-only breadth-first diagnostic traversal with a 4,096-node ceiling; the historical Step-39 256-node helper is unchanged.

**Physical 0.0.178 / Step 45 — CLOSED POSITIVE 4/4.** Step 46 Gate A accepted same-process Step-45 local-save authority with state 2, renderer stopped, selected compatibility bytes unchanged, and zero post-Step-45 resolver/host/private/initializer/rejected/native drift. Step 46 Gate B also mapped exact original `NGame.LaunchMainMenu(bool)` token `0x06001BDF`, async state machine `NGame/<LaunchMainMenu>d__131`, `MoveNext` token `0x06001C3D`, 336 IL instructions, zero external Cecil resolution. Original `LaunchMainMenu` remains uninvoked.

**Physical 0.0.175 / Step 42 — CLOSED POSITIVE 4/4.** Exact `NGame.InitPools()` mapped to a 22-method zero-boundary closure and executed once on retained real NGame with state 2, renderer frozen, and zero resolver/host/private/initializer/rejected/native deltas.

Earlier physical Step 39/40/41 authorities remain closed. **Step 38 must still be skipped** in the process used for Steps 39 onward.

The multi-rung packaging discipline remains unchanged: every rung has independent four-gate state, exact same-process prerequisite authority, step-distinct durable checkpoint/static-map/final-report evidence, and stop-on-first-failure behavior. Never attempt a later rung after a failed prerequisite. Never retry an armed one-shot boundary in-process.

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

## Step 50.0 — in-tree menu frame/input audit + short bounded render pulse

Requires Step 49 4/4. Reuses current Step-48 runtime guards for the actual in-tree callback audit. The exact map is durable before one-shot `StartRendering()`. It requests the proven 100 ms pulse and calls `StopRendering()` at the first managed continuation **before post-stop telemetry/file I/O**. Gate D requires rendering frozen again and retained menu/NGame authority.

## Step 51.0 — non-invoking single-player frontier map

Requires Step 50 4/4/refrozen authority. Binds exact `NMainMenu.SingleplayerButtonPressed` and `NMainMenu.OpenSingleplayerSubmenu`, records direct IL, and builds an execution/deferred frontier using the retained runtime guards. This step **never invokes either handler and never renders**. Classified boundaries are retained as evidence for the next design decision; unresolved same-sts2 references or external Cecil resolution fail.

Reports: `Step51-CrashCheckpoint-<RunId>.txt`, `Step51-SingleplayerFrontier-StaticMap-<RunId>.txt`, `Step51-LastCheckpoint.txt`, `Step51-TransformedRealStS2SingleplayerFrontier.txt`.

## Step 52.0 — sustained real-menu render residency + synchronous refreeze

Requires Step 51 4/4 durable map authority. Runs a fresh actual in-tree frame/input audit, durably writes it, then arms one render residency. `StartRendering()` runs once; target residency is **1500 ms** with a **6000 ms** post-stop evidence ceiling; the first managed continuation calls `StopRendering()` before any telemetry/file I/O. Success requires retained in-tree NMainMenu, zero initializer/rejected/native escape during the residency, and no further context drift after the captured post-pulse baseline. Step 52 always leaves rendering frozen.

Reports: `Step52-CrashCheckpoint-<RunId>.txt`, `Step52-MainMenuSustainedFrameInput-StaticMap-<RunId>.txt`, `Step52-LastCheckpoint.txt`, `Step52-TransformedRealStS2MainMenuSustainedRender.txt`.

## Physical sequence for 0.0.182

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → require 4/4.
3. Step 36.0.5 → require 4/4.
4. Step 37.0.1 → require 4/4.
5. **Skip Step 38 in this process.**
6. Step 39.0 A-D once → require 4/4, renderer frozen.
7. Step 40.1 A-D once → require 4/4, renderer frozen.
8. Step 41.0 A-D once → require 4/4; GameStartup remains uninvoked.
9. Step 42.0 A-D once → require 4/4; InitPools returns once, renderer frozen.
10. Step 43 → continue only on 4/4.
11. Step 44 → continue only on 4/4/no legacy data.
12. Step 45 once → continue only on 4/4; never retry after arm.
13. Step 46.1 → non-invoking map; continue only on 4/4.
14. Step 47 once → resource prep/load; continue only on 4/4; never retry after arm.
15. Step 48.2 once → physically closed design; re-establish same-process off-tree authority and continue only on 4/4; never retry after arm.
16. Step 49.1 once → frozen SceneTree admission with direct-child root verification; continue only on 4/4; never retry after arm.
17. Step 50 once → short audited render pulse/refreeze; continue only on 4/4.
18. Step 51 → non-invoking single-player frontier map; continue only on 4/4.
19. Step 52 once → sustained audited render residency/refreeze; preserve reports/visual observations and relaunch afterward.

Still globally forbidden in 0.0.182 unless explicitly authorized above: whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, external Steamworks/native Steam APIs, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, FMOD/Spine native game extensions, explicit `_ExitTree`, `RemoveChild`/`Free`, state reset, trusted-install mutation, and any single-player handler invocation. **Only Steps 50 and 52 may restart rendering; both must synchronously refreeze before success evaluation.**
