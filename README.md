# StS2 Launcher iOS — Step 28 Ahead-of-Load Managed Transformation

Steps 01–26 are physically closed. Step 27 is a **closed negative architecture result**: physical `0.0.108 (108)` proved that the exact public `HarmonyLib.PatchProcessor.Patch()` boundary still throws `System.NotImplementedException` from `PatchFunctions.UpdateWrapper` even when the target is a genuine post-publish interpreted method whose direct in-fixture IL execution was proven immediately beforehand.

Per the pre-declared stop rule, the project is no longer iterating Harmony/MonoMod runtime detours.

## Codemagic 0.0.109 result

Step 28.0 / `0.0.109 (109)` passed canonical static validation **845/845** and built every external managed fixture, including `StS2Launcher.Step28.AheadOfLoadFixture.dll`. `StS2Launcher.Core` compilation then stopped before MSTest with `CS0246` because `AheadOfLoadManagedTransformation.cs` referenced `CallbackProgress<T>` without declaring the established callback-backed `IProgress<T>` helper. No host-test verdict, iOS publish, IPA, or physical-device result exists for 0.0.109. The raw Codemagic host/build output is preserved at `docs/history/reports/STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt`.

## Active candidate

**Step 28.0.2 / `0.0.111 (111)` — deferred Cecil metadata admission correction; Step-28 experiment unchanged**

Codemagic 0.0.110 proved the prior compile fix and advanced through the full host runner: **216/217 tests passed**. The only failure was the Step-28 end-to-end regression at Gate A, before rewrite or CLR load. `ReadingMode.Immediate` caused Mono.Cecil to eagerly decode unrelated custom-attribute arguments and request `System.Runtime, Version=9.0.0.0` through the deliberately rejecting metadata resolver.

0.0.111 changes only Step-28 fixture module reads to `ReadingMode.Deferred` while retaining `RejectingAssemblyResolver`. The active compatibility pipeline remains:

verified receipt-backed source → launcher-private clone → deterministic Cecil transformation before CLR load → reopen/hash verification → transformed-only private `AssemblyLoadContext` execution through the proven interpreter host.

The Step-28 acceptance experiment remains unchanged: the project-owned post-publish fixture begins with `Adjustment() => 1`, the private transformed image changes it to `1000`, Gate D must prove **1000 / 1041 / 1041**, and Gate E must re-prove OfflineReady and isolation. No real StS2 member is changed yet.

`MASTER-PLAN.md` is intentionally unchanged in 0.0.111 because this is a narrow implementation correction, not an architecture/roadmap change.

Expected app version: `0.0.111 (111)`

## Build

Workflow: `ios-step-28`

Expected app version: `0.0.111 (111)`

Expected IPA: `artifacts/StS2-Launcher-Step-28.ipa`

Next authority: Codemagic compile -> complete host suite -> iOS publish -> IPA verification. If those pass, physical acceptance remains Step 28 A–E **5/5 PASS**, with Gate D reporting **1000 / 1041 / 1041** and Gate E including the required OfflineReady re-verification.
