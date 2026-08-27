# Testing — Step 32.0.5 Stable Transformed Method Verification

Active candidate: Step 32.0.5 / `0.0.120 (120)`.

## Canonical authority chain

1. `python3 tools/validate_current.py`
2. complete host suite through `scripts/test.sh`
3. iOS publish/package through `scripts/build-ios.sh`
4. `scripts/verify-ipa.sh`
5. physical iPhone Step-32 A–D run from a fresh process

Codemagic workflow: `ios-step-32`

Release identity: IPA `StS2-Launcher-Step-32.ipa`, TRX `step32.trx`, version `0.0.120 (120)`.

## What 0.0.120 changes

The semantic rewrite is unchanged: 6 one-argument PrepareMethod calls become one Pop each, and 4 two-argument calls become Pop + Pop. The exact audited System.Runtime/Sentry Constant-table resolver is also unchanged.

The only production correction is Gate C transformed-image method binding. Source admission and Gate B still require physical Step-31 token `0x06007D05`. After Cecil serialization, Gate C now locates exactly one `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()` by declaring type + full method signature rather than assuming the source MethodDef token is preserved. It then applies the existing semantic fingerprint, PrepareMethod count, instruction/EH shape, Pop delta, Constant-table fingerprint, hash, and zero-resolution/zero-CLR-load checks.

Host coverage must prove:

- the representative source fixture still contains the three audited requirements: System.Runtime/BindingFlags/Int32, Sentry/BreadcrumbLevel/Int32, Sentry/SentryLevel/Int16;
- Gate B can serialize that fixture using in-memory exact-identity surrogates only;
- an additional unaudited non-null external constant requirement is rejected before rewrite/output;
- stable transformed-method identity lookup is independent of a historical source token;
- Gate C independently reopens and proves the full constant-metadata and transformed semantic fingerprints unchanged;
- existing Step-32 source/rewrite/isolation contracts still pass.

## Physical Step-32 state

Physical 0.0.119 is **2/4**. Gate A passed and Gate B successfully serialized the first real-StS2 private 6+4 rewrite with exactly three audited metadata types, nine approved write-time resolver requests across exact System.Runtime/Sentry, zero external dependency-byte reads, no source/trusted mutation, and no CLR load. Gate C then failed at the old token-based transformed-method identity/body check before the deeper semantic/metadata verification ran.

The raw physical report is preserved at `docs/history/reports/STEP-32.0.4-PHYSICAL-GATE-C-TRANSFORMED-METHOD-IDENTITY-FAILURE.txt`.

After Codemagic succeeds for 0.0.120, run a fresh-process physical Step 32 A–D and preserve `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`.

Expected Gate-C report evidence now includes:

- exact stable-identity `PrewarmJit()` reopen;
- transformed MethodDef token;
- whether source token `0x06007D05` survived serialization and the old-token occupant, diagnostic only;
- zero transformed `PrepareMethod` references;
- transformed semantic fingerprint equal to the Gate-B pre-write plan;
- source/transformed Constant-table semantic fingerprint equality;
- expected instruction/EH and Pop-count invariants.

Any resolver request for GodotSharp, System.Collections, another Sentry identity/type, or another assembly remains a failure. Do not add default resolver fallback or search directories in response to a failure.

Codemagic `build-summary.txt` remains the timing record for the optimized active build surface.
