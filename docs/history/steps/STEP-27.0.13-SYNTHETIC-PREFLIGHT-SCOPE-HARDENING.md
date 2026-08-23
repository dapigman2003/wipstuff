# Step 27.0.13 — Synthetic preflight scope hardening

Candidate: `0.0.97 (97)`

## Trigger

Codemagic for 0.0.96 compiled successfully and executed all 211 host tests. Two tests failed at Gate A:

- `SyntheticStep26ReplayThroughEmptyProcessorStillPassesBeforePatchBoundary`
- `GateCReportsThrowingModuleInitializerAndDoesNotAdvance`

Both failures were caused by the new `CreateIosNormalizedHarmonyRuntimeImage` path being applied unconditionally to randomized minimal Harmony-like fixtures. Those fixtures intentionally model only the public Harmony/processor surface needed through Gates A–N and do not contain the full internal real-Harmony patch-engine graph audited for the production compatibility rewrite.

## Correction

Gate A now distinguishes the canonical production target from internal synthetic replay:

- exact `0Harmony` + `2.4.2.0` -> full patch-engine fingerprint audit + byte-distinct 11-instruction normalized `HarmonySharedState::.cctor` runtime image;
- non-canonical internal synthetic target -> byte-identical runtime-image passthrough, with an explicit invariant that source/runtime SHA-1 remain equal.

The public constructor remains pinned to `TargetSimpleName` and `TargetVersion`, so no public production bypass is introduced. The canonical target still fails closed if its real patch-engine fingerprint drifts or normalization is not byte-distinct.

## Runtime behavior

No intended production runtime behavior changes from 0.0.96. Gate S, normalized Gate T T5a/T5b/T6, and the single public `PatchProcessor.Patch()` acceptance boundary are unchanged.

## Evidence

The full 0.0.96 Codemagic host report is preserved at `docs/history/reports/STEP-27.0.12-CODEMAGIC-HOST-TEST-FAILURE.txt`.
