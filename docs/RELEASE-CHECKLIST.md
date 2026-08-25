# Release Checklist — Step 31.0

## Source / policy

- Step 28 is physically closed positive 5/5; Step 29 and Step 30 are physically closed positive 4/4.
- Step 27 remains closed negative; do not revive runtime Harmony/MonoMod replacement.
- `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain active host policy.
- Trusted Step-12 install remains immutable.
- Step 31 is read-only: zero real-StS2 Cecil writes and zero CLR load/invocation.
- Gate A hard-binds exact source SHA/MVID plus `PrewarmJit()` token/body fingerprint and ten exact PrepareMethod sites.
- Gate B uses Deferred Cecil + rejecting resolver and records per-site semantic context.
- Gate C may record rewrite-design eligibility but must still say **NO WRITE AUTHORIZED** and make no runtime-reachability claim.
- Gate D re-hashes source and re-proves OfflineReady/isolation.

## Build identity

- step/candidate: **Step 31.0**
- version: `0.0.114 (114)`
- workflow: `ios-step-31`
- IPA: `artifacts/StS2-Launcher-Step-31.ipa`
- TRX: `artifacts/test-results/step31.trx`
- top banner: **STEP 31.0 — PREPAREMETHOD SEMANTIC CONTEXT AUDIT**

## Pre-device authority

Canonical static validation, complete host suite, iOS publish, and IPA verification must all pass. Preserve any first failure and make only the smallest correction.

## Device-run discipline

- force-quit/relaunch before Step 31;
- preserve `Step31-PrepareMethodSemanticContextAudit.txt`;
- do not transform any PrepareMethod site in this build;
- do not run Godot/game startup or native game loading as part of Step 31.

Physical iPhone remains final authority. Step 31 closes only at **A–D / 4/4 PASS**.
