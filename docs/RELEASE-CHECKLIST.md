# Release Checklist — Step 32.0.2

## Candidate identity

- step/candidate: **Step 32.0.2**
- version: `0.0.117 (117)`
- workflow: `ios-step-32`
- IPA: `artifacts/StS2-Launcher-Step-32.ipa`
- physical report: `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`

## Required before device install

- [ ] canonical static validation passes;
- [ ] complete host suite passes;
- [ ] Step-32 host regression exercises an external System.Runtime enum constant and the bounded synthetic write resolver;
- [ ] iOS publish succeeds;
- [ ] IPA verification succeeds;
- [ ] release identity is exactly `0.0.117 (117)`;
- [ ] no proprietary StS2 payload is packaged in source artifacts.

## Physical Step-32 acceptance

- [ ] Gate A PASS — exact source/private clone, OfflineReady, no CLR admission;
- [ ] Gate B PASS — exact 6 + 4 rewrite; only exact System.Runtime write-time synthetic metadata resolution; zero external framework/game bytes opened;
- [ ] Gate C PASS — 10 / 0 PrepareMethod references, exact semantic fingerprint, identical Constant-table semantic fingerprint, source unchanged;
- [ ] Gate D PASS — hashes stable, OfflineReady re-proved, trusted install unchanged, no `sts2` CLR load/invocation;
- [ ] overall `REAL STS2 PREPAREMETHOD REWRITE PASS — 4/4`.

If any gate fails, preserve the raw report and stop. Do not broaden resolver search/fallback or advance into transformed-real-StS2 CLR admission.
