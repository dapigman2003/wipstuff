# Current status

Earlier physical Step 39/40/41 authorities remain closed; **GameStartup remains uninvoked** on the engineering ladder.

### Physical 0.0.175 / Step 42 — CLOSED POSITIVE 4/4

Step 42 retained the exact **22-method zero-boundary closure** for `NGame.InitPools`, returned normally with the **renderer frozen**, and closed with **zero resolver/host/private/initializer/rejected/native deltas**. That physical authority remains protected by 0.0.195.

## Active candidate — Steps 58–62 forensic character-select localization / visible-render trial / 0.0.195 (195)

Physical runtime authority remains closed through **Step 57 4/4/frozen**. The one-button **Physically Closed Path** remains intentionally capped at Step 52; a fresh process must rebuild through 52 and then manually reprove 53–58 before Step 59.

### What physical 0.0.194 changed

0.0.194 disproved the remaining simple Step-59 theory. Immediately before original `OpenCharacterSelect(NButton)`:

- Step 58 had closed 4/4;
- retained `NSingleplayerSubmenu._stack` was already the exact `NMainMenuSubmenuStack`;
- the optional navigation-repair Push was skipped;
- the retained real `_standardButton` was supplied;
- cached `NCharacterSelectScreen` was present, hidden, in-tree, directly parented by the exact stack, and its own logical `_stack` was null;
- original `OpenCharacterSelect` then threw `NullReferenceException`; rendering remained frozen.

Static analysis now proves that character-select cache state is **normal game preload behavior**: `NMainMenuSubmenuStack._Ready()` pre-instantiates character select, hides it, and adds it as a child, while `NSubmenu._stack` remains null until `Push()`. It is no longer treated as an anomalous state.

### 0.0.195 Step 59 purpose

0.0.195 is a **forensic localization build**, not another speculative repair. Gate B records before the one-shot handler:

- `NCharacterSelectScreen.IsNodeReady()` and critical `_Ready`-bound fields (`_charButtonContainer`, `_ascensionPanel`, `_actDropdown`, `_actDropdownLabel`, `_remotePlayerContainer`, `_readyAndWaitingContainer`, `_backButton`, `_unreadyButton`, `_embarkButton`, `_randomCharacterButton`);
- `NAscensionPanel.IsNodeReady()`;
- NGame `HotkeyManager`, `InputManager`, `RemoteCursorContainer`, `ReactionContainer`, `TimeoutOverlay`, and `RootSceneContainer`;
- existing production SaveManager `Progress`, `Progress.Epochs`, and `Progress.EncounterStats`;
- `RootSceneContainer.CurrentScene` identity relative to the retained real main menu;
- current character `_lobby` state.

If original `OpenCharacterSelect` throws, Step 59 checkpoints the **inner exception type, TargetSite, Source, original StackTrace**, then captures post-failure character/lobby/player state before propagating with `ExceptionDispatchInfo` so the game-side stack is not reset by `throw inner`.

This should distinguish scene-readiness, ascension/hotkey, save/unlock, player-connected, `AfterInitialized`, and later `Push` failures in one device run. Steps 60–62 remain unchanged and are reachable only after Step 59 closes. Step 63, deliberate character input, confirm/embark, run start, and continuous Godot ownership remain unopened.

Long-term direction remains the revised master plan: use this ladder as an engineering harness, then converge toward compatibility-transformed normal `GameStartup`/`LaunchMainMenu` ownership rather than continuing screen-by-screen orchestration.

The 0.0.183 Codemagic AOT cache/sentinel experiment remains retained and independent of this runtime experiment. Earlier physical Step 39/40/41/42 and Step 45+ authorities remain as recorded; **GameStartup remains uninvoked** by this candidate. The current same-process path must **skip Step 38** before Step 39, and the convenience runner never executes Step 15 Gate D.

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

## Physical 0.0.184 Step 53 localization — safe Gate-B stop

The supplied physical 0.0.184 run reached the retained frozen Step-52 authority and passed Step 53 Gate A. Gate B then stopped before any handler invocation because the original candidate incorrectly required `OpenSingleplayerSubmenu()` to return `void`; the real selected image has **zero parameters and returns `MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSingleplayerSubmenu`**. The run ended normally with rendering still frozen. No Step-54 one-shot boundary was armed. 0.0.185 corrected that contract and allowed Step 53 to close so Step 54 could be reached.

