# StS2 Launcher iOS — Step 32.0.3 Retired Harmony Active-Surface Trim

Steps 01–26 are physically closed. Step 27 is closed negative for runtime Harmony/MonoMod replacement. Step 28 is closed positive at **5/5** for deterministic transform-before-load + transformed-only interpreted execution. Step 29 is closed positive at **4/4** for exact real-StS2 target auditing. Step 30 is closed positive at **4/4** and deferred the Harmony/mod-loading site. Step 31 is closed positive at **4/4** and confirmed the exact `OneTimeInitialization::PrewarmJit()` / ten-`PrepareMethod` family as eligible for an explicitly predeclared rewrite design.

## Active candidate

**Step 32.0.3 / `0.0.118 (118)` — maintenance-only active-surface trim; Step-32 rewrite semantics unchanged**

Physical `0.0.117` re-proved Gate A with OfflineReady **428/428**, exact source identity, all ten `PrepareMethod` sites, zero Cecil read-time dependency requests, zero CLR admission, and an unchanged trusted install. Gate B then failed closed **before mutation** because the verified real `sts2.dll` contains external constant metadata scoped to exact `Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0`, which is outside the 0.0.117 writer allowlist. Preserve `docs/history/reports/STEP-32.0.2-PHYSICAL-SENTRY-CONSTANT-METADATA-FAILURE.txt`.

`0.0.118` intentionally does **not** correct that Sentry boundary. It keeps `RealStS2PrepareMethodRewrite.cs` and the exact 6+4 transformation unchanged while removing the physically retired Step 25–27 runtime-Harmony implementation, tests, iOS controls, CI Harmony-Fat download, DynamicDependency preservation anchors, and Step-27 interpreted fixture from the active compile/package graph. The complete pre-trim 0.0.117 candidate is retained inertly in `history.zip`.

The immediate authority for 0.0.118 is Codemagic static validation → host tests → iOS publish → IPA verification, with wall-clock comparison against the pre-trim pipeline. A physical Step-32 rerun is not expected to close the Sentry blocker because the writer policy is deliberately unchanged.

The receipt-backed Step-12 install remains immutable. Step 32 performs **zero real-StS2 CLR admission/invocation**, no Harmony/MonoMod runtime patching, no Godot/game startup, and no native loading.

Workflow: `ios-step-32`

Expected IPA: `artifacts/StS2-Launcher-Step-32.ipa`

Next authority: Codemagic static validation → complete host suite → iOS publish → IPA verification → physical Step 32 A–D **4/4 PASS**. Preserve `Step32-RealStS2PrepareMethodRewrite.txt`.
