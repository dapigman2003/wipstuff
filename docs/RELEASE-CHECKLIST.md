# 0.0.205 PropertyTweener observe/repair release checklist

- Bundle identity must be **0.0.205 (205)**.
- 59T CONTROL must explicitly keep repair disabled while recording native pointer, managed-null/wrong-type counts, and last managed runtime type.
- 59X REPAIR must explicitly enable repair; a constructed `PropertyTweener(IntPtr)` requires a nonzero native pointer.
- `SetEase`, `SetTrans`, and `FromCurrent` use only method-local switchable return-self compatibility; their shared NativeCalls helpers remain untouched.
- 59E and real Step 59 explicitly enable the same repair in their fresh processes.
- Same IPA retains 59D/R/H/U/T/X/E, real Step 59, and 60–62.
- The active VeryEarlyInitialization may change only for the bounded GodotSharp PropertyTweener compatibility; the proven PackedScene compatibility slice is separately hash-pinned unchanged.
- No `Step59RuntimeTrace`, `ConfirmButtonCheckpointBridge`, or `STEP59TRACE_` runtime instrumentation may exist in active source.
- The proven PackedScene instanced-root unique-name compatibility implementation must remain byte-identical to 0.0.203.
- GodotSharp `Tween.TweenProperty` compatibility may specialize only its dedicated one-callsite `Godot.NativeCalls` helper: normal managed return unchanged; native pointer zero remains null; managed-null with nonzero pointer may construct exact `Godot.PropertyTweener(IntPtr)`.
- Shared `PropertyTweener.SetEase` / `SetTrans` / `FromCurrent` NativeCalls helpers must remain unpatched controls.
- 59T/59E/static diagnostic state must expose PropertyTweener fallback count and last native pointer.
- Step 58 must unlock from same-process Step 52 authority and call `RunStep58CurrentOwnershipAcquisition`; legacy Steps 53–57 are not prerequisites.
- Step 58 acquisition may invoke exact `OpenSingleplayerSubmenu()` once only when the retained submenu is absent/inactive; it must keep rendering frozen and never invoke `OpenCharacterSelect`.
- Step 59 branches 59D/59R/59H/59U/59T/59E and real 59 must all be present in the same IPA.
- 59H/59U/59T/59E/real 59 are fresh-process experiments.
- Step 63 remains unopened.

## Historical contracts retained below

# Release checklist — Steps 58–62 unique-name provenance forensics / visible-render trial / 0.0.198

Release identity: display/build `0.0.201 (201)`, IPA `StS2-Launcher-Steps-58-62.ipa`, workflow `ios-canonical`.

Require physical authority through **Step 57 4/4/frozen** to be preserved, including final Step-56/57 pass artifacts plus the physical 0.0.190–0.0.192 Step-58 localization evidence and physical 0.0.193 Step-59 localization evidence. The Physically Closed Path remains fresh-process-only and capped at Step 52.

Require the new block to preserve exact sequencing and authority:

- Step 58 performs a non-mutating runtime ownership audit: exact handler/signature/token/IL, proof the explicit `NButton` parameter is unread, whole-handler admissibility under retained guards, and exact cache/type/load-context/stack/tree observations.
- Step 59 first checkpoints a read-only forensic prerequisite surface: character/ascension readiness, critical `_Ready` fields, NGame services, SaveManager progress internals, RootSceneContainer current-scene identity, and pre-handler lobby state. It rejects foreign single-player logical-stack authority and uses the already-audited game-owned Push only if that stack is actually null. It then invokes exact real `OpenCharacterSelect(NButton)` once with the retained real `_standardButton`. Any inner exception must have its game-side `TargetSite`/original stack durably recorded, followed by a post-failure lobby/player/screen snapshot, before failure returns. No direct `_stack` or character-select field write is allowed.
- Step 60 audits the actual active screen's frame/input callbacks and exact trusted-PCK TSCN connection inventory without rendering or interaction.
- Step 61 authorizes exactly one 2-second visible render residency, evidence ceiling 10 seconds, then synchronously refreezes before success evaluation.
- Step 62 authorizes exactly one 10-second visible render residency, evidence ceiling 30 seconds, then synchronously refreezes before success evaluation.

Step 59 is one-shot once its navigation repair and/or handler transition arms. Steps 61/62 are always one-shot after render arm. Character choice/confirm/embark/run-start, whole `GameStartup`/original `LaunchMainMenu`, cloud/migration/platform/Steam/deferred startup, native game extensions, trusted-install mutation, and Step 63+ remain unopened.

Require exact 0.0.197 source/plist/release/shell identity; current active manifests regenerated; protected historical evidence unchanged; no proprietary StS2/native payload in source archive; `history.zip` rebuilt from `docs/history`; final ZIP integrity clean; canonical validator green in the release tree and fresh extraction.


