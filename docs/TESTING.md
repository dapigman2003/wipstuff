# Testing — Step 32.0.4 Audited Constant-Metadata Resolver

Active candidate: Step 32.0.4 / `0.0.119 (119)`.

## Canonical authority chain

1. `python3 tools/validate_current.py`
2. complete host suite through `scripts/test.sh`
3. iOS publish/package through `scripts/build-ios.sh`
4. `scripts/verify-ipa.sh`
5. physical iPhone Step-32 A–D run from a fresh process

Codemagic workflow: `ios-step-32`

Release identity: IPA `StS2-Launcher-Step-32.ipa`, TRX `step32.trx`, version `0.0.119 (119)`.

## What 0.0.119 changes

The semantic rewrite is unchanged: 6 one-argument PrepareMethod calls become one Pop each, and 4 two-argument calls become Pop + Pop. The only changed runtime boundary is the write-time Constant-table resolver.

Host coverage must prove:

- the representative source fixture contains the three audited requirements: System.Runtime/BindingFlags/Int32, Sentry/BreadcrumbLevel/Int32, Sentry/SentryLevel/Int16;
- Gate B can serialize that fixture using in-memory exact-identity surrogates only;
- an additional unaudited non-null external constant requirement is rejected before rewrite/output;
- Gate C independently reopens and proves the full constant-metadata semantic fingerprint unchanged;
- existing Step-32 source/rewrite/isolation contracts still pass.

## Physical Step-32 state

Latest physical evidence remains 0.0.117: Gate A PASS, Gate B fail-closed before mutation on exact Sentry 5.0.0.0. The static audit then proved the exact three non-null requirement set. User-confirmed Codemagic 0.0.118 established the lean build baseline.

The first Codemagic attempt passed 669/669 static checks and compiled, but the three Step-32 rewrite tests failed during synthetic exact-System.Runtime fixture construction (`TypeSystem.Int32` on an image-less core-library module). The fixture-only correction imports primitive storage references and does not change production Step-32 code or release identity. Rerun the full canonical pipeline; do not use the failed attempt as publish/device authority.

0.0.119 is intended to cross that known metadata-serialization boundary without broad resolution. After Codemagic succeeds, run a fresh-process physical Step 32 A–D and preserve `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`.

Expected Gate-B report evidence includes:

- `Synthetic constant-metadata resolver types: 3`;
- `Audited external constant type/storage requirements approved: 3/3 across 2/2 exact assembly scopes`;
- all Cecil write-time resolution requests limited to the configured exact audited System.Runtime/Sentry identities;
- zero external framework/game assembly bytes opened by the write resolver;
- exact 6/6 + 4/4 rewrite counts.

Any request for GodotSharp, System.Collections, another Sentry identity/type, or another assembly remains a failure. Do not add default resolver fallback or search directories in response to a failure.

Codemagic `build-summary.txt` remains the timing record for the optimized active build surface.
