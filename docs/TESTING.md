# Testing — Step 35.0.32 / Step 36.0.3

Active candidate: `0.0.157 (157)`, IPA `StS2-Launcher-Step-36.ipa`, TRX `step36.trx`, workflow `ios-canonical`.

## CI

Run the canonical `ios-canonical` workflow. It must pass current static validation, host tests, native-link preflight, iOS Release publish, IPA inspection, and artifact verification. Keep the workflow key stable so Codemagic can reuse NuGet, `.dotnet`, Step-15 Godot source/build, and Release iOS `obj` caches.

## Device sequence

1. Fresh launch.
2. Run Step 15 Gates A-C.
3. Run Step 35.0.32 **MODEL-BOOTSTRAP** exactly once; require 4/4 and `RUN_MODEL_BOOTSTRAP_4OF4`.
4. Without force-quitting/backgrounding, run Step 36.0.3 exactly once.
5. If Step 36 Gate C starts and fails/stalls, force-quit before another attempt.

## Highest-value evidence

Step35 static/preflight output must include `STEP 36.0.3 MODELDB BOOTSTRAP COMPATIBILITY PLAN`, canonical model count, dependency edge count, backward-edge count, dependency-closure preinject count, the `BowlbugsNormal -> BowlbugEgg` edge, preserved final order, and the compatibility SHA-256.

Step36 must retain `E_C_EXCEPTION_CAPTURED`, every `E_C_EXCEPTION_DEPTH`, `E_C_BASE_EXCEPTION`, `E_C_POST_FAILURE_CONTEXT`, state-after-failure, resolver/load deltas, and sts2/GodotSharp context continuity. On success require normal Gate C return, state `2`, no rejected/initializer-bearing/native requests, and Gate D 4/4.
