# Release Checklist — Step 27.0.1

## Source / policy

- Steps 01–26 remain closed/protected.
- Physical Step 27.0 / 0.0.84 17/25 evidence is preserved.
- Step 27.0.1 adds only the explicit measured `HarmonyLib.AccessTools` type-initialization gate plus shifted later gate labels; the intended launcher-only patch boundary is unchanged.
- No protected Step 23/24/25/26 behavior file is edited.
- `TrimMode=full`, `MtouchInterpreter=-all`, Step-22 roots, `System.Collections.Concurrent`, and the proven Step-25 constructor framework-preservation anchor remain active.
- StS2 member reflection/patching/invocation, broad Harmony discovery, Godot/game startup, and native game-library loading remain absent.

## Build identity

- expected version: `0.0.85 (85)`;
- workflow: `ios-step-27`;
- expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`;
- host TRX: `artifacts/test-results/step27.trx`.

## Pre-device authority

Require static validation, host tests, iOS publish, and IPA verification all PASS. Do not install a candidate with any CI failure.

## Physical run

Fresh process only. Run Step 27 A–Z in order and stop on first failure. Expected final summary: **26/26 PASS**. Then require OfflineReady PASS and Foundation 5/5.

## Failure interpretation

- A–N: regression in a closed prerequisite.
- O: patch/AccessTools metadata drift; no new type initialization or patching occurred.
- P/Q: launcher probe metadata/baseline problem.
- R: explicit AccessTools initialization boundary remains open; no HarmonyMethod or patch should exist.
- S: prefix-description construction remains open; `Patch()` was not attempted.
- T: first real patch-engine boundary remains open.
- U: a patch may exist, but patched execution is intentionally withheld because integrity was not proven.
- V: patch installation completed but patched behavior is not proven.
- W/X: exact removal or post-removal integrity remains open.
- Y: removal completed but restored behavior is not proven.
- Z: behavior may be demonstrated, but Step 27 is not closed until final integrity/isolation passes.

After Gate T or later, force-quit before any retry or earlier fresh-process runtime regression.
