# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 32 report

`Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`

0.0.120 keeps the Step-32 runtime report path, 6+4 rewrite semantics, and exact audited System.Runtime/Sentry resolver authority unchanged. Physical 0.0.119 advanced Step 32 to 2/4: Gate A and Gate B passed, then Gate C failed before semantic verification because the transformed verifier reused the source MethodDef token as a post-Cecil-write locator. 0.0.120 changes only that Gate-C locator to exact declaring type + full signature and adds token-drift diagnostics.

## Current physical frontier

- `docs/history/reports/STEP-32.0.4-PHYSICAL-GATE-C-TRANSFORMED-METHOD-IDENTITY-FAILURE.txt` — raw physical 0.0.119 report: Gate A PASS, Gate B PASS with the first real-StS2 private 6+4 serialization, Gate C failed at the old source-token-based transformed method identity/body check before semantic or Constant-table reopen verification.

## Current Codemagic evidence

- `docs/history/reports/STEP-32.0.4-CODEMAGIC-HOST-FIXTURE-FAILURE.txt` — first 0.0.119 Codemagic attempt: static validation 669/669 and compile succeeded; host suite reached 183/186, with all three failures caused by the image-less synthetic `System.Runtime` fixture asking Cecil for `TypeSystem.Int32` before the intended Step-32 tests ran. Production resolver/rewrite semantics were not changed; rerun the same 0.0.119 candidate after the fixture-only correction.

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
