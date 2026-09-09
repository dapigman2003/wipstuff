# Current status

## Active candidate — Steps 43–47 startup ladder / 0.0.176 (176)

**Physical 0.0.175 / Step 42 — CLOSED POSITIVE 4/4.** Exact `NGame.InitPools()` token `0x06001BFA` mapped to 3 direct IL instructions and a 22-method transitive same-sts2 closure with zero classified startup/platform/native boundaries, zero unresolved same-sts2 references, and zero external Cecil resolution. Exact `InitPools()` was invoked once and returned on the retained real in-tree `NGame`; OneTimeInitialization stayed at state 2, rendering stayed frozen, and resolver/host/private/initializer/rejected/native deltas all remained zero. This physically closes the first isolated real GameStartup primitive.

**Physical 0.0.173 / Step 41 — CLOSED POSITIVE 4/4.** Exact `GameStartup`/compiler-`MoveNext` mapping closed with 691 same-sts2 methods, 46 classified boundary references, zero unresolved/external Cecil resolution; GameStartup remained uninvoked. **Physical 0.0.171 / Step 40 — CLOSED POSITIVE 4/4.** **Physical 0.0.170 / Step 39 — CLOSED POSITIVE 4/4.** Physical **0.0.159**, **0.0.161**, and **0.0.165** remain Step 36/37/38 authorities. Step 38 must still be skipped in the process used for Steps 39 onward.

The closed **Step 42.0 boundary** remains one-shot authority: Step 42.0 never calls GameStartup. Its durable pre-invocation map is `Step42-InitPools-StaticMap-<RunId>.txt`. Once Gate C is armed, Step 42 must never be retried in-process.

0.0.176 changes the **iteration packaging strategy**, not the safety model: one IPA contains five independently gated rungs. Each rung has its own run-correlated checkpoint, static map where applicable, final report, exact prior-rung prerequisite, and four-gate summary. Later rungs stay locked until the prior rung is 4/4 in the same process. Stop immediately on the first failure and preserve/share that rung's artifacts; do not attempt later rungs. This preserves diagnosability while reducing rebuild/install cycles.

## Step 43.0 — concrete Null platform authority

Gate A requires same-process Step 42 4/4 with the real NGame retained in-tree, rendering frozen, state 2, and exact selected compatibility bytes/context unchanged.

Gate B maps `PlatformUtil` plus concrete `MegaCrit.Sts2.Core.Platform.Null.NullPlatformUtilStrategy` read-only methods. The only admissible classified platform edges are exact platform dispatch/read helpers, pure platform extensions, concrete Null-strategy methods, and exact `SteamInitializer.get_Initialized()` observation; external Steamworks/native/FMOD/Spine/external-Sentry boundaries remain forbidden. Zero unresolved same-sts2 and zero external Cecil resolution are required. The map is durably written before Gate C.

Gate C reflects `PlatformUtil.PrimaryPlatform`, obtains `GetPlatformUtil(primary)`, requires the concrete strategy type to be **exactly** `NullPlatformUtilStrategy`, then invokes only `IPlatformUtilStrategy` read getters for local player ID, platform branch, three-letter/raw language, and supported window mode. No platform initialization or Steam API is invoked. State/context/native deltas must remain zero.

Gate D re-proves frozen NGame/state/context confinement. A 4/4 Step 43 becomes the explicit runtime authority allowing later rungs to treat those exact Null-platform read paths as established compatibility behavior rather than an unknown platform boundary.

## Step 44.0 — legacy migration guard, read-only

Gate A requires Step 43 4/4. Gate B maps only `AccountScopeUserDataMigrator.HasLegacyData()` and `ProfileAccountScopeMigrator.HasLegacyData()` with the same Null-platform boundary policy. Gate C invokes those two **read-only** probes. If either returns true, Step 44 fails safely and stops the ladder **before any migration/archive mutation** so a dedicated backup/migration candidate can be designed from evidence. If both are false, Gate D proves frozen confinement and establishes that migration work is unnecessary in this launcher sandbox.

Step 44 never invokes `MigrateToUserScopedDirectories`, `ArchiveLegacyData`, `MigrateToProfileScopedDirectories`, or profile archive mutation.

## Step 45.0 — local SaveManager initialization

Gate A requires Step 44 4/4/no legacy data. Gate B maps exact `SaveManager.InitProfileId(Nullable<int>)`, `InitProgressData()`, and `InitPrefsData()` closures. Proven Null-platform read helpers and already-inert `SentryService` wrappers are admissible; external Steamworks, native extensions, GameStartup, cloud sync, migration mutation, and later startup boundaries remain forbidden. The complete map is durably written before Gate C.

Gate C is one-shot: invoke `InitProfileId(null)` → `InitProgressData()` → `InitPrefsData()` exactly once in original GameStartup order. Returned progress/prefs `ReadSaveResult` metadata is reported. OneTimeInitialization must remain state 2 and resolver/host/private/initializer/rejected/native deltas must remain zero. Never retry Step 45 in-process after Gate C is armed.

Gate D requires `SaveManager.SettingsSave`, `PrefsSave`, and `Progress` to be non-null while NGame/render/context authority remains confined.

## Step 46.0 — exact LaunchMainMenu async map, no invocation

Gate A requires Step 45 4/4. Gate B locates exact instance `NGame.LaunchMainMenu(bool)`, resolves its `AsyncStateMachineAttribute`, compiler-generated state-machine type, fields, and exact `MoveNext` IL without invocation.

