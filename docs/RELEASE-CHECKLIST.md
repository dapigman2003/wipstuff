# Release checklist — Steps 58–62 forensic character-select localization / visible-render trial / 0.0.195

Release identity: display/build `0.0.195 (195)`, IPA `StS2-Launcher-Steps-58-62.ipa`, workflow `ios-canonical`.

Require physical authority through **Step 57 4/4/frozen** to be preserved, including final Step-56/57 pass artifacts plus the physical 0.0.190–0.0.192 Step-58 localization evidence and physical 0.0.193 Step-59 localization evidence. The Physically Closed Path remains fresh-process-only and capped at Step 52.

Require the new block to preserve exact sequencing and authority:

- Step 58 performs a non-mutating runtime ownership audit: exact handler/signature/token/IL, proof the explicit `NButton` parameter is unread, whole-handler admissibility under retained guards, and exact cache/type/load-context/stack/tree observations.
- Step 59 first checkpoints a read-only forensic prerequisite surface: character/ascension readiness, critical `_Ready` fields, NGame services, SaveManager progress internals, RootSceneContainer current-scene identity, and pre-handler lobby state. It rejects foreign single-player logical-stack authority and uses the already-audited game-owned Push only if that stack is actually null. It then invokes exact real `OpenCharacterSelect(NButton)` once with the retained real `_standardButton`. Any inner exception must have its game-side `TargetSite`/original stack durably recorded, followed by a post-failure lobby/player/screen snapshot, before failure returns. No direct `_stack` or character-select field write is allowed.
- Step 60 audits the actual active screen's frame/input callbacks and exact trusted-PCK TSCN connection inventory without rendering or interaction.
- Step 61 authorizes exactly one 2-second visible render residency, evidence ceiling 10 seconds, then synchronously refreezes before success evaluation.
- Step 62 authorizes exactly one 10-second visible render residency, evidence ceiling 30 seconds, then synchronously refreezes before success evaluation.

Step 59 is one-shot once its navigation repair and/or handler transition arms. Steps 61/62 are always one-shot after render arm. Character choice/confirm/embark/run-start, whole `GameStartup`/original `LaunchMainMenu`, cloud/migration/platform/Steam/deferred startup, native game extensions, trusted-install mutation, and Step 63+ remain unopened.

Require exact 0.0.195 source/plist/release/shell identity; current active manifests regenerated; protected historical evidence unchanged; no proprietary StS2/native payload in source archive; `history.zip` rebuilt from `docs/history`; final ZIP integrity clean; canonical validator green in the release tree and fresh extraction.
