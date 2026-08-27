# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 32 report

`Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`

0.0.119 keeps the Step-32 runtime report path and 6+4 rewrite semantics unchanged while changing only the bounded Cecil write-time constant-metadata resolver. The latest physical report remains the 0.0.117 Gate-A PASS / Gate-B Sentry-scope fail-closed result preserved below; 0.0.119 requires new physical A–D evidence after Codemagic passes.

## Static Step 32 metadata evidence

- `docs/history/reports/STEP-32-STATIC-STS2-CONSTANT-METADATA-AUDIT.txt` — static-only audit of the exact Step-32 receipt-backed `sts2.dll`: raw Constant-table coverage, exact non-null external type/storage requirements, Sentry provider identities, and the null-only GodotSharp/System.Collections distinction. The DLL was not executed, CLR-loaded, or modified.

## Physical and closed evidence

- `docs/history/reports/STEP-32.0.2-PHYSICAL-SENTRY-CONSTANT-METADATA-FAILURE.txt` — raw physical 0.0.117 report: Gate A PASS, Gate B rejected exact `Sentry 5.0.0.0` external constant metadata before mutation, no CLR admission.
- `docs/history/reports/STEP-32.0.1-PHYSICAL-CECIL-WRITE-RESOLUTION-FAILURE.txt` — raw physical 0.0.116 Step-32 report: Gate A PASS, Gate B `System.Runtime 9.0.0.0` Cecil Constant-table write-resolution failure, no CLR admission.
- `docs/history/reports/STEP-31.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.114 Step-31 4/4 report.
- `docs/history/steps/STEP-31.0-PHYSICAL-CLOSURE.md` — formal Step-31 closure and rewrite-design authorization boundary.
- `docs/history/reports/STEP-30.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.113 4/4 report.
- `docs/history/reports/STEP-29.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.112 4/4 report.
- `docs/history/reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt` — raw physical 0.0.111 5/5 report.
- `docs/history/reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt` — decisive negative runtime-Harmony evidence.
