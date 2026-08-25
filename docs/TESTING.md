# Testing — Step 32.0.1 Serialized-Fingerprint Verification Fix

Active candidate: Step 32.0.1 / `0.0.116 (116)`.

## Static/host authority

Canonical entry points remain `scripts/validate.sh` and `scripts/test.sh`. Validation must pin:

- release identity `0.0.116 (116)`, workflow `ios-step-32`, IPA `StS2-Launcher-Step-32.ipa`, TRX `step32.trx`;
- physically closed Step-28/29/30/31 implementation/evidence hashes;
- exact Step-31 source SHA/MVID/PrewarmJit token/body fingerprint and ten offsets;
- Step-32 rewrite contract: six one-argument calls become one `Pop`; four two-argument calls become `Pop + Pop`;
- `ReadingMode.Deferred` plus rejecting Cecil resolver;
- no real-StS2 CLR load/invocation path in Step-32 production code;
- trusted-install path is read-only and transformation output lives beneath `Step32-RealStS2PrepareMethodRewrite/`.

Host regressions use a synthetic `sts2` assembly to prove exact private-copy mutation, source immutability, 10→0 `PrepareMethod` references, +4 instruction count for the four inserted pops, +14 pop delta, refusal when a selected call becomes a branch target, and successful pre-write→reopen semantic-fingerprint verification without an invalid pre-write physical-offset hash prediction.

Authority sequence: canonical static validation → complete host tests → iOS publish → IPA verification → physical iPhone. CI is not physical closure.

## Physical Step 32

Force-quit/relaunch first. Run Step 32 A–D once from a fresh CLR/game state and preserve `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`.

Acceptance:

- Gate A: OfflineReady and exact source evidence pass; source clone is exact; no `sts2` CLR identity is resident.
- Gate B: 6/6 one-argument + 4/4 two-argument sites rewritten; source hash remains unchanged; transformed hash is distinct.
- Gate C: source/transformed `PrepareMethod` references are 10/0; reopened transformed **offset-independent semantic fingerprint** equals the exact in-memory pre-write plan; the reopened physical method-body fingerprint is recorded as post-write evidence and differs from source; assembly identity/MVID and exception-handler count remain preserved.
- Gate D: trusted source/private source/transformed hashes remain stable; OfflineReady re-passes; no real-StS2 CLR load/invocation or runtime detour/native/game startup occurred.

Physical close: **4/4 PASS**.
