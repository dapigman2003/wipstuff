# Testing — Step 30 Selected Harmony Target Semantic Context Audit

## Static validation

Run `bash scripts/validate.sh`.

The active candidate is Step 30.0 / `0.0.113 (113)`. Validation must preserve the physically closed Step-28 and Step-29 evidence while pinning the new read-only selected-method semantic audit:

- Step 29 raw physical report remains present and records **4/4 PASS**, source SHA-256/MVID, token `0x06007927`, `IL_0D9D -> Harmony.PatchAll(Assembly)`, body SHA-256, zero writes/CLR load, and OfflineReady 428/428;
- Step 30 production source hard-pins those physical values;
- `ReadingMode.Deferred` plus rejecting Cecil resolver remains mandatory;
- Step 30 contains no Cecil write or real-StS2 CLR admission/invocation path;
- Gate B records bounded IL/control-flow/exception context;
- Gate C can only defer the exact mod-loading Harmony site and must authorize no rewrite;
- version/build/workflow/IPA/TRX identity is `0.0.113 (113)` / `ios-step-30` / `StS2-Launcher-Step-30.ipa` / `step30.trx`.

## Host tests

Run `bash scripts/test.sh`.

The host suite retains all prior regressions and adds synthetic Step-30 coverage proving ordered 4/4 gating, exact fingerprint rebinding, semantic-context capture, mod-path disposition, source byte stability, zero CLR load, and zero Cecil write.

## Codemagic

Run workflow `ios-step-30`.

Authority sequence: canonical static validation → complete host tests → iOS publish → IPA verification → physical iPhone Step 30 A–D. CI is not physical closure.

## Physical Step 30

Force-quit/relaunch and use the existing good Step-12 OfflineReady install. Preserve:

`Documents/StS2Launcher/Reports/Step30-SelectedTargetSemanticContextAudit.txt`

Close condition:

`SELECTED TARGET SEMANTIC CONTEXT AUDIT PASS — 4/4`

Expected Gate-C disposition if the exact Step-29 evidence still matches:

`DEFER — MOD/HARMONY COMPATIBILITY PATH; NO BASE-GAME REWRITE AUTHORIZED`

If any fingerprint/context invariant changes, stop and preserve the failure rather than adapting the target in the same candidate.
