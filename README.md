# StS2 Launcher iOS — Step 32.0.4 Fast-Preflight Assertion Fix

Steps 01–26 are physically closed. Step 27 is closed negative for runtime Harmony/MonoMod replacement. Step 28 is closed positive at **5/5** for ahead-of-load transformation + transformed-only interpreted execution. Steps 29–31 are each closed positive at **4/4**, ending with the exact `OneTimeInitialization::PrewarmJit()` / ten-`RuntimeHelpers.PrepareMethod` family authorized for explicit rewrite design.

## Active candidate

**Step 32.0.4 / `0.0.119 (119)` — test-only correction after fast preflight validated the exact-length rewrite path**

The semantic change remains exactly the Step-31-approved family: six one-argument `PrepareMethod(handle)` calls consume one stack value; four two-argument `PrepareMethod(handle, instantiation[])` calls consume two. Physical 0.0.116 and 0.0.117 proved that whole-module Cecil serialization drags unrelated Constant-table dependencies (`System.Runtime`, then `Sentry`) into this otherwise tiny rewrite.

0.0.118 introduced the exact-length private-copy patch. Its first `step32-fast` run completed static validation at 1027/1027 and executed all 231 host tests in about 27 seconds total. The Step-32 end-to-end fixture itself reached Gate B successfully and reported 6/6 + 4/4 rewrites, ten exact five-byte windows, no Cecil serialization, and byte-diff confinement. The sole 230/231 failure was a stale test-only substring assertion that still expected the old unpadded `Pop + Pop` wording.

0.0.119 corrects that assertion to the exact five-byte padded detail contract and makes Codemagic cache-size telemetry failure-safe with an EXIT trap. The production transformation is byte-for-byte unchanged from 0.0.118 and uses Cecil only to bind and verify the exact receipt-backed assembly/method/sites. Gate B maps the verified method RVA to PE bytes, confirms each selected instruction is the exact five-byte `call` plus expected metadata token, and changes only those ten five-byte windows on the launcher-private clone:

- 6 × `call` → `Pop + Nop + Nop + Nop + Nop`;
- 4 × `call` → `Pop + Pop + Nop + Nop + Nop`.

Equal-length replacement preserves every later IL offset, branch displacement, EH boundary, metadata table, RVA, section layout, and file length. Gate B rejects any byte difference outside the fifty approved bytes. Gate C reopens the transformed image with the rejecting Cecil resolver and verifies the exact padded semantic plan, 10→0 PrepareMethod references, unchanged constant metadata, identity/MVID/EH topology, and the same byte-diff confinement. **No Cecil serialization occurs.**

The trusted Step-12 install remains immutable. Step 32 still performs zero real-StS2 CLR admission/invocation, no Harmony/MonoMod runtime patching, no Godot/game startup, and no native loading.

## Codemagic free-minute workflow

Run **`step32-fast` first**. It performs static validation + the complete host suite only. If it fails, stop and send its artifacts.

Only after fast preflight passes, run **`ios-step-32` on the exact same commit**. That workflow does not repeat the complete host suite; it performs static validation, iOS workload/publish, and IPA verification. Install the IPA only if both workflows passed for the same commit.

Both workflows emit `phase-timings.txt` and `cache-sizes.txt`. NuGet, the pinned Harmony regression archive, and a pristine pinned .NET SDK snapshot are cached to reduce free M2 minutes.

Expected IPA: `artifacts/StS2-Launcher-Step-32.ipa`

Physical close remains Step 32 A–D **4/4 PASS**. Preserve `Step32-RealStS2PrepareMethodRewrite.txt`.
