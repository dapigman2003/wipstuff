# StS2 Launcher iOS — Step 32 First Real StS2 PrepareMethod Rewrite

Steps 01–26 are physically closed. Step 27 is closed negative for runtime Harmony/MonoMod replacement. Step 28 is closed positive at **5/5** for deterministic transform-before-load + transformed-only interpreted execution. Step 29 is closed positive at **4/4** for exact real-StS2 target auditing. Step 30 is closed positive at **4/4** and deferred the Harmony/mod-loading site. Step 31 is closed positive at **4/4** and confirmed the exact `OneTimeInitialization::PrewarmJit()` / ten-`PrepareMethod` family as eligible for an explicitly predeclared rewrite design.

## Active candidate

**Step 32.0 / `0.0.115 (115)` — first real StS2 PrepareMethod rewrite**

Step 32 performs the first semantic write to a real `sts2.dll`, but only on a launcher-private clone. It hard-pins the physical source SHA/MVID/method token/body fingerprint and ten exact `RuntimeHelpers.PrepareMethod` sites. Six one-argument calls become one `Pop`; four two-argument calls become `Pop + Pop`. This consumes exactly the stack arguments the original void calls consumed while preserving the preceding reflection/method-handle discovery and surrounding control-flow/exception structure.

The receipt-backed Step-12 install is immutable. Step 32 performs **zero real-StS2 CLR admission/invocation**, no Harmony/MonoMod runtime patching, no Godot/game startup, and no native loading.

Workflow: `ios-step-32`

Expected IPA: `artifacts/StS2-Launcher-Step-32.ipa`

Next authority: Codemagic static validation → complete host suite → iOS publish → IPA verification → physical Step 32 A–D **4/4 PASS**. Preserve `Step32-RealStS2PrepareMethodRewrite.txt`.
