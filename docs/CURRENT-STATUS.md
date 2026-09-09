# Current status

## Active candidate — Steps 43–50 direct main-menu startup ladder / 0.0.179 (179)

**Physical 0.0.175 / Step 42 — CLOSED POSITIVE 4/4.** Exact `NGame.InitPools()` token `0x06001BFA` mapped to 3 direct IL instructions and a 22-method transitive same-sts2 closure with zero classified startup/platform/native boundaries, zero unresolved same-sts2 references, and zero external Cecil resolution. Exact `InitPools()` was invoked once and returned on the retained real in-tree `NGame`; OneTimeInitialization stayed at state 2, rendering stayed frozen, and resolver/host/private/initializer/rejected/native deltas all remained zero. The closed **Step 42.0 boundary** never calls GameStartup. Its durable pre-invocation map is `Step42-InitPools-StaticMap-<RunId>.txt`. Once Gate C is armed, Step 42 must never be retried in-process.

**Physical 0.0.176 / Steps 43–45 — Step 45 Gate-B safe stop.** Same-process Step-44 4/4/no-legacy-data authority was retained. Gate B stopped before mutation because 0.0.176 incorrectly expected `SaveManager.<Instance>k__BackingField`. Reference metadata proves the actual singleton fields are `_mockInstance` and `_instance`.

**Physical 0.0.177 / Step 45 — Gate-C planned-host safe stop.** Corrected singleton metadata passed. Gate B mapped the three local-save roots to 331 same-sts2 methods with 6 approved classified references and zero forbidden/unresolved/external Cecil resolution. Gate C armed the exact one-shot `InitProfileId(null) -> InitProgressData() -> InitPrefsData()` sequence and observed only resolver +1 / host +1, with private/initializer/rejected/native all zero and rendering frozen.

**Physical 0.0.178 / Step 45 — CLOSED POSITIVE 4/4; old Step 46 safely localized.** Step 46 Gate A accepted same-process Step-45 4/4 authority with rendering stopped, NGame state 2, selected compatibility bytes unchanged, and zero post-Step-45 resolver/host/private/initializer/rejected/native drift. This physically closes Step 45. Step 46 Gate B then located exact `NGame.LaunchMainMenu(bool)` token `0x06001BDF`, async state machine `NGame/<LaunchMainMenu>d__131`, `MoveNext` token `0x06001C3D`, and 336 IL instructions with zero external Cecil resolution. Old Gate C failed safely before invocation because its recursive graph treated every MethodReference—including deferred delegate/function-pointer targets—as executable, pulling `LOAD_DEFERRED_STARTUP_ASSETS`, `ONE_TIME_INITIALIZATION`, platform/Steam, and Spine paths into the closure. `LaunchMainMenu` was never invoked and rendering never restarted.

**Physical 0.0.173 / Step 41 — CLOSED POSITIVE 4/4.** Exact `GameStartup`/compiler-`MoveNext` mapping closed with 691 same-sts2 methods, 46 classified boundary references, zero unresolved/external Cecil resolution; GameStartup remained uninvoked. **Physical 0.0.171 / Step 40 — CLOSED POSITIVE 4/4.** **Physical 0.0.170 / Step 39 — CLOSED POSITIVE 4/4.** Physical **0.0.159**, **0.0.161**, and **0.0.165** remain Step 36/37/38 authorities. **Step 38 must still be skipped** in the process used for Steps 39 onward.

The multi-rung packaging strategy remains: every rung has an independent four-gate sequence, exact same-process prerequisite, run-correlated checkpoint, distinct static map/report, and fail-stop behavior. **Stop immediately on the first failure**; never attempt later rungs after a failed prerequisite or retry an armed one-shot boundary in-process.

## Step 43.0 — concrete Null platform authority

Requires same-process Step 42 4/4, retained real NGame/state 2, selected compatibility bytes/context unchanged, and frozen rendering. Maps the concrete `NullPlatformUtilStrategy` read surface and invokes only exact read-only platform getters. External Steamworks/native/FMOD/Spine/external-Sentry boundaries remain forbidden. Gate D re-proves frozen confinement.

