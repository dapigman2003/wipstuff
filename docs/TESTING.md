# Testing — Step 33.0 Verified Transformed Real-StS2 CLR Admission

Active candidate: Step 33.0 / `0.0.121 (121)`.

## Canonical authority chain

1. `python3 tools/validate_current.py`
2. complete host suite through `scripts/test.sh`
3. iOS publish/package through `scripts/build-ios.sh`
4. `scripts/verify-ipa.sh`
5. physical iPhone Step-33 A–D run from a fresh process

Codemagic workflow: `ios-canonical`

CI cache policy: keep the workflow ID `ios-canonical` stable across future numbered steps so Codemagic can reuse its workflow-scoped cache. The canonical cache preserves the home NuGet cache, the isolated iOS NuGet cache, the validated Godot Step-15 cache, and `src/StS2Launcher.iOS/obj/Release/net9.0-ios/ios-arm64` as the AOT intermediate cache. Cache reuse is an optimization only: all static validation, host tests, publish checks, and IPA verification still run on every build. `artifacts/reports/cache-state.txt` records whether these paths were restored and their sizes before/after publish.


Release identity: IPA `StS2-Launcher-Step-33.ipa`, TRX `step33.trx`, version `0.0.121 (121)`.

## Closed prerequisite

Physical 0.0.120 closed Step 32 at **4/4**. Preserve `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt`. Step 33 must require the exact closed transformed image SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, 9,304,576 bytes, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`, transformed PrewarmJit semantic fingerprint `47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a`, and zero PrepareMethod references before CLR admission.

## Host coverage

Host coverage must prove:

- the four Step-33 gates are ordered and stop after the first failure;
- the admission-only load context can `LoadFromStream` a primary into its dedicated context;
- a private prepared dependency request is refused and does not enter the Step-33 context;
- existing Step-32 transform/reopen/isolation regressions remain green;
- the existing Step-23 prepared-runtime preflight remains green.

## Physical Step-33 run

Start from a fresh app process. Do not start Godot and do not run any earlier real-game CLR-load boundary first.

Gate A must re-run Step 32 A–D successfully, require the exact closed transformed artifact, and requalify the persisted zero-blocker Step-21/22 runtime plan without CLR-loading StS2.

Gate B must immediately re-hash and `LoadFromStream` only the exact transformed `sts2.dll` into `StS2Launcher-Step33-TransformedGame`, then verify exact assembly identity, MVID, context ownership, and unique `sts2` residency. No game member invocation is permitted.

Gate C must require the Step-33 private context to contain transformed `sts2` only. Private dependency requests are a fail-closed boundary. Unplanned managed requests and all native requests are failures. Exact planned host-framework bindings may be serviced from `AssemblyLoadContext.Default` only if requested by the CLR during primary admission.

Gate D must re-prove OfflineReady, the receipt-backed original SHA-256, transformed SHA-256, runtime-plan SHA-256, unique transformed-context residency, and zero private/native/game-execution expansion.

Preserve `Documents/StS2Launcher/Reports/Step33-TransformedRealStS2AssemblyAdmission.txt` whether PASS or FAIL. A valid closure is `TRANSFORMED REAL STS2 CLR ADMISSION PASS — 4/4`.
