# StS2 Launcher iOS — Canonical Foundation

Experimental unofficial iOS launcher/compatibility host for **Slay the Spire 2** for legitimate owners. The repository does not include game payloads, Steam secrets, Apple signing secrets, or proprietary FMOD/Spine payloads.

## Current boundary

Steps 01–22 are physically closed on iPhone. Step 22.4.2 is the final canonical-foundation regression-correction candidate before the first controlled real `sts2.dll` CLR load. It preserves the Step 22 runtime boundary and corrects the stale pre-Step-20 Step 19 dynamic-code assertion.

Read **`docs/MASTER-PLAN.md` first** for architecture, roadmap, safety rules, and the resumption protocol. `docs/CURRENT-STATUS.md` contains the current physical/candidate state.

## Build

Codemagic workflow:

`ios-step-22-4-2`

Authoritative pipeline entry point:

```sh
bash scripts/codemagic.sh
```

## Canonical source layout

- `src/StS2Launcher.Core/` — shared launcher/Steam/compatibility/runtime logic
- `src/StS2Launcher.iOS/` — the one live iOS application project
- `tests/StS2Launcher.Core.Tests/` — host regression tests
- `fixtures/` — project-owned regression fixtures
- `native/` — project-owned Godot host/smoke source
- `scripts/` — current build/test/validation entry points only
- `tools/` — patcher and validation support
- `docs/` — authoritative plan/current docs/history
- `history.zip` — optional inert reference archive; never a build dependency

The old `StS2Launcher.Step05.iOS` project name is historical and no longer appears in live source/tooling.
