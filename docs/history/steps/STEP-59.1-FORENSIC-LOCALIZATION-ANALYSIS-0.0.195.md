# STS2 iOS — Next Iteration Analysis
**Date:** 2026-09-12  
**Basis:** trusted `sts2.dll`, prior static maps, current 0.0.194 source, and physical 0.0.194 Step-59 evidence.

## Executive conclusion

The next code iteration should **not attempt another speculative repair** and should **not append a full `GameStartup()` invocation onto the existing ladder**.

The strongest next move is a **forensic localization / startup-convergence candidate** that:

1. treats the current hidden/in-tree/unbound `NCharacterSelectScreen` as a **normal game-owned preload state**;
2. proves the exact runtime prerequisites consumed by `InitializeSingleplayer()` before invoking the real handler;
3. invokes original `OpenCharacterSelect(NButton)` at most once;
4. preserves and logs the original inner exception stack/target site instead of resetting it at the reflection boundary;
5. if the handler throws, captures post-failure game state before returning failure, allowing the failure to be localized to a specific stage;
6. separately records the remaining lifecycle divergence from original `LaunchMainMenu`, especially `NSceneContainer.CurrentScene`.

This should make one physical run decisive enough to choose the subsequent architectural change.

The likely long-term direction remains **compatibility-transformed normal startup and normal game ownership**, not permanent screen-by-screen launcher orchestration.

---

## 1. What the latest physical run actually proves

Physical 0.0.194 reached the original game handler with:

- Step-58 authority intact;
- renderer frozen;
- retained `NSingleplayerSubmenu._stack` already bound to the exact retained `NMainMenuSubmenuStack`;
- the real retained `_standardButton`;
- a real cached character-select screen that was:
  - present,
  - inside the Godot SceneTree,
  - hidden,
  - not visible in tree,
  - parented to the exact main-menu submenu stack,
  - logically unbound (`NSubmenu._stack == null`).

The original `OpenCharacterSelect(NButton)` was invoked once and immediately produced a `NullReferenceException`.

Therefore these earlier hypotheses are now rejected:

- null/fake `NButton`;
- missing single-player submenu logical stack;
- foreign submenu stack ownership;
- absence of the character-select cache.

The current report loses the original game-side stack location because the launcher unwraps `TargetInvocationException` with `throw tie.InnerException`, resetting the useful exception stack to the launcher callsite.

---

## 2. Important correction: the character-select pre-state is normal

Static analysis of trusted `sts2.dll` shows:

### `NMainMenuSubmenuStack._Ready()`

It calls:

1. `GetSubmenuType<NSettingsScreen>()`
2. `GetSubmenuType<NCharacterSelectScreen>()`

The concrete `NCharacterSelectScreen` factory branch:

1. checks `_characterSelectSubmenu`;
2. if null, instantiates `_characterSelectScreenScene`;
3. sets `Visible = false`;
4. adds the screen as a child of the submenu stack;
5. caches and returns it.

It does **not** assign the inherited `NSubmenu._stack`.

That assignment occurs later inside `NSubmenuStack.Push()` via `NSubmenu.SetStack(this)`.

Therefore:

> cached + hidden + already in-tree + parented to stack + logical `_stack == null`

is the game's intended pre-push state.

This explains the repeated Step-58 observations without requiring any anomalous lifecycle or hidden transition. Future code should encode this as the expected baseline instead of treating it as suspicious.

---

## 3. Exact `OpenCharacterSelect` fault surface

Trusted game IL is effectively:

```text
screen = this._stack.GetSubmenuType<NCharacterSelectScreen>();
screen.InitializeSingleplayer();
this._stack.Push(screen);
```

The physical run already proves `this._stack` is valid immediately before invocation.

The remaining high-level failure locations are:

1. `GetSubmenuType<NCharacterSelectScreen>()`
2. `InitializeSingleplayer()`
3. `NSubmenuStack.Push(screen)`

Static + physical evidence makes (1) relatively unlikely:
- the cache already exists;
- the same cache is physically observable before invocation;
- the factory normally returns the cached instance.

The next run should still prove the returned/cached identity indirectly, but most diagnostic effort should focus on (2) and (3).

---

## 4. Exact `InitializeSingleplayer()` sequence

Trusted IL reduces to five major stages:

```text
A. _lobby = new StartRunLobby(
       GameMode.Standard,
       new NetSingleplayerGameService(),
       this,
       1);

B. _ascensionPanel.Initialize(Singleplayer);

C. unlockState = new UnlockState(SaveManager.Instance.Progress);

D. _lobby.AddLocalHostPlayer(unlockState, 0);

E. AfterInitialized();
```

This is why “character select” is not merely a UI transition. It creates the run lobby and initializes gameplay-adjacent infrastructure.

### Stage A — lobby construction

`NetSingleplayerGameService` is a very small managed implementation.

`StartRunLobby`:
- stores the supplied service/listener/mode;
- initializes collections;
- creates `PeerInputSynchronizer`;
- registers message handlers;
- subscribes disconnect handling.

For `NetSingleplayerGameService`, the network handler-registration surface is effectively inert/local. No strong missing global dependency emerged here.

**Current likelihood:** low-to-moderate.

### Stage B — ascension panel

For singleplayer mode, `NAscensionPanel.Initialize(1)`:

- stores `_mode = Singleplayer`;
- calls `SetFireRed()`;
- enables arrows;
- sets max ascension to 0;
- sets ascension level to 0;
- obtains `NHotkeyManager.Instance`;
- pushes left/right hotkey bindings.

Potential nulls include:
- `NCharacterSelectScreen._ascensionPanel`;
- ascension-panel `_Ready` fields used by `SetFireRed` / level display;
- `NHotkeyManager.Instance`.

`NGame._EnterTree()` physically ran successfully earlier and the real game hierarchy contains the hotkey/input/remote cursor/reaction/timeout nodes, making the NGame-owned manager dependency less suspicious.

The **scene-owned ascension fields remain a meaningful candidate**, because the current evidence says the character screen is in-tree but has never explicitly proven `IsNodeReady()` or each `_Ready`-bound field.

**Current likelihood:** high enough to instrument directly.

### Stage C — save/progression state

Normal `GameStartup` calls:

```text
SaveManager.InitProfileId(null)
SaveManager.InitProgressData()
SaveManager.InitPrefsData()
```

before `LaunchMainMenu`.

The current ladder intentionally reproduced these exact calls in Step 45, in original order, and later steps have accepted physical Step-45 4/4 authority.

Therefore the broad theory “we never initialized saves because we bypassed GameStartup” is **not well supported**.

However, `UnlockState(Progress)` dereferences nested progress structures such as:
- `Progress.Epochs`;
- `Progress.EncounterStats`.

The next preflight should verify these exact objects, not merely `SaveManager.Instance`.

**Current likelihood:** moderate, but straightforward to rule in/out read-only.

### Stage D — add local host player

`AddLocalHostPlayer()` eventually calls the character screen back through `PlayerConnected()`.

`PlayerConnected()` does:

1. `_remotePlayerContainer.OnPlayerConnected(player)`
2. `RefreshButtonSelectionForPlayer(player)`
3. `UpdateRichPresence()`
4. `UpdateRandomCharacterVisibility()`

For the local singleplayer player:
- rich presence exits without using Steam/platform rich presence;
- local selection handling has early-return behavior;
- random-character visibility uses `_randomCharacterButton`, lobby players/unlock state, and `ModelDb.AllCharacters`.

This stage can therefore expose an incomplete character-screen `_Ready()` through fields such as:
- `_remotePlayerContainer`;
- `_randomCharacterButton`.

**Current likelihood:** meaningful.

### Stage E — `AfterInitialized()`

This initializes:
- `NGame.RemoteCursorContainer`;
- `NGame.ReactionContainer`;
- `NGame.TimeoutOverlay`;
- rich presence/random character state;
- additional UI/bootstrap behavior.

Earlier NGame lifecycle evidence shows the corresponding real nodes exist and `NGame._EnterTree()` assigns these properties.

This makes a missing NGame-owned node less likely than a character-screen-owned `_Ready` field, but the next preflight should prove the exact live properties.

**Current likelihood:** lower, but not eliminated.

---

## 5. Character-screen `_Ready()` is now a primary diagnostic target

`NCharacterSelectScreen._Ready()` binds the fields later consumed by `InitializeSingleplayer`, `PlayerConnected`, and `OnSubmenuOpened`, including:

- `_charButtonContainer`;
- `_ascensionPanel`;
- `_actDropdown`;
- `_actDropdownLabel`;
- `_remotePlayerContainer`;
- `_readyAndWaitingContainer`;
- `_backButton`;
- `_unreadyButton`;
- `_embarkButton`;
- and later creates/stores `_randomCharacterButton`.