## Step 44.0 — legacy migration guard, read-only

Requires Step 43 4/4. Maps and invokes only `AccountScopeUserDataMigrator.HasLegacyData()` and `ProfileAccountScopeMigrator.HasLegacyData()`. If either returns true, fail closed before any migration/archive mutation. Step 44 never invokes `MigrateToUserScopedDirectories`, `ArchiveLegacyData`, `MigrateToProfileScopedDirectories`, or profile archive mutation.

## Step 45.0 — local SaveManager initialization — physically closed by 0.0.178

Requires Step 44 4/4/no legacy data. Gate B verifies exact static `SaveManager._mockInstance` / `_instance` fields plus serialized getter fallback shape, then maps exact `InitProfileId(Nullable<int>)`, `InitProgressData()`, and `InitPrefsData()`. Gate C reads `_instance` directly so `get_Instance()` / `ConstructDefault()` cannot run, invokes `InitProfileId(null) -> InitProgressData() -> InitPrefsData()` exactly once, and admits at most one exactly paired framework-shaped host binding already accepted by the persisted Step35 host plan. Private/initializer/rejected/native deltas remain zero. Gate D requires `SettingsSave`, `PrefsSave`, and `Progress` non-null and no further drift. Physical 0.0.178 proves the post-action authority.

## Step 46.1 — execution-aware LaunchMainMenu frontier map, no invocation

Gate A requires Step 45 4/4. Gate B locates exact instance `NGame.LaunchMainMenu(bool)`, its `AsyncStateMachineAttribute`, compiler state-machine type/fields, and full `MoveNext` IL without invocation. Gate C builds an **execution-opcode-qualified** map: `call`, `callvirt`, `newobj`, and `jmp` edges are traversed; `ldftn`, `ldvirtftn`, and `ldtoken` method references are recorded as deferred/non-executing frontiers and are not traversed. Compiler async/iterator state machines are recursively expanded only when their wrapper is reached from an immediate execution edge. Classified immediate boundaries are evidence, not authorization. The complete map is durably written. Gate D proves frozen no-invocation confinement. Original `LaunchMainMenu` remains forbidden regardless of map contents.

Reports: `Step46-CrashCheckpoint-<RunId>.txt`, `Step46-LaunchMainMenu-ImmediateFrontier-StaticMap-<RunId>.txt`, `Step46-LastCheckpoint.txt`, `Step46-TransformedRealStS2LaunchMainMenuImmediateFrontierMap.txt`.

## Step 47.0 — exact main-menu resource preparation, no instantiation

Requires Step 46.1 4/4 durable map authority. Extract exact receipt-backed `res://scenes/screens/main_menu.tscn` (19,087 B, SHA-256 `402b03596092097ffd7742a482642d740a293aa68dad6645f8b0c5aeab3376c0`) and `res://scenes/backgrounds/main_menu_bg.tscn` (7,916 B, SHA-256 `133a2ce2e05fb8d2e72a8e2087019cb5405b105636e16889525e53cc379aeab7`) from the trusted PCK into memory. Build only a private deterministic Spine-neutral background derivative (6,375 B, SHA-256 `0cf0c664f97e36325ef1645986af4ef5efaeff41f0ef0ee8180d115c381b8f3f`) under launcher data; trusted install/PCK bytes remain immutable. Durably write the resource map before Godot loading. Gate C is one-shot at both Core and UI levels: after arming, disable Step 47 and never retry in-process. Load the private background with cache-ignore, take over the exact background resource path in Godot's cache, then load the exact main-menu PackedScene with cache-reuse. No menu instantiation or rendering occurs.

Reports: `Step47-CrashCheckpoint-<RunId>.txt`, `Step47-MainMenuResources-StaticMap-<RunId>.txt`, `Step47-LastCheckpoint.txt`, `Step47-TransformedRealStS2MainMenuResourcePreparation.txt`.

## Step 48.0 — one-shot off-tree exact NMainMenu instantiation

