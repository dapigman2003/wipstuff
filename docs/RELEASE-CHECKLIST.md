# Release Checklist — Step 26

## Source / policy

- Steps 01–25 remain represented as closed/protected boundaries.
- Step 26 adds only the separate controlled empty `PatchProcessor` subsystem, launcher-owned inert probe, host tests, isolated UI/reporting, and release wiring.
- No protected Step 23/24/25 runtime file is edited.
- `TrimMode=full`, `MtouchInterpreter=-all`, Step-22 roots, `System.Collections.Concurrent`, and the proven Step-25 constructor framework-preservation anchor remain active.
- `PatchProcessor.Patch`, `Harmony.Patch/PatchAll`, `HarmonyMethod` creation, StS2 member reflection/invocation, Godot/game startup, and native game-library loading remain absent from the Step 26 boundary.

## Build identity

- expected version: `0.0.83 (83)`;
- workflow: `ios-step-26`;
- expected IPA: `artifacts/StS2-Launcher-Step-26.ipa`;
- host TRX: `artifacts/test-results/step26.trx`.

## Pre-device authority

Require:

1. static validation PASS;
2. host tests PASS;
3. iOS publish PASS;
4. IPA verification PASS.

Do not install a candidate with any CI failure.

## Physical run

Fresh process only. Run Step 26 A–N in order and stop on first failure.

Expected final summary: **14/14 PASS**.

Then run:

- OfflineReady = **PASS**;
- Foundation = **5/5 PASS**.

## Failure interpretation

- A–I failure: regression in a physically closed prerequisite; do not advance.
- Gate J failure: processor API/metadata shape drift; no new processor execution occurred.
- Gate K failure: exact PatchProcessor type-initialization boundary remains open.
- Gate L failure: launcher-owned target reflection/preservation problem; no Harmony processor was created.
- Gate M failure: exact empty processor-construction boundary remains open; `Patch()` was not attempted.
- Gate N failure: processor object may exist, but Step 26 is not closed because post-boundary integrity/isolation was not proven.
