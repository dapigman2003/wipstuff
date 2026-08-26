# Step 32.0.3 — Test Procedure

Version: `0.0.118 (118)`

## Codemagic — stop at the first failure

1. Run workflow **`step32-fast`**.
2. Require canonical static validation PASS and the complete host regression suite PASS. Record the commit SHA from `fast-preflight-summary.txt`.
3. If fast preflight fails, stop and preserve its artifacts. Do **not** run the device workflow.
4. If it passes, run **`ios-step-32` on the exact same commit**.
5. Require static validation, iOS workload/install, publish and IPA verification PASS. Compare the device build commit SHA with the fast-preflight SHA.
6. If device CI fails, stop and preserve artifacts. Do not install an IPA.
7. Only after both workflows pass on the same commit, install the produced IPA.

Use `artifacts/reports/phase-timings.txt` and `cache-sizes.txt` to measure where free M2 minutes are spent.

## Physical Step 32 A–D

Force-quit first. Preserve `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`.

Expected Gate B evidence now includes:

- 6/6 one-argument and 4/4 two-argument sites;
- 10 × exactly five-byte patch windows;
- raw opcode/token binding at each selected site;
- exact source/transformed file length equality;
- every changed byte confined to the ten approved windows;
- Cecil serialization: **NO**;
- Cecil dependency-resolution requests during rewrite planning: **0**.

Expected Gate C includes source/transformed PrepareMethod references 10/0, exact padded Pop/Nop shape at the original offsets, semantic-fingerprint match, unchanged Constant-table semantics, and repeated byte-diff confinement.

Pass condition remains `REAL STS2 PREPAREMETHOD REWRITE PASS — 4/4`.
