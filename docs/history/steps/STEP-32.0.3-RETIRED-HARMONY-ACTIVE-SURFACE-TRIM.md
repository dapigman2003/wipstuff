# Step 32.0.3 — Retired Harmony Active-Surface Trim

Version: `0.0.118 (118)`

## Trigger

Physical 0.0.117 re-proved Step-32 Gate A against the exact receipt-backed real `sts2.dll`, then Gate B failed closed before any rewrite because the module-wide constant-metadata inventory found an additional external scope:

`Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0`

That result is preserved in `docs/history/reports/STEP-32.0.2-PHYSICAL-SENTRY-CONSTANT-METADATA-FAILURE.txt`. It does not disprove the Step-32 6+4 rewrite; the failure occurred in pre-write resolver configuration before mutation or `ModuleDefinition.Write`.

At the same time, Codemagic build/test latency had accumulated from historical experiments that were still compiled, tested, downloaded, and bundled even though their architecture was already closed.

## Purpose

Step 32.0.3 is a **maintenance-only** candidate. It does not attempt to correct the Sentry finding and does not advance the Step-32 physical boundary.

Its purpose is to reduce the active compile/test/AOT/package surface while preserving the closed experiments as inert historical evidence.

## Active-surface changes

The following Step-25/26/27 runtime-Harmony experiment surface is retired from the active tree:

- controlled Harmony constructor, processor-creation, and patch-execution engines and their gate/result/progress types;
- Harmony patch/processor probe helpers;
- dedicated Step-25/26/27 host tests;
- Step-25/27 iOS framework-preservation anchors;
- Step-25/26/27 launcher UI controls;
- the Step-27 post-publish interpreted-patch fixture;
- the recurring Harmony-Fat 2.4.2 host-test download;
- Step-27 fixture build, bundle-copy, and IPA verification work;
- obsolete candidate/protected hash manifests whose only purpose was to keep those retired implementations active.
- pre-populated generated `artifacts/` reports from the source package; canonical scripts recreate fresh reports, preventing stale CI evidence from being mistaken for a current run.

The complete pre-trim `0.0.117` source archive is preserved inside the inert historical archive so the retired implementation remains reconstructable and auditable.

## What remains active

This maintenance step deliberately retains:

- Step-12 receipt-backed install integrity and OfflineReady foundations;
- Step-20 dynamic managed execution foundations still relevant to future transformed-game execution;
- Step-21/22 prepared dependency/runtime binding machinery;
- Step-23 real-StS2 admission foundations;
- Step-24 controlled initialization, because it remains a live managed-runtime regression boundary;
- Step-28 ahead-of-load transformation architecture and its fixture/regression coverage;
- Step-29/30/31 read-only real-game audit evidence/contracts;
- the complete Step-32 implementation and tests;
- `MtouchInterpreter=-all`, `MtouchLink=None`, and `TrimMode=copy`;
- the existing Godot host/build architecture.

The Step-28 fixture build is also made conditional in the iOS build stage so a fixture already produced by the canonical host-test stage is not rebuilt unnecessarily.
The canonical Codemagic wrapper records elapsed seconds for SDK setup, static validation, host tests/fixture builds, iOS workload setup, iOS publish/package preparation, IPA verification, and total pipeline duration so the maintenance effect is measurable.

## Step-32 behavior intentionally unchanged

`RealStS2PrepareMethodRewrite.cs` and the Step-32 compatibility test implementation are not modified for this maintenance candidate. In particular, 0.0.118 still contains the 0.0.117 exact-`System.Runtime` write-only constant-metadata surrogate policy and therefore is expected to encounter the known Sentry scope on a physical Step-32 run.

No Sentry resolver authority is added here.

## Acceptance

The authority for this maintenance candidate is CI/build evidence rather than a new physical Step-32 result:

1. canonical static validation passes;
2. the remaining host suite compiles and passes;
3. iOS publish succeeds under the unchanged runtime policy;
4. IPA verification passes without retired Step-27 payloads;
5. Codemagic timing/artifact size can be compared with the previous active surface.

A physical run of 0.0.118 is optional and is not expected to close Step 32. Step 32 itself remains open until a later candidate passes Gates A–D 4/4 under the predeclared real-game rewrite contract.
