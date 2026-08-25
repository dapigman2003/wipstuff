# Release Checklist — Step 32.0

## Candidate identity

- step/candidate: **Step 32.0**
- version: `0.0.115 (115)`
- workflow: `ios-step-32`
- IPA: `artifacts/StS2-Launcher-Step-32.ipa`
- TRX: `artifacts/test-results/step32.trx`
- top banner: **STEP 32.0 — FIRST REAL STS2 PREPAREMETHOD REWRITE**

## Required invariants

- receipt-backed Step-12 install is never written;
- exact physical Step-31 source/method/site evidence remains pinned;
- private transformed image changes only the ten exact `PrepareMethod` sites using the predeclared stack-neutral replacement;
- no arbitrary Cecil resolver fallback;
- no real-StS2 CLR admission/invocation in Step 32;
- no Harmony/MonoMod runtime patching;
- no Godot/game startup or native game loading;
- `MASTER-PLAN.md` is not changed for this routine execution of the already-selected transform-before-load architecture.

## Before device run

- Codemagic static validation PASS;
- complete host suite PASS;
- iOS publish PASS;
- IPA verification PASS;
- force-quit/relaunch before Step 32.

Physical iPhone remains final authority. Step 32 closes only at **A–D / 4/4 PASS** with the dedicated report preserved.
