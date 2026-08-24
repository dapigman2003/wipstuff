# StS2 Launcher iOS — Step 28 Ahead-of-Load Managed Transformation

Steps 01–26 are physically closed. Step 27 is now a **closed negative architecture result**: physical `0.0.108 (108)` proved that the exact public `HarmonyLib.PatchProcessor.Patch()` boundary still throws `System.NotImplementedException` from `PatchFunctions.UpdateWrapper` even when the target is a genuine post-publish interpreted method whose direct in-fixture IL execution was proven immediately beforehand.

Per the pre-declared stop rule, the project is no longer iterating Harmony/MonoMod runtime detours.

## Active candidate

**Step 28.0 / `0.0.109 (109)` — deterministic ahead-of-load semantic transformation + transformed-only execution**

The new active compatibility pipeline is:

`verified source -> launcher-private clone -> Cecil transform before CLR load -> reopen/hash verify -> load only transformed image -> execute through Mono interpreter`

0.0.109 deliberately proves that pipeline on a project-owned post-publish fixture before touching real StS2 behavior:

- `StS2Launcher.Step28.AheadOfLoadFixture.dll` is built separately and copied into the `.app` only after `dotnet publish`;
- the iOS project and host-test project do not reference the fixture project;
- source IL has `Adjustment() => 1`, `Target(value) => value + Adjustment()`, and `InvokeTarget(value) => Target(value)`;
- Gate A re-proves OfflineReady, hash/metadata-verifies the source fixture, clones it privately, and requires the fixture identity to be absent from the CLR;
- Gate B changes only `Adjustment()` from `1` to `1000` in a new private transformed image with Mono.Cecil;
- Gate C reopens and verifies source/transformed IL and hashes before load;
- Gate D loads only transformed bytes into a dedicated private `AssemblyLoadContext` and requires `Adjustment()==1000`, `Target(41)==1041`, and the in-fixture direct-call `InvokeTarget(41)==1041`;
- Gate E re-hashes all images and re-proves OfflineReady/isolation;
- no Harmony/MonoMod patch API and no real StS2 member reflection/invocation are used by Step 28.0.

The master plan is intentionally updated in this release because the runtime compatibility architecture has materially changed.

## Build

Workflow: `ios-step-28`

Expected app version: `0.0.109 (109)`

Expected IPA: `artifacts/StS2-Launcher-Step-28.ipa`

Physical acceptance: Step 28 A–E **5/5 PASS**. Gate E includes the required OfflineReady re-verification.
