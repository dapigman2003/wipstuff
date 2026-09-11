# Current status

## Active candidate — Steps 53–57 single-player submenu continuation / 0.0.184 (184)

Runtime authority is physically closed through **Step 52** on the same-process direct-main-menu route. Physical 0.0.182 closed Step 49; the user subsequently reran and reported **Steps 50, 51 and 52 each passed 4/4**, and then tested the 0.0.183 app successfully. Successful rerun report files were not supplied, so that closure remains recorded as user-reported physical authority rather than fabricated evidence.

**0.0.184 opens only Steps 53–57.** The existing fresh-process **Physically Closed Path** runner remains intentionally capped at Step 52 and is the preferred way to reconstruct known authority. From that frozen Step-52 state, Step 53 isolates exact `NMainMenu.OpenSingleplayerSubmenu()` without invocation; Step 54 invokes only that exact admissible method once while frozen; Step 55 audits and briefly renders/refreezes the real `NSingleplayerSubmenu`; Steps 56–57 map and read-only-preflight the exact `OpenCharacterSelect` resource boundary without invoking or loading character select. Step 58 remains unopened.

The 0.0.183 Codemagic AOT cache/sentinel experiment is retained unchanged in 0.0.184. It is not required for the runtime experiment and can be evaluated from a later warm-build artifact without changing this candidate.

The multi-rung safety discipline remains unchanged: **the unit of safety remains one gate at a time; the unit of packaging becomes multiple gates per build.** The closed-path convenience runner calls existing numbered methods rather than bypassing them, checks exact closure after each return, explicitly skips Step 38, stops on first failure, and never auto-runs Step 15 Gate D. Steps 53–57 remain manual and each later rung requires the prior exact same-process 4/4 authority.

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

## Step 53.0 — exact single-player submenu-open frontier, no invocation

Requires Step 52 4/4/refrozen authority. Binds exact zero-argument void `NMainMenu.OpenSingleplayerSubmenu()`, records the broader `SingleplayerButtonPressed` only as evidence, and audits **only** `OpenSingleplayerSubmenu` using the execution-opcode-qualified frontier plus retained Step-48 runtime guards. Every immediate classified boundary must be admissible; unresolved same-StS2 references or external Cecil resolution fail. The exact map is durably written before Gate D. No handler is invoked and rendering remains frozen.

Reports: `Step53-CrashCheckpoint-<RunId>.txt`, `Step53-SingleplayerOpen-StaticMap-<RunId>.txt`, `Step53-LastCheckpoint.txt`, `Step53-TransformedRealStS2SingleplayerOpenFrontier.txt`.

## Step 54.0 — one-shot frozen real NSingleplayerSubmenu open

Requires Step 53 4/4 admissible authority. Token-matches the exact runtime `OpenSingleplayerSubmenu()` and binds exact `NMainMenu._singleplayerSubmenu` as `NSingleplayerSubmenu`. The binding map is durable before mutation. Gate C is one-shot and invokes **only** exact `OpenSingleplayerSubmenu()` once while rendering stays frozen; `SingleplayerButtonPressed` remains uninvoked. Success requires the same submenu object inside the SceneTree with `Visible=true` and `IsVisibleInTree=true`, and it must not already have been fully visible before the open. Selected-image/load-context/native authority must remain unchanged.

Reports: `Step54-CrashCheckpoint-<RunId>.txt`, `Step54-SingleplayerSubmenuOpen-StaticMap-<RunId>.txt`, `Step54-LastCheckpoint.txt`, `Step54-TransformedRealStS2SingleplayerSubmenuFrozenOpen.txt`.

## Step 55.0 — actual single-player submenu render residency

Requires Step 54 4/4. Audits the actual visible `NSingleplayerSubmenu` subtree with the bounded menu traversal and physically proven frame/input callback surface. The execution-qualified frontier must be admissible and the exact map durable before rendering. One render residency is authorized: requested target **750 ms**, post-stop evidence ceiling **5000 ms**. The first managed continuation synchronously calls `StopRendering()` before telemetry/file I/O. Gate D requires the real submenu still visible/in-tree and rendering frozen.

Reports: `Step55-CrashCheckpoint-<RunId>.txt`, `Step55-SingleplayerSubmenuFrameInput-StaticMap-<RunId>.txt`, `Step55-LastCheckpoint.txt`, `Step55-TransformedRealStS2SingleplayerSubmenuRender.txt`.

## Step 56.0 — exact character-select frontier map, no invocation

Requires Step 55 4/4/refrozen authority. Binds exact zero-argument void `NSingleplayerSubmenu.OpenCharacterSelect()`, records its IL, maps the execution/deferred frontier, and collects every `res://` string literal in the immediate same-module closure. Character-select `.tscn` candidates are recorded separately. Classified boundaries remain evidence only at this rung; unresolved same-StS2 references or external Cecil resolution fail. `OpenCharacterSelect` is never invoked and rendering remains frozen.

Reports: `Step56-CrashCheckpoint-<RunId>.txt`, `Step56-CharacterSelectFrontier-StaticMap-<RunId>.txt`, `Step56-LastCheckpoint.txt`, `Step56-TransformedRealStS2CharacterSelectFrontier.txt`.

## Step 57.0 — exact character-select PCK resource preflight, read-only

Requires Step 56 4/4. Selects exactly one character-select TSCN candidate (or one unambiguous `character_select`/`characterselect` candidate), reads only that exact entry from the receipt-backed PCK, refuses encryption/oversize/header drift, validates the directory MD5, records SHA-256/size/flags, decodes strict UTF-8 text, lists referenced `res://` paths, and counts textual Spine/FMOD/`.gdextension`/`.dylib`/`.dll` risk tokens. Risk findings authorize nothing. No `OpenCharacterSelect`, `ResourceLoader`, `PackedScene`, character-select execution, or rendering occurs.

Reports: `Step57-CrashCheckpoint-<RunId>.txt`, `Step57-CharacterSelectResource-StaticMap-<RunId>.txt`, `Step57-LastCheckpoint.txt`, `Step57-TransformedRealStS2CharacterSelectResourcePreflight.txt`.

## Physical sequence for 0.0.184

Preferred: fresh process → press **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** once. It reconstructs the known same-process authority and leaves rendering frozen after Step 52. Step 15 Gate D is intentionally not run. Then run the new rungs manually in order:

1. Step 53 — if 4/4, continue.
2. Step 54 — one-shot once Gate C arms; never retry in-process after an armed failure.
3. Step 55 — one-shot render residency once armed; always refreezes before evaluation.
4. Step 56 — non-invoking character-select frontier map.
5. Step 57 — read-only exact PCK resource preflight.

Stop at the first failure and preserve that rung's checkpoint/static-map/final report.

Still globally forbidden in 0.0.184: Step 58+, whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, external Steamworks/native Steam APIs, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, character-select handler invocation/resource loading/instantiation/admission, FMOD/Spine native game extensions, explicit `_ExitTree`, `RemoveChild`/`Free`, state reset, and trusted-install mutation. Physically closed Steps 50/52 and active Step 55 are the only render rungs; each synchronously refreezes before success evaluation.
