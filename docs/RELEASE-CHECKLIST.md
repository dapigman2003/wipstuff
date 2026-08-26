# Release Checklist — Step 32.0.3 Maintenance Trim

## Candidate identity

- step/candidate: **Step 32.0.3**
- version: `0.0.118 (118)`
- workflow: `ios-step-32`
- IPA: `artifacts/StS2-Launcher-Step-32.ipa`
- known physical Step-32 blocker: exact external constant-metadata scope `Sentry 5.0.0.0`

## Required for the maintenance candidate

- [ ] canonical static validation passes;
- [ ] complete **active** host suite passes;
- [ ] no Harmony-Fat network acquisition occurs;
- [ ] retired Step-25–27 runtime-Harmony Core/UI/tests/preservation anchors are absent from active compilation;
- [ ] Step-27 interpreted fixture is not built or bundled;
- [ ] Step-28 fixture remains hash-verified and post-publish only;
- [ ] iOS publish succeeds;
- [ ] IPA verification succeeds;
- [ ] release identity is exactly `0.0.118 (118)`;
- [ ] no proprietary StS2 payload is packaged in source artifacts;
- [ ] `RealStS2PrepareMethodRewrite.cs` remains identical to 0.0.117;
- [ ] record Codemagic build duration/logs for comparison with the pre-trim pipeline.

## Step-32 physical acceptance remains unchanged

A future feature candidate still requires:

- [ ] Gate A PASS — exact source/private clone, OfflineReady, no CLR admission;
- [ ] Gate B PASS — exact 6 + 4 rewrite with only explicitly justified bounded constant-metadata handling;
- [ ] Gate C PASS — 10 / 0 PrepareMethod references, exact semantic fingerprint, identical Constant-table semantics, source unchanged;
- [ ] Gate D PASS — hashes stable, OfflineReady re-proved, trusted install unchanged, no `sts2` CLR load/invocation;
- [ ] overall `REAL STS2 PREPAREMETHOD REWRITE PASS — 4/4`.

0.0.118 is not a Sentry correction. Do not broaden resolver search/fallback merely to make the physical run advance.

Codemagic `build-summary.txt` records per-stage elapsed seconds for SDK setup, static validation, host tests/fixture builds, iOS workload setup, iOS publish/package preparation, IPA verification, and the total canonical pipeline so the maintenance trim can be measured rather than inferred.
