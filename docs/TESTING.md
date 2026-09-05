# Testing — Step 35.0.31 / Step 36.0.2

Active candidate: `0.0.156 (156)`, IPA `StS2-Launcher-Step-36.ipa`, TRX `step36.trx`, workflow `ios-canonical`.

## CI

1. `bash scripts/validate.sh` must pass.
2. `bash scripts/test.sh` must pass all current host regressions, including Step-36 gate/order/authority constants.
3. Step-15 native-link preflight must pass.
4. iOS publish/link must pass.
5. `scripts/verify-ipa.sh` must verify the exact release identity, runtime policy, fixture payloads, and absence of proprietary game payloads.

## Physical 0.0.156 sequence

Fresh launch:

1. Run Step 15 Gates A-C.
2. Leave the Step-15 smoke engine alive in the same process.
3. Run Step 35 EXACT-CLOSURE once.
4. Require durable `D_WORKER_RETURN`, then `D_THREADPOOL_CONTINUATION`, `D_UI_DISPATCH_ENTER`, `D_RESULT_RECORD_PASS`, `D_UI_DISPATCH_RETURN`, `RUN_EXACT_STEP35_4OF4`, and normal `RUN_END`.
5. Without force-quitting/backgrounding, press Step 36.0.2 once.

Expected Step-36 evidence:

- `E_A_PASS` with source token `0x06007D03` and transformed semantic fingerprint equality.
- `E_B_PACK_RECEIPT_PASS` for the exact receipt-backed `Slay the Spire 2.pck`.
- `E_B_PACK_BIND_PASS` and `E_B_PACK_LOAD_RETURNED — returned=True` with `replaceFiles=false`.
- `E_B_LOCALIZATION_DIR_PROBE_RETURNED — exists=True` for `res://localization/eng`, then `E_B_GAME_RESOURCE_PACK_PASS`.
- `E_B_PASS` with exact transformed MethodInfo, stateBefore=1, and unchanged resolver baseline.
- `E_C_INVOKE_START` then either a precise failure boundary or `E_C_INVOKE_RETURNED`.
- On success: `E_C_PASS` with stateAfter=2 and zero initializer-bearing/rejected/native escape.
- Gate D: OfflineReady, exact authority/plan/dependency/context reproof, then `E_D_THREADPOOL_CONTINUATION`, `E_D_UI_DISPATCH_ENTER`, `E_D_RESULT_RECORD_PASS`, and `RUN_STEP36_4OF4`.

If Step-36 Gate C begins, do not retry in the same process. Preserve Step36 checkpoint/static-map/report artifacts first.


### Step 36.0.2 failure evidence
On Gate-C failure preserve the run-correlated journal and final report. The report must expose the complete exception depths, `GetBaseException()`, any `ReflectionTypeLoadException.LoaderExceptions`, state after failure, resolver/load deltas, and sts2/GodotSharp load-context continuity. Do not retry Step 36 in the same process.