Therefore the next build should explicitly prove:

```text
characterSelect.IsInsideTree()
characterSelect.IsNodeReady()
```

and exact non-null/type identity for the fields above.

Being inside the SceneTree is not, by itself, sufficient evidence that the managed `_Ready()` callback completed without an exception.

The nested `NAscensionPanel` should likewise prove `IsNodeReady()` and its critical bound fields before `InitializeSingleplayer()` is allowed.

This is currently the most valuable missing runtime evidence.

---

## 6. ModelDb / ExecuteDeferred hypothesis — investigated and downgraded

A plausible initial hypothesis was:

```text
current ladder skips ExecuteDeferred()
    -> ModelDb.Preload() never runs
    -> UpdateRandomCharacterVisibility() fails
```

Static analysis weakens that as the immediate explanation:

- `ExecuteEssential()` already runs `ModelDb.Init()` and `ModelDb.InitIds()`;
- current launcher physically executes the essential initialization path;
- `NCharacterSelectScreen._Ready()` itself uses `ModelDb.AllCharacters` while creating character buttons;
- `ModelDb.Preload()` primarily forces broad model/property materialization and asset/model warm-up.

So skipping deferred startup remains a **real lifecycle divergence that should be repaired in the long-term startup profile**, but it is not currently the strongest explanation for this NRE.

`ExecuteDeferred()` also contains `PrewarmJit()`, which repeatedly calls `RuntimeHelpers.PrepareMethod`. That is specifically inappropriate for an iOS AOT-oriented product path and should be transformed/no-op'd when deferred startup is restored.

Recommended long-term treatment:

| Deferred operation | Classification |
|---|---|
| `AtlasManager.LoadAllAtlases()` | KEEP |
| `ModelDb.Preload()` | KEEP |
| `PrewarmJit()` / `RuntimeHelpers.PrepareMethod` | SUBSTITUTE with exact AOT-safe no-op |
| `ConditionalFormatter` construction | KEEP |

---

## 7. Original `LaunchMainMenu` divergence

The current harness intentionally bypasses original `LaunchMainMenu`.

Original game flow:

```text
LaunchMainMenu(skipLogo)
    -> LoadMainMenuEssentials()
    -> optional logo lifecycle
    -> LoadMainMenu(false)
         -> NMainMenu.Create(...)
         -> RootSceneContainer.SetCurrentScene(menu)
    -> fire-and-forget LoadDeferredStartupAssetsAsync()
```

The current direct path instead:
- instantiates the exact main-menu scene;
- repairs `NGame.RootSceneContainer` if needed;
- directly adds the menu as a child.

It does **not** reproduce `NSceneContainer.SetCurrentScene(menu)`.

That means the visible/real menu can exist as a child while the container's semantic `CurrentScene` ownership differs from normal game startup.

This is an important product-architecture divergence.

It is **not yet proven to be the direct cause of the current `InitializeSingleplayer()` NRE**, because the immediate character-select methods do not clearly require `NGame.MainMenu`.

The next diagnostic should therefore **record**:

```text
RootSceneContainer.CurrentScene
NGame.MainMenu
```

and whether either is reference-identical to the retained real `NMainMenu`.

Do not mutate it merely to make Step 59 pass unless the fault/provenance map demonstrates that it is required.

---

## 8. GameStartup convergence matrix

The existing ladder has already recreated more normal startup than the recent NRE might suggest.

Physically established/proven pieces include:
- transformed `ExecuteVeryEarly`;
- `ExecuteEssential`;
- real `NGame` SceneTree `_EnterTree` / `_Ready`;
- `InitPools`;
- local SaveManager profile/progress/prefs initialization;
- Null/offline platform authority;
- real main-menu and submenu Godot lifecycles.

So the current failure is **not simply “we skipped GameStartup.”**

For a future production-style startup profile:

