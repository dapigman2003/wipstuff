# Testing — Step 34.0 Controlled Transformed Real-StS2 PrewarmJit Execution

Active candidate: Step 34.0 / `0.0.122 (122)`.

## Canonical authority chain

1. `python3 tools/validate_current.py`
2. complete host suite through `scripts/test.sh`
3. iOS publish/package through `scripts/build-ios.sh`
4. `scripts/verify-ipa.sh`
5. physical iPhone Step-34 A–D run from a fresh process

Codemagic workflow: `ios-canonical`

CI cache policy: keep the workflow ID `ios-canonical` stable across future numbered steps so Codemagic can reuse its workflow-scoped cache. The canonical cache preserves the home NuGet cache, the isolated iOS NuGet cache, the validated Godot Step-15 cache, and `src/StS2Launcher.iOS/obj/Release/net9.0-ios/ios-arm64` as the AOT intermediate cache. Cache reuse is an optimization only: all static validation, host tests, publish checks, and IPA verification still run on every build. `artifacts/reports/cache-state.txt` records whether these paths were restored and their sizes before/after publish.

Release identity: IPA `StS2-Launcher-Step-34.ipa`, TRX `step34.trx`, version `0.0.122 (122)`.

## Closed prerequisites

Physical 0.0.120 closed Step 32 at **4/4**, fixing the exact transformed image SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, 9,304,576 bytes, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`, transformed PrewarmJit semantic fingerprint `47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a`, transformed token `0x0600AFEA`, and zero PrepareMethod references. Preserve `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt`.

Physical 0.0.121 closed Step 33 at **4/4**. Only the exact transformed primary entered `StS2Launcher-Step33-TransformedGame`; primary admission caused zero managed resolver requests, zero private dependency loads/requests, and zero native attempts, and no game member was reflected or invoked. Preserve `docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt`.

## Host coverage

Host coverage must prove:

- the four Step-34 gates are ordered and stop after the first failure;
- the strict execution context admits the primary and can service an exact hash-pinned initializer-free private dependency;
- exact `0Harmony 2.4.2.0`, the known initializer-bearing private dependency, remains refused;
- the closed transformed target constants remain pinned, including token `0x0600AFEA`;
- existing Step-32 transform/reopen/isolation and Step-33 transformed-admission regressions remain green;
- the existing Step-23 prepared-runtime preflight remains green.

## Physical Step-34 run

Start from a **fresh app process**. Do not start Godot and do not run an earlier real-game CLR-load boundary first.

Gate A must re-run Step 32 A–D successfully, require the exact closed transformed artifact/target identity/semantic fingerprint, requalify the persisted zero-blocker Step-21/22 runtime plan, re-hash every prepared assembly, and prove the sole initializer-bearing private dependency remains exact `0Harmony 2.4.2.0`. No StS2 CLR admission or game-member invocation is permitted in Gate A.

Gate B must immediately re-hash and `LoadFromStream` only the exact transformed primary into `StS2Launcher-Step34-PrewarmJit`, then verify exact identity, MVID, context ownership, and the Step-33 zero-resolution admission behavior.

Gate C must bind only `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()` from the transformed primary, require static parameterless `void`, transformed token `0x0600AFEA`, and the closed MVID, then invoke it **exactly once**. Exact persisted host-framework bindings and exact hash-pinned initializer-free prepared private dependencies may resolve on demand. Any initializer-bearing dependency request, unplanned managed request, native request, or target exception fails closed and defines the next evidence boundary.

Gate D must re-prove OfflineReady, the receipt-backed original SHA-256, transformed SHA-256, runtime-plan SHA-256, hashes for any admitted private dependencies, unique transformed-context residency, clean resolver/native counters, and exactly one successful PrewarmJit invocation. Game entry-point execution, broader managed startup, Harmony/MonoMod patching, and Godot/game startup remain forbidden.

Preserve `Documents/StS2Launcher/Reports/Step34-TransformedRealStS2PrewarmJitExecution.txt` whether PASS or FAIL. Accept only ordered A–D **4/4 PASS**. If Gate C fails, preserve the exact exception and resolver/native state. Do not retry Step 34 in the same process after Gate B because transformed StS2 and any initializer-free dependencies may remain resident.
