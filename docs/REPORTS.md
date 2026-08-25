# Diagnostic Reports

Current on-device diagnostics write text reports beneath `Documents/StS2Launcher/Reports/*.txt` and are visible through Files. Reports are output-only and never trusted runtime input.

## Active Step 29 report

`Documents/StS2Launcher/Reports/Step29-RealStS2CompatibilityTargetAudit.txt`

It records ordered Gates A–D: OfflineReady + exact receipt-backed ARM64 `sts2.dll` metadata admission, concrete compatibility-risk IL call-site fingerprints, deterministic at-most-one candidate selection, and final source-hash/OfflineReady/no-CLR-load isolation.

## Physically closed Step 28 evidence

Raw device report:

`docs/history/reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt`

Closure note:

`docs/history/steps/STEP-28.0.2-PHYSICAL-CLOSURE.md`

The report records A–E **5/5 PASS**, Gate-D **1000 / 1041 / 1041**, exactly one transformed fixture identity in the CLR, and post-execution OfflineReady **428/428**.

## Preserved Step-28 CI evidence

- `docs/history/reports/STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt` — 0.0.109 CS0246 compile stop.
- `docs/history/reports/STEP-28.0.1-CODEMAGIC-HOST-TEST-FAILURE.txt` — 0.0.110 compile pass / host 216/217 / Gate-A eager Cecil `System.Runtime` resolution.

## Preserved Step-27 architecture-decision evidence

`docs/history/reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt` remains the decisive negative runtime-Harmony result.
