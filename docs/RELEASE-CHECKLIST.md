# Release checklist — Steps 58–62 unique-name provenance forensics / visible-render trial / 0.0.198

Release identity: display/build `0.0.198 (198)`, IPA `StS2-Launcher-Steps-58-62.ipa`, workflow `ios-canonical`.

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
