# Testing — Step 32.0.3 Exact-Length Private IL Patch

Active candidate: Step 32.0.3 / `0.0.118 (118)`.

## Authority sequence optimized for Codemagic free M2 minutes

1. **`step32-fast`** — pinned .NET SDK, `scripts/validate.sh`, then the **complete** `scripts/test.sh` host regression suite. No iOS workload, Godot publish, or IPA.
2. If fast fails, stop. Preserve artifacts; do not run device CI.
3. **`ios-step-32` on the exact same commit** — repeats static validation only, installs the pinned iOS workload with `--skip-manifest-update`, builds/publishes, and runs `scripts/verify-ipa.sh`. It intentionally does not repeat the full host suite already proven for that commit.
4. If device CI fails, stop. Do not install an IPA.
5. Only when both summaries show PASS for the same commit, install the IPA and run physical Step 32 A–D.

Both workflows produce `artifacts/reports/phase-timings.txt` and `cache-sizes.txt`. Cached inputs include `$HOME/.nuget/packages`, the pinned Harmony-Fat regression archive, a pristine pinned .NET SDK snapshot, and the existing Godot archive on the device workflow.

## Step 32.0.3 regression focus

Production Gate B must contain **no `ModuleDefinition.Write` path**. It must:

- use deferred Cecil + rejecting resolver only to bind the exact physical source/method/sites;
- map `PrewarmJit()` RVA to PE file offset and parse tiny/fat IL headers fail-closed;
- require each selected site to be a direct five-byte `call` (`0x28`) whose raw 4-byte metadata token equals Cecil's bound target token;
- use only `26 00 00 00 00` for one-argument sites and `26 26 00 00 00` for two-argument sites;
- preserve exact file length;
- prove every changed byte is inside the ten non-overlapping approved five-byte windows;
- perform zero dependency resolution while planning/writing.

The host fixture intentionally contains an unrelated **Sentry-scoped external enum constant**, reproducing the class of whole-module metadata dependency that stopped physical 0.0.117. The Step-32 rewrite must succeed without resolving or serializing that metadata.

Gate C must reopen under the rejecting resolver and verify source/transformed PrepareMethod references **10 / 0**, instruction delta **+40**, Pop/Nop delta **+14 / +36**, exact replacement instructions at original offsets, semantic-fingerprint match, unchanged Constant-table fingerprint, EH count, identity/MVID, and repeated raw byte-diff confinement.

Physical acceptance remains A–D **4/4 PASS**.
