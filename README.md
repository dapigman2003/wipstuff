# StS2 Launcher iOS — Step 32.0.1 Serialized-Fingerprint Verification Fix

Steps 01–26 are physically closed. Step 27 is closed negative for runtime Harmony/MonoMod replacement. Step 28 is closed positive at **5/5** for deterministic transform-before-load + transformed-only interpreted execution. Step 29 is closed positive at **4/4** for exact real-StS2 target auditing. Step 30 is closed positive at **4/4** and deferred the Harmony/mod-loading site. Step 31 is closed positive at **4/4** and confirmed the exact `OneTimeInitialization::PrewarmJit()` / ten-`PrepareMethod` family as eligible for an explicitly predeclared rewrite design.

## Active candidate

**Step 32.0.1 / `0.0.116 (116)` — serialized-fingerprint verification correction for the first real StS2 PrepareMethod rewrite**

Step 32 performs the first semantic write to a real `sts2.dll`, but only on a launcher-private clone. It hard-pins the physical source SHA/MVID/method token/body fingerprint and ten exact `RuntimeHelpers.PrepareMethod` sites. Six one-argument calls become one `Pop`; four two-argument calls become `Pop + Pop`. This consumes exactly the stack arguments the original void calls consumed while preserving the preceding reflection/method-handle discovery and surrounding control-flow/exception structure.

The receipt-backed Step-12 install is immutable. Step 32 performs **zero real-StS2 CLR admission/invocation**, no Harmony/MonoMod runtime patching, no Godot/game startup, and no native loading.

Codemagic 0.0.115 passed static validation **996/996**, compiled successfully, and ran the complete host suite at **230/231 PASS**. The sole failure was Step-32 Gate C after the private transformed image had been written: 0.0.115 incorrectly compared a pre-serialization, offset-sensitive IL body fingerprint against the reopened serialized method. Cecil finalizes instruction offsets during `module.Write`, so that hash was never a valid pre-write invariant.

0.0.116 leaves the 6+4 rewrite unchanged. Gate B predicts only the offset-independent semantic fingerprint; Gate C requires the reopened semantic fingerprint to match that exact plan and records the post-write physical body fingerprint as evidence. All source immutability, 10→0 call-count, instruction/Pop/EH, rejecting-resolver, and no-CLR-load invariants remain unchanged.

Workflow: `ios-step-32`

Expected IPA: `artifacts/StS2-Launcher-Step-32.ipa`

Next authority: Codemagic static validation → complete host suite → iOS publish → IPA verification → physical Step 32 A–D **4/4 PASS**. Preserve `Step32-RealStS2PrepareMethodRewrite.txt`.
