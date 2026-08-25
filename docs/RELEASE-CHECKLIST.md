# Release Checklist — Step 29.0

## Source / policy

- Step 28.0.2 / 0.0.111 is physically closed positive at **5/5**; preserve its raw report and closure note.
- Step 27 remains closed negative; do not revive runtime Harmony/MonoMod replacement.
- `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain active host policy.
- The receipt-backed Step-12 managed install remains immutable.
- Step 29 is read-only: zero Cecil writes and zero real-StS2 CLR load/invocation.
- Cecil source admission/audit uses `ReadingMode.Deferred` plus a rejecting assembly/metadata resolver; zero resolver requests are required.
- Gate B records exact source method token, IL offset/opcode, target scope/member and method-body SHA-256.
- Gate C selects at most one audit candidate under a statically pinned priority policy; selection is evidence only.
- Gate D re-hashes source and re-proves OfflineReady.
- Godot/game startup and native game loading remain separately gated.

## Build identity

- step/candidate: **Step 29.0**
- version: `0.0.112 (112)`
- workflow: `ios-step-29`
- IPA: `artifacts/StS2-Launcher-Step-29.ipa`
- TRX: `artifacts/test-results/step29.trx`
- top banner: **STEP 29.0 — REAL STS2 COMPATIBILITY TARGET AUDIT**

## Pre-device authority

- canonical static validation: PASS;
- complete host suite: PASS;
- iOS publish: PASS;
- IPA verification: PASS.

If any stage fails, preserve the raw artifact, classify the first failing boundary, make the smallest correction, bump candidate identity, and do not broaden the Step-29 audit scope unless evidence requires it.

## Device-run discipline

- force-quit/relaunch before Step 29;
- Gate A must report no CLR-resident `sts2` and zero Cecil resolver requests;
- preserve `Step29-RealStS2CompatibilityTargetAudit.txt`;
- if Gate C selects a candidate, do not transform it in the same build;
- if Gate C reports no direct primary target, preserve that outcome instead of choosing a broad fallback manually.

## Authority

Physical iPhone remains final authority. Step 29 closes only at **A–D / 4/4 PASS**. The resulting exact candidate report is the authority for designing the next semantic transformation candidate.
