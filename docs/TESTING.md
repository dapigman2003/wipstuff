# Testing — Steps 58–62 forensic character-select localization / visible-render trial / 0.0.195

Active candidate: `0.0.195 (195)`, IPA `StS2-Launcher-Steps-58-62.ipa`, workflow `ios-canonical`.

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

The original real `OpenCharacterSelect(NButton)` handler is permitted **only in Step 59 and only when the observed screen is not already visibly/logically complete**. 0.0.195 is diagnostic: it must not add character-select field repair or manually invoke `InitializeSingleplayer` substages. A null retained single-player logical stack may still be repaired only through the exact audited game-owned `NSubmenuStack.Push(NSubmenu)` path; direct `_stack` writes are forbidden. The reflection boundary must preserve/log the original inner exception stack rather than `throw inner`. No character choice, confirm, embark, run-start, or Step 63 behavior is authorized.

## Expected evidence surfaces

Every rung writes `StepNN-CrashCheckpoint-<RunId>.txt`, a distinct `StepNN-...-StaticMap-<RunId>.txt`, `StepNN-LastCheckpoint.txt`, and a final `StepNN-TransformedRealStS2....txt` report. Preserve all four for the first failure. If all five rungs succeed, preserve Step 62's four files plus any Step 58–61 static maps useful for the next design.

## Codemagic performance telemetry

The 0.0.183 AOT cache/sentinel telemetry remains unchanged and independent of this runtime experiment.
