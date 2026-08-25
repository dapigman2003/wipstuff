# Release Checklist — Step 30.0

## Source / policy

- Step 28 is physically closed positive 5/5; Step 29 is physically closed positive 4/4.
- Step 27 remains closed negative; do not revive runtime Harmony/MonoMod replacement.
- `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain active host policy.
- Trusted Step-12 install remains immutable.
- Step 30 is read-only: zero real-StS2 Cecil writes and zero CLR load/invocation.
- Gate A hard-binds exact Step-29 physical source SHA/MVID/token/IL/target/body fingerprint.
- Gate B uses Deferred Cecil + rejecting resolver and records bounded semantic context.
- Gate C must not authorize a rewrite; if the site remains `ModManager.TryLoadMod -> Harmony.PatchAll`, disposition is DEFER from base-game frontier.
- Gate D re-hashes source and re-proves OfflineReady/isolation.

## Build identity

- step/candidate: **Step 30.0**
- version: `0.0.113 (113)`
- workflow: `ios-step-30`
- IPA: `artifacts/StS2-Launcher-Step-30.ipa`
- TRX: `artifacts/test-results/step30.trx`
- top banner: **STEP 30.0 — SELECTED HARMONY TARGET SEMANTIC CONTEXT AUDIT**

## Pre-device authority

Canonical static validation, complete host suite, iOS publish, and IPA verification must all pass. Preserve any first failure and make only the smallest correction.

## Device-run discipline

- force-quit/relaunch before Step 30;
- preserve `Step30-SelectedTargetSemanticContextAudit.txt`;
- do not transform the selected Harmony site in this build;
- do not run Godot/game startup or native game loading as part of Step 30.

Physical iPhone remains final authority. Step 30 closes only at **A–D / 4/4 PASS**.
