# Release Checklist — Step 27

## Source / policy

- Steps 01–26 remain represented as closed/protected boundaries.
- Step 27 adds only a separate launcher-owned patch/unpatch subsystem, deterministic probe, host tests, isolated UI/reporting, and release wiring.
- No protected Step 23/24/25/26 behavior file is edited.
- `TrimMode=full`, `MtouchInterpreter=-all`, Step-22 roots, `System.Collections.Concurrent`, and the proven Step-25 constructor framework-preservation anchor remain active.
- StS2 member reflection/patching/invocation, broad Harmony patch discovery, Godot/game startup, and native game-library loading remain absent from the Step 27 boundary.

## Build identity

- expected version: `0.0.84 (84)`;
- workflow: `ios-step-27`;
- expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`;
- host TRX: `artifacts/test-results/step27.trx`.

## Pre-device authority

Require:

1. static validation PASS;
2. host tests PASS;
3. iOS publish PASS;
4. IPA verification PASS.

Do not install a candidate with any CI failure.

## Physical run

Fresh process only. Run Step 27 A–Y in order and stop on first failure.

Expected final summary: **25/25 PASS**.

Then run:

- OfflineReady = **PASS**;
- Foundation = **5/5 PASS**.

## Failure interpretation

- A–N failure: regression in a physically closed prerequisite; do not advance.
- Gate O failure: patch API/metadata shape drift; no patch descriptor or replacement was created.
- Gate P/Q failure: launcher-owned target/prefix preservation or baseline behavior problem; no patch descriptor/replacement was created.
- Gate R failure: prefix-description boundary remains open; `Patch()` was not attempted.
- Gate S failure: first real Harmony patch-engine boundary remains open; do not infer patched behavior.
- Gate T failure: a patch may exist, but patched execution is intentionally not attempted because isolation/integrity was not proven.
- Gate U failure: patch installation completed but deterministic patched execution is not proven.
- Gate V/W failure: exact removal or post-removal integrity remains open; do not infer restoration.
- Gate X failure: removal completed but restored behavior is not proven.
- Gate Y failure: patch/unpatch behavior may be demonstrated, but Step 27 is not closed because final integrity/isolation was not proven.

After Gate S or later, force-quit before any retry or earlier fresh-process runtime regression.