Retained evidence: `STEP-53.0-PHYSICAL-0.0.184-FAIL-REPORT.txt`, `STEP-53.0-PHYSICAL-0.0.184-FAIL-CHECKPOINT.txt`, and `STEP-53.0-PHYSICAL-0.0.184-FAIL-LAST-CHECKPOINT.txt`.


## Physical 0.0.185 Step 54 localization — safe Gate-B stop

The supplied physical 0.0.185 run passed Step 54 Gate A from exact Step-53 authority, then failed at Gate B with `MissingFieldException` for `NMainMenu._singleplayerSubmenu`. The one-shot open was **not armed**, rendering remained frozen, and the run ended normally. The supplied game metadata places `_singleplayerSubmenu` on `NMainMenuSubmenuStack` and documents that stack as lazily spawning submenus when requested.

0.0.186 therefore binds exact `NMainMenu.SubmenuStack` plus `NMainMenuSubmenuStack._singleplayerSubmenu` field metadata before mutation, permits that lazy slot to be null pre-open, and after the one-shot requires `OpenSingleplayerSubmenu()`'s returned `NSingleplayerSubmenu` to be reference-identical to the now-populated stack field.

Retained evidence: `STEP-54.0-PHYSICAL-0.0.185-FAIL-REPORT.txt`, `STEP-54.0-PHYSICAL-0.0.185-FAIL-CHECKPOINT.txt`, `STEP-54.0-PHYSICAL-0.0.185-FAIL-LAST-CHECKPOINT.txt`.

## Physical 0.0.186 Step 56 localization — safe Gate-B stop after Step 55 closure

The supplied physical 0.0.186 run reached Step 56 from exact same-process Step-55 authority. Gate A passed with the real `NSingleplayerSubmenu` retained after its rendered/refrozen Step-55 closure. Gate B then failed before any character-select invocation because the candidate required zero parameters, while the selected real method has **one parameter and returns `System.Void`**. Rendering remained frozen and the run ended normally.