## 0.0.196 forensic-ready requirements

- Preserve the physical 0.0.195 Step-59 failure evidence in history.
- Selected `_Ready` IL must prove `_charButtonContainer` store precedes `%AscensionPanel`, which precedes
  `_ascensionPanel` store.
- Step 59 must collect live ascension-like node type/load-context/owner/UniqueNameInOwner evidence.
- Step 59 must read both character-select and ascension-panel TSCN bytes only from the receipt-backed sealed PCK.
- An unhealthy ready binding must block Gate C before `_step59TransitionStarted` is set.
- No direct `_ascensionPanel` write and no explicit/manual `_Ready()` invocation is permitted.


## 0.0.197 gate-name contract

- Step-59 Core and UI must share `TransformedRealStS2VeryEarlyInitialization.Step59GateName`.
- The obsolete UI gate-constructor literal `FORENSIC REAL OPENCHARACTERSELECT FROZEN TRANSITION` must not remain.
- Preserve physical 0.0.196 harness-failure evidence.
- No game-facing 0.0.196 ready-binding diagnostic behavior is broadened in this patch.

## 0.0.198 unique-name provenance requirements

- Preserve the complete physical 0.0.197 Step-59 report/checkpoint/last-checkpoint/static-map evidence.
- Step 59 must query retained PackedScene `GetState()` node/property data for exact unique-name targets.
- Step 59 may create only temporary off-tree diagnostic instances; they must never enter the SceneTree and must be released.
- Compare ordinary unique nodes and instantiated-subscene roots on NGame/main-menu as systemic controls.
- Compare direct/non-generic node lookup and `%Name` lookup identity on the same live graph.
- No setter for `UniqueNameInOwner`, no owner mutation, no direct game-field repair and no explicit `_Ready()` call.
- Existing unhealthy prerequisites still block Gate C before one-shot Push/OpenCharacterSelect arm.


## 0.0.201 NConfirmButton OnEnable-preflight requirements

- The synthetic GodotSharp host fixture must define the instance `Godot.PackedScene.Instantiate` surface and verify one compatibility field load/invoke/return epilogue.
- MODEL-BOOTSTRAP live status/checkpoint text must identify the verified GodotSharp compatibility derivative as the selected bridge authority, while exact prepared GodotSharp remains immutable source authority.

- The exact prepared/trusted GodotSharp file is never modified in place.
- The selected derivative preserves GodotSharp assembly identity and MVID.
- `PackedScene.Instantiate(GenEditState)` contains exactly one compatibility callback invocation after native
  instantiation and before return.
- The callback considers only SceneState nodes with non-null `GetNodeInstance(index)` and exact
  `unique_name_in_owner=true`.
- Ordinary/non-instanced nodes are not written.
- Runtime nodes are matched by serialized relative path using `GetChildren()`/`Name`; the correction does not depend
  on the reflection-time `Godot.NodePath` lookup that produced diagnostic TypeLoad noise in 0.0.198.
- No StS2 private UI/game field is written and no `_Ready()` method is manually invoked.
- Physical 0.0.198 report/checkpoint/static-map/last-checkpoint evidence is retained in history.
- Physical 0.0.200 Step-59 report/checkpoint/static-map/last-checkpoint evidence is retained in history.
- Step 59 Gate B emits `M59_B_CONFIRM_BUTTON_PREFLIGHT` before handler arm and statically verifies selected `NConfirmButton._Ready`, `NConfirmButton.OnEnable`, `NButton.OnEnable`, and `NButton.RegisterHotkeys`.
- A dedicated **Step 59D** control runs only Step-59 Gates A+B, writes `Step59-DeepDiagnosticDeck-StaticMap-*`, emits `M59D_B_STATIC_MAP_WRITE_RETURNED`, and never calls `RunStep59RealHandlerTransition`.
- The Step59D map includes a full inherited embark field matrix, whole-SceneTree `NConfirmButton` peer matrix, observational accessor snapshot, null-field live-node candidate matrix, exact IL field/call order, and transitive execution frontiers for the immediate Ready/Enable chain.
- The normal Step-59 real-handler control refuses same-process continuation after Step59D; a fresh process is required so the two experiments can reuse one compiled IPA without sharing mutated/diagnostic state.
- Any real-handler failure captures `M59_C_CONFIRM_BUTTON_POSTFAIL` and refreshes the durable static map before UI return with `[POST-FAILURE CONFIRM-BUTTON RUNTIME DECK]`.
- Live `_embarkButton` `_outline`, `_buttonImage`, `_viewport`, and `_hotkeys` are non-null prerequisites; any definite failure blocks Gate C without direct repair.
- No direct NConfirmButton field write, manual `_Ready()`, or `OnEnable` bypass is introduced.
