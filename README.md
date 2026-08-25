# StS2 Launcher iOS — Step 32.0.2 Bounded Cecil Write-Metadata Resolver Fix

Steps 01–26 are physically closed. Step 27 is closed negative for runtime Harmony/MonoMod replacement. Step 28 is closed positive at **5/5** for deterministic transform-before-load + transformed-only interpreted execution. Step 29 is closed positive at **4/4** for exact real-StS2 target auditing. Step 30 is closed positive at **4/4** and deferred the Harmony/mod-loading site. Step 31 is closed positive at **4/4** and confirmed the exact `OneTimeInitialization::PrewarmJit()` / ten-`PrepareMethod` family as eligible for an explicitly predeclared rewrite design.

## Active candidate

**Step 32.0.2 / `0.0.117 (117)` — bounded Cecil write-time constant-metadata resolution for the first real StS2 PrepareMethod rewrite**

Step 32 still performs the first semantic write to a real `sts2.dll`, only on a launcher-private clone. The semantic transformation is unchanged: six one-argument `RuntimeHelpers.PrepareMethod` calls become one `Pop`; four two-argument calls become `Pop + Pop`. The exact physical Step-31 source SHA/MVID/method token/body fingerprint and ten offsets remain hard-pinned.

Physical 0.0.116 passed Gate A with OfflineReady **428/428**, exact source identity, all ten sites, zero Cecil read-time resolution, and no CLR admission. Gate B then failed during `module.Write` because Mono.Cecil needed the declared type of an unrelated external enum constant to encode the Constant table and requested exact `System.Runtime, Version=9.0.0.0, PublicKeyToken=b03f5f7f11d50a3a` through the all-rejecting resolver. Preserve `docs/history/reports/STEP-32.0.1-PHYSICAL-CECIL-WRITE-RESOLUTION-FAILURE.txt`.

0.0.117 keeps read/audit resolvers rejecting. Only the serialization phase gets an in-memory constant-metadata surrogate for that exact `System.Runtime` identity. The surrogate is synthesized from constant values already present in the verified source metadata, opens no external framework/game assembly bytes, has no directory probing/fallback, and rejects every other assembly request. Gate C additionally requires source/transformed Constant-table semantic fingerprints to match exactly.

The receipt-backed Step-12 install remains immutable. Step 32 performs **zero real-StS2 CLR admission/invocation**, no Harmony/MonoMod runtime patching, no Godot/game startup, and no native loading.

Workflow: `ios-step-32`

Expected IPA: `artifacts/StS2-Launcher-Step-32.ipa`

Next authority: Codemagic static validation → complete host suite → iOS publish → IPA verification → physical Step 32 A–D **4/4 PASS**. Preserve `Step32-RealStS2PrepareMethodRewrite.txt`.
