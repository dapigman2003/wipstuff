# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 34 report

`Documents/StS2Launcher/Reports/Step34-TransformedRealStS2PrewarmJitExecution.txt`

0.0.122 is the first controlled transformed-real-game execution boundary. Gate A re-runs and re-verifies the closed Step-32 transform and prepared runtime plan. Gate B re-establishes transformed-primary-only CLR admission in `StS2Launcher-Step34-PrewarmJit`. Gate C binds only exact transformed `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()` and invokes it once under the strict resolver. Gate D re-proves source/transformed/plan/dependency/context isolation. Preserve any first failure exactly; especially preserve Gate-C target exception and resolver/native state.

## Latest physical closures

- `docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt` — authoritative physical 0.0.121 Step-33 report: A–D 4/4 PASS; exact transformed-primary-only CLR admission; zero managed/private/native admission-time expansion; receipt-backed/prepared original excluded; no game-member invocation.
- `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt` — authoritative physical 0.0.120 Step-32 report: A–D 4/4 PASS; exact 6+4 private rewrite; transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`; zero PrepareMethod references after reopen; source/trusted install unchanged; zero real-StS2 CLR admission.
- `docs/history/reports/STEP-32.0.4-PHYSICAL-GATE-C-TRANSFORMED-METHOD-IDENTITY-FAILURE.txt` — prior physical 0.0.119 2/4 evidence that motivated stable post-write method identity.

## Current Codemagic evidence

The first cache-tuned 0.0.121 attempt did not reach iOS publish: all configured caches were cold and host-test compilation stopped on removed MSTest `Assert.ThrowsException`. See `docs/history/reports/STEP-33.0-CODEMAGIC-COLD-CACHE-HOST-COMPILE-FAILURE.txt`. The corrected 0.0.121 candidate subsequently built and physically closed Step 33 at 4/4. The active 0.0.122 candidate keeps the stable `ios-canonical` workflow and the same NuGet/Godot/iOS-arm64 `obj` cache paths so AOT cache reuse can continue without changing runtime policy.

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
