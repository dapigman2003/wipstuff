# Step 28.0 — Ahead-of-Load Managed Transformation

## Why this step exists

Physical Step 27.0.24 / `0.0.108 (108)` removed the last meaningful ambiguity around runtime Harmony replacement: `PatchProcessor.Patch()` still threw `System.NotImplementedException` from `PatchFunctions.UpdateWrapper` when its target was a genuine post-publish interpreted method. Per the pre-declared stop rule, runtime Harmony/MonoMod detouring is no longer the active compatibility architecture.

Step 28 therefore starts the deterministic ahead-of-load path already foreshadowed by the project plan.

## Candidate 0.0.109 question

Can the launcher combine its separately proven Cecil rewrite capability and post-publish Mono interpreter capability into one end-to-end pipeline where a verified source image is transformed **before CLR load**, only transformed bytes are admitted, and an ordinary in-assembly direct call observes the changed semantics?

The first candidate intentionally uses a project-owned fixture rather than real StS2 behavior so a failure can be attributed to the new architecture itself rather than a game-specific target.

## Fixture

`StS2Launcher.Step28.AheadOfLoadFixture.dll` is a standalone `net9.0` class library with no iOS project/content reference. Build tooling compiles it separately and copies it into `Step28AheadOfLoadFixture/` only after `dotnet publish` returns.

Source behavior:

- `Adjustment() => 1`
- `Target(value) => value + Adjustment()`
- `InvokeTarget(value) => Target(value)`

The Step-28 transformation changes only the IL constant in `Adjustment()` from `1` to `1000` in a launcher-private transformed image.

## Gates

1. **A — FixtureAdmissionAndOfflineReady**: OfflineReady; exact manifest/hash/metadata; private source clone; no fixture CLR load.
2. **B — DeterministicRewrite**: Cecil writes only a transformed private copy; source hashes unchanged; no runtime load/Harmony.
3. **C — TransformedImageVerification**: reopen source/transformed images and prove exact `1 -> 1000` semantic change plus preserved direct-call topology.
4. **D — TransformedExecution**: load only transformed bytes in a dedicated private ALC; require `Adjustment()==1000`, `Target(41)==1041`, `InvokeTarget(41)==1041`.
5. **E — FinalIsolationAudit**: re-hash all fixture images, re-prove OfflineReady, and verify exactly one Step-28 fixture identity remains in the dedicated context with no unexpected private dependency fallback.

## Explicit non-goals

- no real StS2 member reflection, rewrite, or invocation;
- no Harmony/MonoMod runtime patch API;
- no Godot/game startup;
- no native game-library loading;
- no trusted Step-12 install mutation;
- no arbitrary dependency resolver fallback.

A physical 5/5 pass closes only the combined ahead-of-load transformation/execution mechanism. The next candidate can then choose a narrowly audited real StS2 compatibility transformation.
