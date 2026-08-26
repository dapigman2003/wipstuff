# Release Checklist — Step 32.0.3

## Candidate identity

- step/candidate: **Step 32.0.3**
- version: `0.0.118 (118)`
- fast workflow: `step32-fast`
- device workflow: `ios-step-32`
- IPA: `artifacts/StS2-Launcher-Step-32.ipa`
- physical report: `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`

## Required before device install

- [ ] run `step32-fast` and require canonical static validation + complete host suite PASS;
- [ ] record its exact `CM_COMMIT` from `fast-preflight-summary.txt`;
- [ ] only then run `ios-step-32` on the exact same commit;
- [ ] require device-workflow static validation, iOS publish, and IPA verification PASS;
- [ ] confirm both workflow summaries show the same commit;
- [ ] release identity is exactly `0.0.118 (118)`;
- [ ] no proprietary StS2 payload is packaged in source artifacts.

## Physical Step-32 acceptance

- [ ] Gate A PASS — exact source/private clone, OfflineReady, no CLR admission;
- [ ] Gate B PASS — exact 6 + 4 rewrite in ten five-byte windows; raw opcode+metadata-token binding; zero differences outside approved windows; no Cecil serialization/resolution;
- [ ] Gate C PASS — PrepareMethod 10/0; exact `Pop/Nop` replacement shape at original offsets; semantic fingerprint, Constant-table semantics, identity/MVID/EH and byte-diff proof all pass;
- [ ] Gate D PASS — hashes stable, OfflineReady re-proved, trusted install unchanged, no `sts2` CLR load/invocation;
- [ ] overall `REAL STS2 PREPAREMETHOD REWRITE PASS — 4/4`.

If either CI workflow or any physical gate fails, preserve artifacts and stop. Do not rerun later stages.
