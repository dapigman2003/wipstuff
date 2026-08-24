# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.24 / `0.0.108 (108)` — single post-publish interpreted Harmony decision experiment**

Physical 0.0.107 proved the copy/no-link host policy removed the prior `Enumerable.Union<T>` and `DebuggableAttribute` trimming blockers. The normalized `HarmonySharedState` boundary remained viable and the first exact public `PatchProcessor.Patch()` call then failed with `System.NotImplementedException` surfaced from `HarmonyLib.PatchFunctions.UpdateWrapper`.

That is now a real patch-engine execution failure rather than another trimmed-framework-member failure, but the stack does not distinguish replacement generation from the later MonoMod detour installation. Step 27's predefined stop rule therefore allows exactly one representative experiment against managed code that is not part of the launcher's build-time iOS AOT graph.

0.0.108 performs that experiment:

- a new launcher-owned `StS2Launcher.Step27.InterpretedPatchFixture.dll` is built separately and copied into the `.app` only **after** `dotnet publish`;
- the iOS project and host-test project do not reference that fixture;
- the fixture contains the exact `Target`, in-fixture `InvokeTarget`, and Harmony `Prefix` methods plus deterministic counters;
- Gate P loads the exact fixture bytes into the Step-27 private context and creates a **fresh** processor for the interpreted Target through public `Harmony.CreateProcessor(MethodBase)`;
- Gate Q proves baseline behavior through Target reflection plus an in-fixture direct IL call to Target;
- Gate T invokes public `PatchProcessor.Patch()` exactly once;
- if patching succeeds, Gate V proves prefix/skip-original behavior through both interpreted routes, Gate W unpatches exactly that prefix once, and Gate Y proves original behavior is restored;
- no MonoMod backend override is forced and Harmony internals are not modified again;
- no StS2 member is reflected, patched, or invoked.

This is the final Harmony decision experiment. If the post-publish interpreted fixture cannot patch, Step 27 stops iterating Harmony internals and Step 28 pivots to deterministic ahead-of-load managed IL transformation. If patch/unpatch succeeds, Harmony remains viable for the representative dynamically loaded managed target model.

`MASTER-PLAN.md` remains unchanged from 0.0.107 for this candidate; the copy/no-link policy revision is already recorded there, and a patch-engine architecture pivot will be recorded only if the physical 0.0.108 result triggers it.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.108 (108)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
