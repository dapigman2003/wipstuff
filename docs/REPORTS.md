# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 32 report

`Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`

For Step 32.0.4 / 0.0.119 it records exact source/evidence binding, private-clone provenance, the ten verified five-byte raw call windows, the 6/6 + 4/4 stack-neutral `Pop/Nop` replacements, whole-image byte-diff confinement, reopened 10→0 `PrepareMethod` verification, source/transformed semantic fingerprints, and final OfflineReady/no-CLR-load isolation. Cecil remains the binding/reopen-verification authority; this candidate performs no Cecil whole-module serialization.

## Codemagic reports

Both `step32-fast` and `ios-step-32` emit `artifacts/reports/phase-timings.txt` and `artifacts/reports/cache-sizes.txt` so the free M2 minute cost can be measured rather than guessed. `step32-fast` also emits `fast-preflight-summary.txt`; `ios-step-32` emits the iOS workload/build/IPA authority reports. Run the device workflow only after the fast workflow passes on the exact same commit.

## Preserved Step 32 evidence

- `docs/history/reports/STEP-32.0-CODEMAGIC-HOST-TEST-FAILURE.txt` — 0.0.115 compiled and reached 230/231 host tests; the sole failure exposed the invalid pre-serialization offset-sensitive fingerprint check.
- `docs/history/reports/STEP-32.0-CODEMAGIC-STATIC-VALIDATION.txt` — preserved 0.0.115 static authority report.
- `docs/history/reports/STEP-32.0.1-PHYSICAL-CECIL-WRITE-RESOLUTION-FAILURE.txt` — physical 0.0.116: Gate A PASS; Gate B stopped when Cecil whole-module serialization requested `System.Runtime 9.0.0.0` Constant-table metadata; no CLR admission.
- `docs/history/reports/STEP-32.0.2-PHYSICAL-UNEXPECTED-CONSTANT-SCOPE-FAILURE.txt` — physical 0.0.117: Gate A PASS; Gate B failed closed before writing after the bounded pre-scan discovered unrelated `Sentry 5.0.0.0` constant metadata.
- `docs/history/reports/STEP-32.0.3-CODEMAGIC-FAST-HOST-TEST-FAILURE.txt` — 0.0.118 fast preflight: 1027/1027 static PASS; complete host suite 230/231; exact-length Step-32 Gate B succeeded in the fixture and the only failure was a stale test-only detail-string assertion. No device workflow was run.
- `docs/history/reports/STEP-32.0.3-CODEMAGIC-FAST-PHASE-TIMINGS.txt` — raw 0.0.118 timing proof: SDK 13s, static 1s, complete host suite 13s; failure stopped before iOS work.

## Earlier physically closed evidence

- `docs/history/reports/STEP-31.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.114 Step-31 4/4 report.
- `docs/history/steps/STEP-31.0-PHYSICAL-CLOSURE.md` — formal Step-31 closure and rewrite-design authorization boundary.
- `docs/history/reports/STEP-30.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.113 4/4 report.
- `docs/history/reports/STEP-29.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.112 4/4 report.
- `docs/history/reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt` — raw physical 0.0.111 5/5 report.
- `docs/history/reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt` — decisive negative runtime-Harmony evidence.