| Original startup operation | Recommendation | Reason |
|---|---|---|
| `ExecuteVeryEarly()` | KEEP, with existing proven compatibility transform | Required one-time initialization |
| Dev console creation | DEFER | Diagnostic, not base-game requirement |
| account/profile migrations | DEFER + audit | Mutating/idempotency risk |
| `DoCloudSync()` | SUBSTITUTE initially | Use local/offline completion; Steam Cloud later |
| `InitPools()` | KEEP | Physically proven |
| `ExecuteEssential()` | KEEP | Physically proven |
| await Godot readiness | KEEP | Preserves lifecycle order |
| graphics/display initialization | SUBSTITUTE | iOS window/display semantics differ |
| audio preference application | KEEP semantics / SUBSTITUTE backend as necessary | Preference ordering matters, native audio compatibility separate |
| leaderboard initialization | SUBSTITUTE/DEFER | Offline/null strategy initially |
| Steam stats | BLOCK/DEFER | Native Steam dependency |
| timeout overlay relocalization | KEEP | Ordinary managed/UI state |
| wait cloud completion | KEEP ordering against substituted task | Preserves sequencing |
| `InitProfileId` / `InitProgressData` / `InitPrefsData` | KEEP | Physically proven |
| Sentry after-init | SUBSTITUTE inert/local | Not required for gameplay |
| screen-shake / fast-mode prefs | KEEP | Managed gameplay/UI preferences |
| `LaunchMainMenu()` | RESTORE / KEEP | Critical ownership/lifecycle semantics |
| `LoadDeferredStartupAssetsAsync()` | KEEP overall, transformed | Normal background completion |
| `ExecuteDeferred()` | KEEP transformed | Important deferred game state |
| `ModelDb.Preload()` | KEEP | Normal deferred model warm-up |
| `PrewarmJit()` | SUBSTITUTE no-op | Desktop JIT behavior incompatible with iOS AOT |
| Steam join callback | BLOCK/DEFER | Native Steam |
| mod-detection subscription | DEFER | Mods are later milestone |

### Important testing constraint

Do **not** call full original/transformed `GameStartup()` on top of the current same-process ladder.

The ladder has already executed stateful pieces such as one-time initialization and saves. Re-running normal startup in the same process would duplicate stateful work and violate `OneTimeInitialization` state expectations.

A transformed full-normal-startup experiment must be a **fresh alternative startup mode** that replaces the piecemeal path, not an additional later step.

---

## 9. Recommended next candidate: forensic character-select localization

The next physical candidate should preserve the currently closed baseline through Step 57 and replace speculative repair logic with a diagnostic transition.

### Gate A — retain trusted baseline

Keep:
- selected compatibility-image identity;
- Step-57 authority;
- renderer frozen;
- retained menu/submenu/cache identities.

### Gate B — read-only prerequisite snapshot

Before any handler call, durably record:

#### Character screen
- exact type/load-context/cache identity;
- `IsInsideTree`;
- `IsNodeReady`;
- `Visible`;
- `IsVisibleInTree`;
- exact parent;
- logical `_stack` — expected null before Push;
- non-null/type status:
  - `_ascensionPanel`;
  - `_charButtonContainer`;
  - `_remotePlayerContainer`;
  - `_randomCharacterButton`;
  - `_embarkButton`;
  - `_actDropdown`;
  - `_readyAndWaitingContainer`.

#### Ascension panel
- `IsInsideTree`;
- `IsNodeReady`;
- `_iconHsv`;
- `_ascensionLevel`;
- left/right arrows;
- any other fields directly dereferenced by `SetFireRed`, `SetMaxAscension`, or `SetAscensionLevel`.

#### NGame-owned services
- `NGame.Instance`;
- `HotkeyManager`;
- `InputManager`;
- controller manager;
- `RemoteCursorContainer`;
- `ReactionContainer`;
- `TimeoutOverlay`.

All must be exact private-context game instances where appropriate.

#### Save/progress
- exact `SaveManager.Instance`;
- `Progress`;
- `Progress.Epochs`;
- `Progress.EncounterStats`;
- optionally construct a temporary managed `UnlockState(Progress)` as a separately audited read/managed-allocation preflight if static policy permits.

#### Model system
- current `OneTimeInitialization` state;
- read-only `ModelDb.AllCharacters` enumeration succeeds;
- no need to call `ExecuteDeferred` merely for diagnosis.

#### Menu ownership
Record, but do not automatically require:
- `RootSceneContainer.CurrentScene`;
- `NGame.MainMenu`;
- reference identity against retained `NMainMenu`.

This quantifies how far the direct-main-menu path diverges from original ownership.

### Gate C — one-shot original handler

Only if all **truly required** prerequisites pass:

- invoke original `OpenCharacterSelect(NButton)` exactly once;
- use the retained real standard button;
- rendering stays frozen.

### Exception handling improvement

On `TargetInvocationException`:

1. obtain `InnerException`;
2. **durably record the inner exception before rethrow**, including:
   - type;
   - message;
   - `TargetSite`;
   - `Source`;
   - original `StackTrace`;