The supplied trusted `sts2.dll` (SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`) resolves the exact parameter type as `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton`, yielding exact signature `void NSingleplayerSubmenu.OpenCharacterSelect(NButton)`. 0.0.187 pins that exact parameter/return contract at both Gate B and Gate C before frontier mapping. No character-select execution is authorized.

Because Step 56 Gate A requires and observed exact Step-55 4/4/refrozen authority, Steps 53–55 are now recorded physically closed. The convenience closed-path runner remains capped at Step 52, so those three rungs must still be rerun in the same fresh process before Step 56.

Retained evidence: `STEP-56.0-PHYSICAL-0.0.186-FAIL-REPORT.txt`, `STEP-56.0-PHYSICAL-0.0.186-FAIL-CHECKPOINT.txt`, `STEP-56.0-PHYSICAL-0.0.186-FAIL-LAST-CHECKPOINT.txt`.

## Physical 0.0.187 Step 57 localization — safe Gate-A stop after Step 56 closure

The supplied physical 0.0.187 run entered Step 57 only after exact same-process Step-56 4/4 authority had closed. Step 57 then failed at Gate A with `observed=none` because the 0.0.187 discovery pipeline promoted only immediate-closure strings already shaped as full `res://...*.tscn` paths. The run ended normally with rendering frozen; no `OpenCharacterSelect`, `ResourceLoader`, `PackedScene`, PCK scene extraction, or character-select execution occurred.

The selected game image carries the shorter exact scene identity `screens/character_select_screen`; the supplied PCK path inventory shows the corresponding exact entry `res://scenes/screens/character_select_screen.tscn`. 0.0.188 attempted to retain that exact short/full identity from Step-56 immediate-closure `ldstr` evidence, but the next physical run proved no qualifying hint exists in that closure.

## Physical 0.0.188 Step 57 localization — retained hint absent, still read-only

Physical 0.0.188 re-entered Step 57 from the same physically closed Step-56 authority and failed immediately at Gate A with `requires the exact character-select scene hint from Step 56. observed=none`. The run ended normally with rendering frozen. No PCK entry bytes, `ResourceLoader`, `PackedScene.Instantiate`, `OpenCharacterSelect`, character-select execution, or Step 58 behavior occurred.

0.0.189 removes that incidental-literal dependency from Step 57 authority. Gate A instead proves the actual exported `NMainMenuSubmenuStack._characterSelectScreenScene : Godot.PackedScene` in selected metadata and the retained live stack, reads only its already-loaded `ResourcePath`, requires exact `res://scenes/screens/character_select_screen.tscn`, and then requires one matching receipt-backed PCK directory entry. Gate B remains the first byte-extraction point.

Because Step 57 requires Step 56 4/4 before entry, physical closure is now recorded through **Step 56**.

Retained evidence: `STEP-57.0-PHYSICAL-0.0.187-FAIL-REPORT.txt`, `STEP-57.0-PHYSICAL-0.0.187-FAIL-CHECKPOINT.txt`, `STEP-57.0-PHYSICAL-0.0.187-FAIL-LAST-CHECKPOINT.txt`.

## Step 53.0 — exact single-player submenu-open frontier, no invocation

Requires Step 52 4/4/refrozen authority. Binds exact zero-argument `NMainMenu.OpenSingleplayerSubmenu()` returning `NSingleplayerSubmenu`, records the broader `SingleplayerButtonPressed` only as evidence, and audits **only** `OpenSingleplayerSubmenu` using the execution-opcode-qualified frontier plus retained Step-48 runtime guards. Every immediate classified boundary must be admissible; unresolved same-StS2 references or external Cecil resolution fail. The exact map is durably written before Gate D. No handler is invoked and rendering remains frozen.

Reports: `Step53-CrashCheckpoint-<RunId>.txt`, `Step53-SingleplayerOpen-StaticMap-<RunId>.txt`, `Step53-LastCheckpoint.txt`, `Step53-TransformedRealStS2SingleplayerOpenFrontier.txt`.

## Step 54.0 — one-shot frozen real NSingleplayerSubmenu open

Requires Step 53 4/4 admissible authority. Token-matches the exact runtime `OpenSingleplayerSubmenu()` and binds exact `NMainMenu.SubmenuStack : NMainMenuSubmenuStack` plus that stack's exact `_singleplayerSubmenu : NSingleplayerSubmenu` field. Because the real stack lazily spawns submenus, the field may be null before mutation; any pre-existing value is recorded without forcing creation. The binding map is durable before mutation. Gate C is one-shot and invokes **only** exact `OpenSingleplayerSubmenu()` once while rendering stays frozen; `SingleplayerButtonPressed` remains uninvoked. Success requires the method return to be reference-identical to the stack's post-open `_singleplayerSubmenu`, with the resulting real submenu inside the SceneTree, `Visible=true`, and `IsVisibleInTree=true`. Selected-image/load-context/native authority must remain unchanged.

Reports: `Step54-CrashCheckpoint-<RunId>.txt`, `Step54-SingleplayerSubmenuOpen-StaticMap-<RunId>.txt`, `Step54-LastCheckpoint.txt`, `Step54-TransformedRealStS2SingleplayerSubmenuFrozenOpen.txt`.

## Step 55.0 — actual single-player submenu render residency

Requires Step 54 4/4. Audits the actual visible `NSingleplayerSubmenu` subtree with the bounded menu traversal and physically proven frame/input callback surface. The execution-qualified frontier must be admissible and the exact map durable before rendering. One render residency is authorized: requested target **750 ms**, post-stop evidence ceiling **5000 ms**. The first managed continuation synchronously calls `StopRendering()` before telemetry/file I/O. Gate D requires the real submenu still visible/in-tree and rendering frozen.

Reports: `Step55-CrashCheckpoint-<RunId>.txt`, `Step55-SingleplayerSubmenuFrameInput-StaticMap-<RunId>.txt`, `Step55-LastCheckpoint.txt`, `Step55-TransformedRealStS2SingleplayerSubmenuRender.txt`.

## Step 56.0 — exact character-select frontier map, no invocation

Requires Step 55 4/4/refrozen authority. Binds exact `void NSingleplayerSubmenu.OpenCharacterSelect(MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton)`, records its IL, rechecks that exact signature before frontier mapping, and maps the same immediate/deferred closure without invocation. Full `res://` literals and any character-select strings remain diagnostic evidence only; they are no longer authority for Step 57. Classified boundaries remain evidence only at this rung; unresolved same-StS2 references or external Cecil resolution fail. `OpenCharacterSelect` is never invoked and rendering remains frozen. Physical 0.0.187 closed this rung 4/4.

Reports: `Step56-CrashCheckpoint-<RunId>.txt`, `Step56-CharacterSelectFrontier-StaticMap-<RunId>.txt`, `Step56-LastCheckpoint.txt`, `Step56-TransformedRealStS2CharacterSelectFrontier.txt`.

## Step 57.0 — exact character-select PCK resource preflight, read-only

Requires Step 56 4/4. Gate A binds exact selected-metadata and retained-runtime `NMainMenuSubmenuStack._characterSelectScreenScene : Godot.PackedScene`, requires the retained field value non-null, reads only its existing inherited `ResourcePath`, requires exact `res://scenes/screens/character_select_screen.tscn`, and scans only the receipt-backed PCK directory to require exactly one matching entry under the sealed PCK header/count authority. Gate B then reads only that exact entry, refuses encryption/oversize/header drift, validates the directory MD5, records SHA-256/size/flags, decodes strict UTF-8 text, lists referenced `res://` paths, and counts textual Spine/FMOD/`.gdextension`/`.dylib`/`.dll` risk tokens. Risk findings authorize nothing. No `OpenCharacterSelect`, `ResourceLoader`, `PackedScene.Instantiate`, character-select execution, or rendering occurs.

Reports: `Step57-CrashCheckpoint-<RunId>.txt`, `Step57-CharacterSelectResource-StaticMap-<RunId>.txt`, `Step57-LastCheckpoint.txt`, `Step57-TransformedRealStS2CharacterSelectResourcePreflight.txt`.


## Physical Step 58 localization — 0.0.190 and 0.0.191

Physical 0.0.190 retained Step-57 4/4 authority and passed Step 58 Gate A, then stopped safely because `_characterSelectSubmenu` was already non-null before any Step-58 factory invocation. Rendering remained stopped and no new character-select operation armed.

Physical 0.0.191 retained the same Step-57 authority and again passed Step 58 Gate A. Gate B then proved the pre-existing exact character-select cache was already **inside the SceneTree**. Step 58 never created character select or restarted rendering, and the process ended normally frozen. This is the evidence that triggers the 0.0.193 ownership pivot.

Retained evidence includes the 0.0.190 and 0.0.191 Step-58 report/checkpoint/last-checkpoint sets under `docs/history/reports/`, plus the 0.0.192 logical-stack localization and ownership-pivot history notes.

## Physical Step 59 localization — 0.0.193 and 0.0.194

Physical 0.0.193 first reached the real handler and received `NullReferenceException`. 0.0.194 then tested the logical-stack/button theory directly. It closed Step 58, proved retained `NSingleplayerSubmenu._stack` was already the exact retained main-menu stack, skipped the optional Push repair, supplied the real retained `_standardButton`, and still received `NullReferenceException` immediately from original `OpenCharacterSelect`. Rendering remained frozen. This disproves both the null-button and missing-single-player-stack theories. Raw 0.0.193 and 0.0.194 report/checkpoint/last-checkpoint evidence is retained under `docs/history/reports/`.

## Step 58.0 — runtime ownership + real OpenCharacterSelect frontier audit

Requires Step 57 4/4/frozen. Binds exact `NSingleplayerSubmenu.OpenCharacterSelect(NButton)`, token/signature-checks it against the selected assembly, requires selected IL to prove the explicit `NButton` parameter is unread, and audits the whole handler frontier under the retained runtime guards. It also observes the actual `_characterSelectSubmenu` state as absent, off-tree, hidden in-tree, or visible in-tree, requiring exact selected/private-context type plus exact inherited submenu-stack identity whenever present. No handler invocation, rendering, or interaction occurs.

Reports: `Step58-CrashCheckpoint-<RunId>.txt`, `Step58-CharacterSelectOwnership-StaticMap-<RunId>.txt`, `Step58-LastCheckpoint.txt`, `Step58-TransformedRealStS2CharacterSelectOwnership.txt`.

## Step 59.0 — forensic prerequisites + one-shot real OpenCharacterSelect transition

Requires Step 58 4/4/frozen. Before any handler mutation, Gate B captures a durable prerequisite snapshot proving character-select/ascension `IsNodeReady`, critical character-select `_Ready` fields, NGame hotkey/input/remote-cursor/reaction/timeout/root-scene services, and production SaveManager `Progress/Epochs/EncounterStats`; it also records `RootSceneContainer.CurrentScene` identity and current character `_lobby` state. The retained single-player logical stack is still checked strictly: a foreign stack fails; exact retained needs no repair; null alone permits the previously audited game-owned `NSubmenuStack.Push(NSubmenu)` repair. Then original `OpenCharacterSelect(NButton)` is invoked once with the retained real `_standardButton`. If it throws, Step 59 checkpoints the inner game exception `TargetSite`, `Source`, and original stack trace and captures post-failure lobby/player/screen state before returning failure with the inner stack preserved through `ExceptionDispatchInfo`. No direct field repair, initializer-substage reproduction, `ExecuteDeferred`, or full `GameStartup` call is authorized. Never retry after handler transition arm.

Reports: `Step59-CrashCheckpoint-<RunId>.txt`, `Step59-RealOpenCharacterSelect-StaticMap-<RunId>.txt`, `Step59-LastCheckpoint.txt`, `Step59-TransformedRealStS2OpenCharacterSelectFrozen.txt`.

## Step 60.0 — actual active character-select surface audit

Requires Step 59 4/4/frozen. Audits the actual active character-select subtree's managed frame/input roots and execution-qualified frontiers under retained runtime guards. It also reads the exact Step-57 PCK TSCN bytes read-only and records its signal `[connection ...]` lines as evidence. No rendering, input, character choice, or game transition is authorized.

Reports: `Step60-CrashCheckpoint-<RunId>.txt`, `Step60-CharacterSelectActiveSurface-StaticMap-<RunId>.txt`, `Step60-LastCheckpoint.txt`, `Step60-TransformedRealStS2CharacterSelectActiveSurface.txt`.

## Step 61.0 — short visible character-select render residency

Requires Step 60 4/4/frozen. Writes durable pre-render evidence, starts real Godot rendering once, targets **2,000 ms** of visible observation with a **10,000 ms** post-stop evidence ceiling, and synchronously calls `StopRendering()` before post-stop telemetry/file I/O. No intentional taps or interaction are authorized. Gate D requires the same exact active screen authority with rendering frozen. Never retry after render arm.

Reports: `Step61-CrashCheckpoint-<RunId>.txt`, `Step61-CharacterSelectShortRender-StaticMap-<RunId>.txt`, `Step61-LastCheckpoint.txt`, `Step61-TransformedRealStS2CharacterSelectShortRender.txt`.

## Step 62.0 — sustained visible character-select render residency

Requires Step 61 4/4/refrozen. Rechecks the same active-screen authority, then starts real Godot rendering once for a target **10,000 ms** visible observation with a **30,000 ms** evidence ceiling, and synchronously refreezes before success evaluation. No intentional interaction is authorized. If this physically closes, the next design may consider continuous/interactive Godot ownership rather than another internal lifecycle ladder. Never retry after render arm.

Reports: `Step62-CrashCheckpoint-<RunId>.txt`, `Step62-CharacterSelectSustainedRender-StaticMap-<RunId>.txt`, `Step62-LastCheckpoint.txt`, `Step62-TransformedRealStS2CharacterSelectSustainedRender.txt`.

## Physical sequence for 0.0.195

Fresh process → press **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** once. Require Step 52 4/4/frozen. Then manually run **53 → 54 → 55 → 56 → 57**. Continue **58 → 59** first, stopping immediately at the first failure. Only if Step 59 closes 4/4 continue **60 → 61 → 62**.

Step 59 is one-shot once it arms the real handler transition; preserve the forensic checkpoint journal and final report on any failure. Steps 61 and 62 are one-shot after render arm. If all five new rungs close 4/4, stop after Step 62 and preserve its evidence. **Step 63 has no behavior in 0.0.195.**

Still globally forbidden in 0.0.195: whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, external Steamworks/native Steam APIs, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, character choice/confirm/embark/run-start actions, native FMOD/Spine game extensions, explicit teardown/state reset beyond already proven paths, trusted-install mutation, and Step 63+. Render rungs are Step 50, Step 52, Step 55, Step 61, and Step 62; each bounded rung synchronously refreezes before success evaluation.
