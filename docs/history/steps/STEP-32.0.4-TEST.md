# Step 32.0.4 — Codemagic + Physical iPhone Test

Version: `0.0.119 (119)`

## Codemagic prerequisite

Run workflow `ios-step-32` and require:

1. canonical static validation PASS;
2. complete active host suite PASS, including the representative three-requirement synthetic Constant-table fixture and fail-closed unaudited-requirement test;
3. iOS publish/package PASS under `MtouchInterpreter=-all`, `MtouchLink=None`, `TrimMode=copy`;
4. `scripts/verify-ipa.sh` PASS for `artifacts/StS2-Launcher-Step-32.ipa`;
5. release identity exactly `0.0.119 (119)`.

Do not physically test a Codemagic failure.

## Physical run

Install the verified IPA, force-quit the launcher, ensure the legitimate Step-12 managed install is OfflineReady, then run Step 32 A–D once from a fresh process. Preserve:

`Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`

Expected gates:

- **Gate A — SourceAdmissionAndPrivateClone:** exact receipt-backed ARM64 `sts2.dll`; Step-31 hash/MVID/token/body/site evidence; OfflineReady 428/428; private clone exact; zero CLR admission; zero Cecil read-time resolution.
- **Gate B — DeterministicStackNeutralRewrite:** the pre-write external non-null Constant requirement set equals exactly 3 audited type/storage requirements across exact System.Runtime 9.0.0.0 and Sentry 5.0.0.0; three synthetic enum definitions are created in per-exact-assembly in-memory surrogates; only approved exact write-time resolution identities are accepted; 6 one-argument + 4 two-argument PrepareMethod calls are rewritten exactly; transformed private image is written; zero external framework/game assembly bytes are opened by the write resolver.
- **Gate C — TransformedImageVerification:** reopened source/transformed PrepareMethod references 10/0; exact Pop/instruction/EH topology; transformed semantic fingerprint matches the pre-write plan; full source/transformed Constant-table semantic fingerprints are identical; read/verification resolvers remain rejecting.
- **Gate D — FinalIsolationAudit:** source/private-source/transformed hashes stable; OfflineReady re-proved; trusted install unchanged; no real `sts2` CLR load/invocation; no Harmony/MonoMod runtime patching; no Godot/game/native startup.

## Pass condition

`REAL STS2 PREPAREMETHOD REWRITE PASS — 4/4`

A 4/4 pass closes Step 32's first real-game private rewrite boundary only. It does not itself authorize transformed-real-StS2 CLR admission/execution.

## Failure discipline

Stop at the first failed gate and preserve the complete report. In particular:

- an unaudited external Constant requirement is evidence, not permission to broaden the set;
- a request for GodotSharp, System.Collections, another Sentry identity/type, or any other assembly must remain fail-closed;
- do not enable Cecil default resolution or search directories;
- do not change the 6+4 rewrite unless new evidence specifically disproves it.
