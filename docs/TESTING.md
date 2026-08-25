# Testing — Step 31 PrepareMethod Semantic Context Audit

## Static validation

Run `bash scripts/validate.sh`.

The active candidate is Step 31.0 / `0.0.114 (114)`. Validation must preserve the physically closed Step-28/29/30 evidence while pinning the new read-only PrepareMethod semantic audit:

- Step 30 raw physical report remains present and records **4/4 PASS**, exact Harmony/mod disposition, zero writes/CLR load/resolver fallback, and OfflineReady 428/428;
- Step 31 hard-pins the receipt-backed source SHA/MVID, `PrewarmJit()` token `0x06007D05`, body fingerprint, and all ten Step-29 `PrepareMethod` offsets/signatures;
- `ReadingMode.Deferred` plus rejecting Cecil resolver remains mandatory;
- Step 31 contains no Cecil write or real-StS2 CLR admission/invocation path;
- Gate B records exact per-site IL/control-flow/exception context;
- Gate C can record rewrite-design eligibility only and must still authorize no write;
- version/build/workflow/IPA/TRX identity is `0.0.114 (114)` / `ios-step-31` / `StS2-Launcher-Step-31.ipa` / `step31.trx`.

## Host tests

Run `bash scripts/test.sh`.

The host suite retains all prior regressions and adds synthetic Step-31 coverage proving ordered 4/4 gating, exact ten-site evidence rebinding, per-site semantic-context capture, rewrite-design-only disposition, source byte stability, zero CLR load, and zero Cecil write.

## Codemagic

Run workflow `ios-step-31`.

Authority sequence: canonical static validation → complete host tests → iOS publish → IPA verification → physical iPhone Step 31 A–D. CI is not physical closure.

## Physical Step 31

Force-quit/relaunch and use the existing good Step-12 OfflineReady install. Preserve:

`Documents/StS2Launcher/Reports/Step31-PrepareMethodSemanticContextAudit.txt`

Close condition:

`PREPAREMETHOD SEMANTIC CONTEXT AUDIT PASS — 4/4`

Expected Gate-C disposition if the exact physical evidence still matches:

`BASE-GAME COMPATIBILITY FAMILY CONFIRMED — ELIGIBLE FOR EXPLICIT REWRITE DESIGN; NO WRITE AUTHORIZED`

If any source/method/site fingerprint changes, stop and preserve the failure rather than adapting the evidence in the same candidate.
