# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 33 report

`Documents/StS2Launcher/Reports/Step33-TransformedRealStS2AssemblyAdmission.txt`

0.0.121 is an admission-only boundary. Gate A re-runs the physically closed Step-32 A–D contract, requires the exact closed transformed image/hash/semantic fingerprint, and requalifies the existing Step-21/22 zero-blocker runtime plan without CLR-loading StS2. Gate B loads only the exact transformed primary bytes into a dedicated private AssemblyLoadContext. Gate C requires transformed `sts2` to be the only private assembly and rejects private dependency/native/unplanned managed expansion. Gate D re-proves OfflineReady, original/transformed/plan hashes, and transformed-context residency.

## Latest physical closure

- `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt` — authoritative physical 0.0.120 Step-32 report: A–D 4/4 PASS; exact 6+4 private rewrite; transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`; zero PrepareMethod references after reopen; source/trusted install unchanged; zero real-StS2 CLR admission.
- `docs/history/reports/STEP-32.0.4-PHYSICAL-GATE-C-TRANSFORMED-METHOD-IDENTITY-FAILURE.txt` — prior physical 0.0.119 2/4 evidence that motivated stable post-write method identity.

## Current Codemagic evidence

The first cache-tuned 0.0.121 attempt did not reach iOS publish: all configured caches were cold and host-test compilation stopped on removed MSTest `Assert.ThrowsException`. The corrected 0.0.121 rerun must seed the iOS arm64 obj/AOT cache before cache reuse can be measured. See `docs/history/reports/STEP-33.0-CODEMAGIC-COLD-CACHE-HOST-COMPILE-FAILURE.txt`.

0.0.120 passed Codemagic before the successful physical 4/4 run. Step 33.0 / 0.0.121 must separately pass canonical static validation, the full host suite, iOS publish, and IPA verification before physical execution.

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
