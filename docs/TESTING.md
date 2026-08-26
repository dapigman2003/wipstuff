# Testing — Step 32.0.2 Bounded Cecil Write-Metadata Resolver Fix

Active candidate: Step 32.0.2 / `0.0.117 (117)`.

## Authority sequence

1. `scripts/validate.sh` — canonical static policy/provenance validation.
2. `scripts/test.sh` — complete host regression suite.
3. `scripts/build-ios.sh` — publish/package the iOS candidate.
4. `scripts/verify-ipa.sh` — verify release identity, native closure, fixture rules, and IPA structure.
5. Physical iPhone — final Step-32 A–D authority.

Release identity remains workflow `ios-step-32`, IPA `StS2-Launcher-Step-32.ipa`, TRX `step32.trx`, now version `0.0.117 (117)`.

### Step 32.0.2 regression focus

The host end-to-end Step-32 fixture must include an unrelated constant whose declared type is an external enum scoped to exact `System.Runtime 9.0.0.0`. Gate B must reproduce Cecil's Constant-table write-time resolution need and satisfy it only through the production in-memory constant-metadata surrogate. The test must still prove source bytes unchanged, exact 10→0 PrepareMethod rewrite, 6+4 Pop semantics, and reopened verification.

Static/runtime guards must require:

- `ReadingMode.Deferred` and rejecting resolvers for source admission and reopened verification;
- no `DefaultAssemblyResolver`, search directories, framework file probing, `Assembly.Load`, `LoadFromStream`, `LoadFromAssemblyPath`, or `LoadFromAssemblyName` in Step 32;
- the only Gate-B write-time assembly identity permitted is exact `System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a`;
- the write resolver synthesizes only constant enum metadata from values already present in the verified source module;
- zero external framework/game assembly bytes are opened by that resolver;
- source/transformed Constant-table semantic fingerprints match exactly;
- the offset-independent transformed method semantic fingerprint remains the pre-write→reopen IL invariant;
- the physical transformed body hash remains post-write evidence only.

### Physical acceptance

Force-quit before running Step 32. Require A–D **4/4 PASS**. Gate B should report 6/6 + 4/4 replacements plus only exact System.Runtime synthetic write-time resolution; Gate C should report PrepareMethod references **10 / 0** and identical source/transformed constant-metadata fingerprints; Gate D must re-prove OfflineReady and immutable source/no-CLR isolation.
