# Step 32.0.4 — Fast-Preflight Assertion + Failure-Telemetry Fix

Version: `0.0.119 (119)`

## Trigger

Codemagic 0.0.118 ran the new `step32-fast` workflow. Pinned .NET SDK setup took 13 seconds, canonical static validation passed **1027/1027** in 1 second, and the complete 231-test host suite ran in 13 seconds. Result: **230/231 PASS**. No iOS workload, publish, IPA, or physical test was run.

The sole failure was `ExactPrewarmJitPrepareMethodFamilyIsRewrittenOnPrivateCopyOnly`. The Step-32 implementation itself had already completed Gate B successfully in that test and reported:

- one-argument sites rewritten: 6/6;
- two-argument sites rewritten: 4/4;
- ten exact five-byte patch windows;
- no Cecil serialization;
- all differences confined to the approved windows.

The test then failed only because it expected the obsolete substring `PrepareMethod(handle, instantiation[]) -> Pop + Pop`. Production 0.0.118 correctly reports the exact equal-length contract as `PrepareMethod(handle, instantiation[]) 5-byte call -> Pop + Pop + Nop + Nop + Nop`.

Raw authority:

- `docs/history/reports/STEP-32.0.3-CODEMAGIC-FAST-HOST-TEST-FAILURE.txt`
- `docs/history/reports/STEP-32.0.3-CODEMAGIC-FAST-STATIC-VALIDATION.txt`
- `docs/history/reports/STEP-32.0.3-CODEMAGIC-FAST-PHASE-TIMINGS.txt`

## Correction

0.0.119 changes only the host regression assertion so it pins both exact padded five-byte detail strings:

- `PrepareMethod(handle) 5-byte call -> Pop + Nop + Nop + Nop + Nop`
- `PrepareMethod(handle, instantiation[]) 5-byte call -> Pop + Pop + Nop + Nop + Nop`

The production `RealStS2PrepareMethodRewrite` implementation, exact 6+4 transformation, PE/IL write mechanism, byte-diff confinement, Cecil binding/reopen verification, resolver policy, Step-32 A–D physical boundary, and fast/device authority split are unchanged.

The 0.0.118 failure also exposed a telemetry issue: `cache-sizes.txt` was not emitted because `set -e` stopped `codemagic-fast.sh` before its final cache-report call. 0.0.119 installs `sts2_report_cache_sizes` as an EXIT trap in both fast and device scripts so success and failure artifacts both retain cache-size evidence. This changes no build authority or transformation behavior.

## Authority rule

Run `step32-fast` first. If it does not reach the complete host-suite PASS, stop and do not run `ios-step-32`. Only a fast PASS on the exact commit authorizes the device workflow; only both CI passes authorize a physical install/test.

## Architecture status

Unchanged. This is a test-contract/release-identity correction only. `MASTER-PLAN.md` remains unchanged.
