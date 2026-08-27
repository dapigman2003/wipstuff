# Step 32.0.2 — Physical iPhone Test

Version: `0.0.117 (117)`

## Before device testing

1. Run Codemagic workflow `ios-step-32`.
2. Require canonical static validation PASS and the complete host suite PASS.
3. Require iOS publish and `scripts/verify-ipa.sh` PASS for `artifacts/StS2-Launcher-Step-32.ipa`.
4. Install the produced IPA.
5. Force-quit the launcher before the Step-32 run.
6. Ensure the legitimate Step-12 installation is OfflineReady.

## Run

Run Step 32 A–D once from a fresh process and preserve `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`.

Expected gates:

- **Gate A — SourceAdmissionAndPrivateClone:** exact receipt-backed ARM64 `sts2.dll`, source SHA/MVID/token/body fingerprint and all 10 sites rebound; OfflineReady 428/428; private clone exact; zero CLR admission; zero Cecil dependency resolution.
- **Gate B — DeterministicStackNeutralRewrite:** 6 one-argument and 4 two-argument PrepareMethod calls rewritten exactly; bounded write-only constant-metadata resolver may satisfy only exact `System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a`; zero external framework/game assembly bytes opened; transformed private image written.
- **Gate C — TransformedImageVerification:** source/transformed PrepareMethod references 10/0; exact Pop/instruction/EH topology; transformed semantic fingerprint matches the pre-write plan; source/transformed Constant-table semantic fingerprints are identical; read/verification resolvers remain rejecting with zero requests.
- **Gate D — FinalIsolationAudit:** source/private-source/transformed hashes stable; OfflineReady re-proved; trusted install unchanged; no `sts2` CLR load/invocation; no Harmony/MonoMod runtime patching; no Godot/game/native startup.

## Pass condition

`REAL STS2 PREPAREMETHOD REWRITE PASS — 4/4`

A pass closes Step 32's first real-game private rewrite boundary. It does **not** itself prove transformed-real-StS2 CLR admission/execution or game startup.

## Failure discipline

Stop at the first failing gate. Preserve the full report and raw CI artifacts. Do not broaden the resolver, add search directories, use Cecil's default resolver, or alter the 6+4 rewrite unless the new evidence specifically requires it.
