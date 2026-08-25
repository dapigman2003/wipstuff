# StS2 Launcher iOS — Step 28 Ahead-of-Load Managed Transformation

Steps 01–26 are physically closed. Step 27 is a **closed negative architecture result**: physical `0.0.108 (108)` proved that the exact public `HarmonyLib.PatchProcessor.Patch()` boundary still throws `System.NotImplementedException` from `PatchFunctions.UpdateWrapper` even when the target is a genuine post-publish interpreted method whose direct in-fixture IL execution was proven immediately beforehand.

Per the pre-declared stop rule, the project is no longer iterating Harmony/MonoMod runtime detours.

## Codemagic 0.0.109 result

Step 28.0 / `0.0.109 (109)` passed canonical static validation **845/845** and built every external managed fixture, including `StS2Launcher.Step28.AheadOfLoadFixture.dll`. `StS2Launcher.Core` compilation then stopped before MSTest with `CS0246` because `AheadOfLoadManagedTransformation.cs` referenced `CallbackProgress<T>` without declaring the established callback-backed `IProgress<T>` helper. No host-test verdict, iOS publish, IPA, or physical-device result exists for 0.0.109. The raw Codemagic host/build output is preserved at `docs/history/reports/STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt`.

## Active candidate

**Step 28.0.1 / `0.0.110 (110)` — compile correction; Step-28 experiment unchanged**

0.0.110 adds only the missing private `CallbackProgress<T> : IProgress<T>` adapter and static validation that pins its declaration and forwarding behavior. The active compatibility pipeline remains:

`verified source -> launcher-private clone -> Cecil transform before CLR load -> reopen/hash verify -> load only transformed image -> execute through Mono interpreter`

The unchanged Step-28 mechanism is still proven on a project-owned post-publish fixture before touching real StS2 behavior:

- `StS2Launcher.Step28.AheadOfLoadFixture.dll` is built separately and copied into the `.app` only after `dotnet publish`;
- the iOS project and host-test project do not reference the fixture project;
- source IL has `Adjustment() => 1`, `Target(value) => value + Adjustment()`, and `InvokeTarget(value) => Target(value)`;
- Gate A re-proves OfflineReady, hash/metadata-verifies the source fixture, clones it privately, and requires the fixture identity to be absent from the CLR;
- Gate B changes only `Adjustment()` from `1` to `1000` in a new private transformed image with Mono.Cecil;
- Gate C reopens and verifies source/transformed IL and hashes before load;
- Gate D loads only transformed bytes into a dedicated private `AssemblyLoadContext` and requires `Adjustment()==1000`, `Target(41)==1041`, and the in-fixture direct-call `InvokeTarget(41)==1041`;
- Gate E re-hashes all images and re-proves OfflineReady/isolation;
- no Harmony/MonoMod patch API and no real StS2 member reflection/invocation are used by Step 28.

`MASTER-PLAN.md` is intentionally unchanged in 0.0.110 because this candidate does not change architecture, methodology, roadmap, or end-state assumptions.

## Build

Workflow: `ios-step-28`

Expected app version: `0.0.110 (110)`

Expected IPA: `artifacts/StS2-Launcher-Step-28.ipa`

Next authority: Codemagic compile -> complete host suite -> iOS publish -> IPA verification. If those pass, physical acceptance remains Step 28 A–E **5/5 PASS**, with Gate D reporting **1000 / 1041 / 1041** and Gate E including the required OfflineReady re-verification.