Requires Step 47 4/4. Gate B binds exact `PackedScene.Instantiate(GenEditState)` and verifies exact managed root type `MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu` exists in the selected image. Gate C is one-shot: instantiate the retained exact menu PackedScene with `GenEditState.Disabled`, require exact private-load-context ownership and `IsInsideTree=false`, then enumerate the actual instantiated hierarchy/callback roots and build an immediate execution frontier. Any inadmissible immediate boundary fails while the menu remains off-tree. The actual hierarchy/lifecycle map is durably written before Step 49. Gate D requires frozen, off-tree, zero-post-instantiation drift authority. Never retry after Gate C is armed.

Reports: `Step48-CrashCheckpoint-<RunId>.txt`, `Step48-MainMenuOffTree-StaticMap-<RunId>.txt`, `Step48-LastCheckpoint.txt`, `Step48-TransformedRealStS2MainMenuOffTreeInstantiation.txt`.

## Step 49.0 — one-shot frozen NMainMenu SceneTree admission

Requires Step 48 4/4 retained off-tree authority. Gate B resolves exact real `NGame.RootSceneContainer`, captures child count, and binds exact `AddChild(Node,bool,InternalMode)`. Its map is durable before Gate C. Gate C adds the already-audited real `NMainMenu` once while rendering remains stopped. Gate D requires exact parent, child-count +1, `IsInsideTree=true`, retained NGame/state 2, and frozen post-admission context/native confinement. Never retry after AddChild is armed.

Reports: `Step49-CrashCheckpoint-<RunId>.txt`, `Step49-MainMenuAdmission-StaticMap-<RunId>.txt`, `Step49-LastCheckpoint.txt`, `Step49-TransformedRealStS2MainMenuFrozenAdmission.txt`.

## Step 50.0 — actual in-tree menu frame/input audit + bounded render pulse

Requires Step 49 4/4 frozen in-tree authority. Gate B enumerates actual in-tree main-menu frame/input callback roots and audits their immediate execution frontiers; the exact map must be durably written before rendering. Gate C is one-shot: `StartRendering()` exactly once, request the proven 100 ms pulse, then `StopRendering()` at the first managed continuation **before any post-stop telemetry/file I/O**. There is no abandonment timeout. Gate D requires rendering stopped again, exact retained menu/NGame authority, state 2, and frozen context/native confinement. Step 50 always leaves rendering stopped, including managed-exception cleanup.

Reports: `Step50-CrashCheckpoint-<RunId>.txt`, `Step50-MainMenuFrameInput-StaticMap-<RunId>.txt`, `Step50-LastCheckpoint.txt`, `Step50-TransformedRealStS2MainMenuRenderPulse.txt`.

## Physical sequence for 0.0.179

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → require 4/4.
3. Step 36.0.5 → require 4/4.
4. Step 37.0.1 → require 4/4.
5. **Skip Step 38 in this process.**
6. Step 39.0 A-D once → require 4/4 and renderer frozen.
7. Step 40.1 A-D once → require 4/4 and renderer frozen again.
8. Step 41.0 A-D once → require 4/4; GameStartup remains uninvoked.
9. Step 42.0 A-D once → require 4/4; exact `InitPools()` returned once and renderer remains frozen.
10. Step 43.0 → if 4/4 continue immediately.
11. Step 44.0 → if 4/4/no legacy data continue; otherwise stop before migration.
12. Step 45.0 once → if 4/4 continue; never retry after Gate C arm.
13. Step 46.1 → non-invoking map; if 4/4 continue.
14. Step 47.0 → exact resource preparation/load; if 4/4 continue.
15. Step 48.0 once → off-tree NMainMenu instantiate/audit; if 4/4 continue; never retry after arm.
16. Step 49.0 once → frozen RootSceneContainer admission; if 4/4 continue; never retry after arm.
17. Step 50.0 once → audited bounded render pulse; preserve reports/visual observations and relaunch afterward.

Still globally forbidden in 0.0.179 unless explicitly authorized above: whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, external Steamworks/native Steam APIs, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, FMOD/Spine native game extensions, explicit `_ExitTree`, `RemoveChild`/`Free`, state reset, or mutation of the trusted Step-12 install. **Step 50 is the only rung that may restart rendering, and it always refreezes before success evaluation.**