3. preserve its stack if propagating (`ExceptionDispatchInfo.Capture(inner).Throw()` or equivalent), rather than `throw inner`.

This alone may identify the exact StS2 failing method.

### Gate C failure forensic snapshot

Before returning failure, capture the mutated state.

At minimum:

#### Character
- `_lobby` null/non-null;
- logical `_stack`;
- visible/in-tree state.

#### Lobby, if created
- net-service type;
- `InputSynchronizer` null/non-null;
- `Players.Count`;
- safe local-player presence.

#### Ascension
- mode;
- arrows-visible;
- max/current ascension;
- hotkey binding counts before/after, if safely readable.

#### Stack
- submenu stack count;
- top/Peek identity;
- whether character logical `_stack` became the retained stack.

### Stage interpretation

This gives a practical fault classifier:

| Post-failure observation | Most likely stage |
|---|---|
| `_lobby == null` | lobby/service construction before assignment |
| lobby exists, ascension init markers absent | ascension panel / scene readiness |
| lobby exists, ascension complete, `Players.Count == 0` | save/unlock/add-player pre-insertion |
| `Players.Count == 1`, character logical `_stack == null` | `PlayerConnected`, later lobby update, or `AfterInitialized` |
| character logical `_stack == retained stack` | `InitializeSingleplayer` returned; failure is inside `Push` / `OnSubmenuOpened` / active-screen update |
| stack exact + visible true | failure occurred very late in Push, likely after `OnSubmenuOpened` |

This is much more informative than another trial patch.

---

## 10. What the next candidate should NOT do

Do not:

- directly write `_stack` or character-screen private fields;
- manually fabricate UI nodes;
- call `InitializeSingleplayer()` sub-stages separately as the first diagnostic;
- call `ExecuteDeferred()` simply to see whether the NRE disappears;
- call full `GameStartup()` on top of the existing ladder;
- set `RootSceneContainer.CurrentScene` merely because it differs;
- suppress the NRE;
- proceed to interaction/embark/run start before the real transition closes cleanly.

---

## 11. What should follow the forensic run

### If character/ascension `_Ready` fields are missing
Restore the lifecycle cause. The likely architectural question becomes why direct main-menu loading produced an incomplete child lifecycle. Prefer original menu loading/scene ownership over field patches.

### If SaveManager nested state is missing
Repair the exact normal save/startup provenance, not character-select code.

### If an NGame service property is missing
Repair NGame lifecycle/property ownership. Static and physical evidence currently makes this less likely.

### If `InitializeSingleplayer()` completes but Push fails
Prioritize restoration of original main-menu / `SetCurrentScene` semantics and active-screen ownership.

### If the handler succeeds
Proceed immediately to:
1. active character-select frame/input audit;
2. visible render residency;
3. then transition toward continuous Godot ownership rather than per-button steps.

---

## 12. Following architectural experiment: Startup Convergence Mode

After the fault is localized, the next major branch should be a **fresh-process alternate startup mode** approximating product architecture.

Instead of running Steps 35/36/42/45 and then direct menu reconstruction, it should:

1. load the verified prepared compatibility image;
2. start from the intended one-time-initialization state;
3. run a transformed normal `GameStartup`;
4. preserve original ordering and original `LaunchMainMenu`;
5. use exact compatibility substitutions only where necessary:
   - Null/iOS platform strategy;
   - local/offline cloud completion;
   - migrations deferred initially;
   - Steam stats/join deferred;
   - Sentry inert;
   - iOS display/window adaptation;
   - audio compatibility boundary;
   - AOT-safe no-op for `PrewarmJit`;
6. allow `LoadMainMenu` to use `RootSceneContainer.SetCurrentScene`;
7. preserve transformed deferred startup;
8. then give Godot a bounded render/input residency.

This mode should eventually replace the engineering ladder as the basis of the user-facing launcher.

---

## Decision for the next code iteration

**Recommended immediate iteration:** forensic character-select localization on the current physically closed ladder.

**Do not yet switch the phone test directly to transformed full `GameStartup`.**

Reason: the current NRE can now be localized cheaply and decisively, and that result will tell us whether the first startup-convergence code should focus on:

- scene readiness;
- save state;
- a game service;
- normal main-menu ownership;
- or a transformed deferred-startup prerequisite.

At the same time, implementation should be structured so the diagnostic logic is temporary engineering instrumentation—not permanent product-runtime behavior.
