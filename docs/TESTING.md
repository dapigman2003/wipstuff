# Testing — Steps 58–62 unique-name provenance forensics / visible-render trial / 0.0.198

Active candidate: `0.0.199 (199)`, IPA `StS2-Launcher-Steps-58-62.ipa`, workflow `ios-canonical`.

Static/container validation proves source wiring, hashes, fail-stop sequencing, one-shot guards, evidence surfaces, release identity, provenance and payload/security policy. Codemagic is compile/AOT/link/package authority. Physical iPhone reports are runtime authority.

## Physical sequence

Start from a **fresh process** and run **Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52**. Require 4/4 with rendering frozen. Then run **53 → 54 → 55 → 56 → 57** to reconstruct the physically closed Step-57 authority.

Continue, stopping immediately on the first failure:

1. **58** — runtime ownership audit only. Observe the exact cached character-select state and audit exact `OpenCharacterSelect(NButton)` without invoking it.
2. **59** — before mutation, checkpoint a forensic prerequisite snapshot covering character/ascension `IsNodeReady`, critical `_Ready`-bound fields, NGame hotkey/input/remote-cursor/reaction/timeout services, production SaveManager `Progress/Epochs/EncounterStats`, RootSceneContainer current-scene identity, and current lobby state. Then invoke original `OpenCharacterSelect(NButton)` once with the retained real `_standardButton` (using the already-audited Push repair only if the retained single-player logical stack is actually null). If it throws, require durable inner `TargetSite`/original-stack evidence plus a post-failure lobby/player/screen snapshot.
3. **60** — audit the actual active character-select frame/input surface and exact TSCN connection inventory; no rendering or interaction.
4. **61** — one-shot 2-second visible render residency, then synchronous refreeze. Do not intentionally tap or interact.
5. **62** — one-shot 10-second visible render residency, then synchronous refreeze. Do not intentionally tap or interact.

Never retry Step **59** after its navigation-repair/handler transition is armed, or Steps **61/62** after their render boundary is armed. Step 58/60 keep rendering frozen. Steps 61/62 must call `StopRendering()` before post-stop telemetry/file I/O.

The original real `OpenCharacterSelect(NButton)` handler is permitted **only in Step 59 and only when the observed screen is not already visibly/logically complete**. 0.0.196 is diagnostic: it must not add character-select field repair or manually invoke `InitializeSingleplayer` substages. A null retained single-player logical stack may still be repaired only through the exact audited game-owned `NSubmenuStack.Push(NSubmenu)` path; direct `_stack` writes are forbidden. The reflection boundary must preserve/log the original inner exception stack rather than `throw inner`. No character choice, confirm, embark, run-start, or Step 63 behavior is authorized.

## Expected evidence surfaces

Every rung writes `StepNN-CrashCheckpoint-<RunId>.txt`, a distinct `StepNN-...-StaticMap-<RunId>.txt`, `StepNN-LastCheckpoint.txt`, and a final `StepNN-TransformedRealStS2....txt` report. Preserve all four for the first failure. If all five rungs succeed, preserve Step 62's four files plus any Step 58–61 static maps useful for the next design.

## Codemagic performance telemetry

The 0.0.183 AOT cache/sentinel telemetry remains unchanged and independent of this runtime experiment.


## 0.0.196 Step-59 expected evidence

Gate B should now write `Step59-RealOpenCharacterSelect-StaticMap-<RunId>.txt` even when the ready-binding
prerequisites are unhealthy. Preserve `M59_B_FORENSIC_PREFLIGHT`, `M59_B_READY_BINDING_DIAGNOSIS`,
`M59_B_STATIC_MAP_WRITE_RETURNED`, and, when blocked, `M59_C_BLOCKED_READY_BINDING`.

A blocked Gate C in 0.0.196 is **not** a one-shot arm: `Step59TransitionStarted` must remain false and neither
`NSubmenuStack.Push` nor `OpenCharacterSelect` may be invoked. Do not proceed to Step 60 unless Step 59 closes 4/4.


## 0.0.197 rerun note

0.0.196 did not execute Gate B because of a Core/UI gate-name mismatch. 0.0.197 changes only that harness contract. Re-run Step 59 from a fresh process and expect `M59_B_READY_BINDING_DIAGNOSIS` plus a durable Step-59 static map before any Gate-C block/transition.

## 0.0.198 Step-59 expected evidence

Preserve `M59_B_UNIQUE_NAME_PROVENANCE` and the durable Step-59 static map. The map should contain character-select,
game and main-menu SceneState/runtime comparisons. In particular, classify `AscensionPanel` across:
TSCN `unique_name_in_owner=true`, SceneState `unique_name_in_owner`, temporary off-tree clone
`UniqueNameInOwner`, retained live `UniqueNameInOwner`, direct-path lookup and `%AscensionPanel` lookup.

Temporary diagnostic PackedScene instances must never be added to the SceneTree and must be released after inspection.
No `SetUniqueNameInOwner`, live owner mutation, direct `_ascensionPanel` write, manual `_Ready`, Push, or
`OpenCharacterSelect` is authorized when the existing ready blocker remains. Do not proceed to Step 60 unless Step 59
closes 4/4.


## 0.0.199 PackedScene compatibility expectations

The active model-bootstrap path now selects the verified GodotSharp compatibility derivative rather than exact
prepared GodotSharp bytes. During GodotSharp load, the checkpoint must include
`GODOT_PACKEDSCENE_UNIQUE_NAME_COMPAT_ARMED`.

Reconstruct from a **fresh process**. Earlier rungs may now observe healthier normal game lifecycle state because the
compatibility correction applies from the first real PackedScene instantiation. Stop on the first failure and preserve
that rung's evidence.

If execution reaches Step 59, `M59_B_UNIQUE_NAME_PROVENANCE` should show:

- character-select SceneState `AscensionPanel=True` / temporary instance `UniqueNameInOwner=True` / live `true`;
- character-select `ActDropdown` likewise `true`;
- game instanced roots such as `RootSceneContainer` and `MultiplayerTimeoutOverlay` `true`;
- ordinary-node controls remain unchanged;
- `_ascensionPanel`, `_actDropdown`, and `NGame.TimeoutOverlay` should no longer be null if their managed Ready
  callbacks complete normally.

Only then may Step 59 authorize the original game-owned transition. Do not retry an armed Step 59 in-process.
