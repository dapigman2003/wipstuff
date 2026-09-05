# Testing — Step 35.0.30 / Step 36.0

Active candidate: `0.0.153 (153)`, IPA `StS2-Launcher-Step-36.ipa`, TRX `step36.trx`, workflow `ios-canonical`.

## CI

1. `bash scripts/validate.sh` must pass.
2. `bash scripts/test.sh` must pass all current host regressions, including Step-36 gate/order/authority constants.
3. Step-15 native-link preflight must pass.
4. iOS publish/link must pass.
5. `scripts/verify-ipa.sh` must verify the exact release identity, runtime policy, fixture payloads, and absence of proprietary game payloads.

## Physical 0.0.153 sequence

Fresh launch:

1. Run Step 15 Gates A-C.
2. Leave the Step-15 smoke engine alive in the same process.
3. Run Step 35 EXACT-CLOSURE once.
4. Require visible Step-35 4/4 finalization and durable `D_WORKER_RETURN`, `D_TASK_AWAIT_RESUMED`, `D_RESULT_RECORD_PASS`, `RUN_EXACT_STEP35_4OF4`.
5. Without force-quitting/backgrounding, press Step 36.0 once.

Expected Step-36 evidence:

- `E_A_PASS` with source token `0x06007D03` and transformed semantic fingerprint equality.
- `E_B_PASS` with exact transformed MethodInfo and stateBefore=1.
- `E_C_INVOKE_START` then either a precise failure boundary or `E_C_INVOKE_RETURNED`.
- On success: `E_C_PASS` with stateAfter=2 and zero initializer-bearing/rejected/native escape.
- Gate D: OfflineReady, exact authority/plan/dependency/context reproof, then `E_D_TASK_AWAIT_RESUMED` and `RUN_STEP36_4OF4`.

If Step-36 Gate C begins, do not retry in the same process. Preserve Step36 checkpoint/static-map/report artifacts first.
