# StS2 Launcher iOS — Step 32.0.4 Audited Constant-Metadata Resolver

Steps 01–26 are physically closed. Step 27 is closed negative for runtime Harmony/MonoMod replacement. Step 28 is closed positive at **5/5** for deterministic transform-before-load + transformed-only interpreted execution. Step 29 is closed positive at **4/4** for exact real-StS2 target auditing. Step 30 is closed positive at **4/4** and deferred the Harmony/mod-loading site. Step 31 is closed positive at **4/4** and confirmed the exact `OneTimeInitialization::PrewarmJit()` / ten-`PrepareMethod` family as eligible for an explicitly predeclared rewrite design.

## Active candidate

**Step 32.0.4 / `0.0.119 (119)` — exact audited System.Runtime + Sentry write-time metadata resolver; 6+4 rewrite semantics unchanged**

Physical `0.0.117` re-proved Gate A with OfflineReady **428/428**, exact source identity, all ten `PrepareMethod` sites, zero Cecil read-time dependency requests, zero CLR admission, and an unchanged trusted install. Gate B then failed closed **before mutation** because the verified real `sts2.dll` contains non-null external constant metadata scoped to exact `Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0`, outside the 0.0.117 writer allowlist. Preserve `docs/history/reports/STEP-32.0.2-PHYSICAL-SENTRY-CONSTANT-METADATA-FAILURE.txt`.

The exact DLL was subsequently audited statically. `docs/history/reports/STEP-32-STATIC-STS2-CONSTANT-METADATA-AUDIT.txt` proves exactly three non-null external type/storage requirements: `System.Reflection.BindingFlags / Int32` under exact System.Runtime 9.0.0.0, plus `Sentry.BreadcrumbLevel / Int32` and `Sentry.SentryLevel / Int16` under exact Sentry 5.0.0.0. Null-only GodotSharp/System.Collections Constant rows remain outside resolver authority. User-confirmed Codemagic success for `0.0.118` establishes the lean active-source baseline.

`0.0.119` changes only `ConstantMetadataWriteResolver`: before mutation it requires the source module's distinct non-null external requirement set to match those three audited entries exactly, synthesizes them in per-exact-assembly in-memory surrogates, and rejects every other requirement or write-time resolution identity. No external dependency bytes are opened by this resolver. The Step-32 semantic rewrite remains exactly 6 × one-argument `PrepareMethod` → `Pop` and 4 × two-argument `PrepareMethod` → `Pop + Pop`.

The receipt-backed Step-12 install remains immutable. Step 32 performs **zero real-StS2 CLR admission/invocation**, no Harmony/MonoMod runtime patching, no Godot/game startup, and no native loading.

Workflow: `ios-step-32`

Expected IPA: `artifacts/StS2-Launcher-Step-32.ipa`

Next authority: Codemagic static validation → complete host suite → iOS publish → IPA verification → physical Step 32 A–D **4/4 PASS**. Preserve `Step32-RealStS2PrepareMethodRewrite.txt`.
