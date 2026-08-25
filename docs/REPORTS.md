# Diagnostic Reports

Current on-device diagnostics write text reports beneath `Documents/StS2Launcher/Reports/*.txt` and are visible through Files. Reports are output-only and never trusted runtime input.

## Active Step 30 report

`Documents/StS2Launcher/Reports/Step30-SelectedTargetSemanticContextAudit.txt`

It records exact Step-29 evidence binding, bounded IL/control-flow/exception context for the selected `ModManager.TryLoadMod -> Harmony.PatchAll` site, deterministic product-scope disposition, and final source-hash/OfflineReady/no-CLR-load isolation.

## Physically closed Step 29 evidence

- `docs/history/reports/STEP-29.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.112 report, 4/4 PASS.
- `docs/history/steps/STEP-29.0-PHYSICAL-CLOSURE.md` — closure note and selected exact fingerprint.

## Physically closed Step 28 evidence

- `docs/history/reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt` — raw 0.0.111 report, 5/5 PASS with 1000 / 1041 / 1041 and OfflineReady 428/428.
- `docs/history/steps/STEP-28.0.2-PHYSICAL-CLOSURE.md` — formal positive closure note.

## Preserved Step-27 architecture-decision evidence

`docs/history/reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt` remains the decisive negative runtime-Harmony result.
