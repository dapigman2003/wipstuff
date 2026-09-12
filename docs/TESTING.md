# Testing — Steps 58–64 character-select admission/render continuation / 0.0.190

Active candidate: `0.0.190 (190)`, IPA `StS2-Launcher-Steps-58-64.ipa`, workflow `ios-canonical`.

Static/container validation proves source wiring, hashes, fail-stop sequencing, one-shot guards, evidence surfaces, release identity, provenance and payload/security policy. Codemagic is compile/AOT/link/package authority. Physical iPhone reports are runtime authority.

## Physical sequence

Start from a **fresh process** and run **Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52**. Require 4/4 with rendering frozen. Then run **53 → 54 → 55 → 56 → 57** to reconstruct the physically closed Step-57 authority.

Continue, stopping immediately on the first failure:

1. **58** — exact `GetSubmenuType<NCharacterSelectScreen>()` factory map; no invocation.
2. **59** — one-shot frozen off-tree factory invocation + actual off-tree hierarchy/lifecycle audit.
3. **60** — exact zero-arg `InitializeSingleplayer()` map; no invocation.
4. **61** — one-shot frozen off-tree `InitializeSingleplayer()` invocation.
5. **62** — exact `NSubmenuStack.Push(NSubmenu)` + tree-entry lifecycle map; no push.
6. **63** — one-shot frozen exact `Push` admission; require visible/in-tree exact screen.
7. **64** — actual in-tree frame/input audit + one bounded 750 ms render residency + synchronous refreeze.

Never retry Steps **59, 61, 63, or 64** in-process after their one-shot boundary is armed. Steps 58/60/62 and all pre-action gates keep rendering frozen. Step 64 must write its map before `StartRendering()`, and `StopRendering()` must occur on the first managed continuation before post-stop telemetry/file I/O.

The original `OpenCharacterSelect(NButton)` handler must not be invoked anywhere in 0.0.190. No character selection/confirm/embark/run-start or Step 65 behavior is authorized.

## Expected evidence surfaces

Every rung writes `StepNN-CrashCheckpoint-<RunId>.txt`, a distinct `StepNN-...-StaticMap-<RunId>.txt`, `StepNN-LastCheckpoint.txt`, and a final `StepNN-TransformedRealStS2....txt` report. Preserve all four for the first failure, and preserve Step 64's four files if the entire block succeeds.

## Codemagic performance telemetry

The 0.0.183 AOT cache/sentinel telemetry remains unchanged. It is independent of the Steps 58–64 runtime experiment.