Gate C traverses the **inner LaunchMainMenu MoveNext** same-sts2 closure and recursively expands every reached same-sts2 `AsyncStateMachineAttribute`, `IteratorStateMachineAttribute`, or `AsyncIteratorStateMachineAttribute` into its compiler-generated `MoveNext`. This corrects the limitation of the Step-41 GameStartup map and prevents a nested async/iterator wrapper from hiding an unsafe edge. Proven Null-platform reads and inert Sentry wrappers are allowed; `OneTimeInitialization`, `InitializePlatform`, `LoadDeferredStartupAssets`, external Steamworks, FMOD, Spine, external Sentry, and native-extension edges are forbidden. Zero unresolved same-sts2 and zero external Cecil resolution are required.

The complete state-machine + closure map must be durably written and explicitly marked authoritative before Gate D. Gate D keeps rendering frozen, proves no invocation/runtime drift, and only then unlocks Step 47.

## Step 47.0 — one-shot controlled live LaunchMainMenu

Step 47 is available only if Step 46 closes 4/4 with `launchAdmissible=true` and the complete map was durably written.

Gate A re-proves prior authority while rendering is still frozen and records the pre-launch `RootSceneContainer` child count. Gate B binds exact runtime `NGame.LaunchMainMenu(bool)` and requires its metadata token to equal the Step-46 Cecil token and its return type to be `Task`.

Gate C is one-shot. The iOS caller starts Godot rendering exactly once immediately before invoking `LaunchMainMenu(skipIntro=true)`. The launcher awaits that Task to completion and deliberately has **no abandonment timeout**, because the game method has no cancellation contract and returning a timeout while its Task can still mutate state would compromise confinement. A hard native/render/task hang remains diagnosable by the durable pre-invocation/start-render checkpoint and requires killing/relaunching the process. Any normal managed fault returns to the launcher and rendering is synchronously refrozen at the first managed opportunity. Resolver/host/private/initializer/rejected/native deltas must remain zero and state must remain 2.

Gate D requires the real NGame singleton/parent/`_window` authority to remain intact **and** requires `RootSceneContainer` to gain at least one child scene compared with the pre-launch count. On 4/4 success rendering intentionally remains active so the resulting real menu scene can be observed. On any failure/incomplete result, the launcher refreezes rendering and later work remains locked.

## Physical sequence for 0.0.176

1. Fresh process → Step 15 A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP → require 4/4.
3. Step 36.0.5 → require 4/4.
4. Step 37.0.1 → require 4/4.
5. **Skip Step 38 in this process.**
6. Step 39.0 A-D once → require 4/4 and renderer frozen.
7. Step 40.1 A-D once → require 4/4 and renderer frozen again.
8. Step 41.0 A-D once → require 4/4; GameStartup remains uninvoked.
9. Step 42.0 A-D once → require 4/4; exact `InitPools()` returned once and renderer remains frozen.
10. Run **Step 43.0**. If 4/4, immediately continue to Step 44 in the same process.
11. Run **Step 44.0**. If 4/4, immediately continue to Step 45. If legacy data is reported, stop and preserve Step44 artifacts; do not migrate.
12. Run **Step 45.0** once. If Gate C is armed, never retry it in-process. If 4/4, continue to Step 46.
13. Run **Step 46.0**. If 4/4/admissible, continue to Step 47.
14. Run **Step 47.0 once**. On failure preserve Step47 reports and relaunch; the launcher attempts to refreeze rendering. On 4/4 success leave the app running to observe the resulting live scene and preserve all Step47 artifacts before any later experiment.

Reports:
- Step 43: `Step43-CrashCheckpoint-<RunId>.txt`, `Step43-Platform-StaticMap-<RunId>.txt`, `Step43-LastCheckpoint.txt`, `Step43-TransformedRealStS2NullPlatformAuthority.txt`
- Step 44: `Step44-CrashCheckpoint-<RunId>.txt`, `Step44-LegacyGuard-StaticMap-<RunId>.txt`, `Step44-LastCheckpoint.txt`, `Step44-TransformedRealStS2LegacyMigrationGuard.txt`
- Step 45: `Step45-CrashCheckpoint-<RunId>.txt`, `Step45-LocalSave-StaticMap-<RunId>.txt`, `Step45-LastCheckpoint.txt`, `Step45-TransformedRealStS2LocalSaveInitialization.txt`
- Step 46: `Step46-CrashCheckpoint-<RunId>.txt`, `Step46-LaunchMainMenu-StaticMap-<RunId>.txt`, `Step46-LastCheckpoint.txt`, `Step46-TransformedRealStS2LaunchMainMenuMap.txt`
- Step 47: `Step47-CrashCheckpoint-<RunId>.txt`, `Step47-LastCheckpoint.txt`, `Step47-TransformedRealStS2ControlledLaunchMainMenu.txt` (Step 47 consumes the durable Step-46 map authority rather than writing a second static map)

Still globally forbidden in 0.0.176 unless explicitly stated above: invoking `GameStartup` as a whole, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, external Steamworks/native Steam APIs, `ExecuteDeferred`, `LoadDeferredStartupAssets`, FMOD/Spine/native game extensions, explicit `_ExitTree`, RemoveChild/Free, state reset, or mutation of the trusted Step-12 install. Step 47 is the only rung that may restart rendering, and only after Step 46 has physically established an admissible LaunchMainMenu closure.
