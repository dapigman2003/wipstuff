# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.22 / `0.0.106 (106)` — post-publish System.Linq framework preservation**

Physical 0.0.105 proved the new raw-method-body normalizer on the actual iPhone. The normalized `HarmonySharedState::.cctor` returned successfully and Gate T advanced into the first exact public `PatchProcessor.Patch()` call. Patch then failed normally in `HarmonyLib.MethodCreator..ctor` with:

`System.MissingMethodException: System.Linq.Enumerable.Union<T>(IEnumerable<T>, IEnumerable<T>)`

This is not a MonoMod detour failure. `MethodCreator` did not finish constructing the replacement and `PatchTools.DetourMethod` was not reached. The cause is full trimming: the real Harmony assembly is loaded only after publish, so ILLink cannot see its LINQ calls and had removed `Union<T>` from the host `System.Linq` surface even though the assembly itself was loadable.

0.0.106 therefore treats framework binding and framework member preservation as separate contracts:

- retains `TrimMode=full` and all physically proven earlier roots;
- adds one measured whole-assembly root, `System.Linq`, rather than chasing only `Union<T>`;
- keeps the 0.0.105 raw-PE `HarmonySharedState` normalizer unchanged;
- after T6 and before public `PatchProcessor.Patch()`, T6a/T6b verifies the exact `Enumerable.Select`, two-sequence `Union`, and three-selector `ToDictionary` public signatures used by the audited Harmony MethodCreator path;
- invokes none of those LINQ operators during the preflight and still does not touch a StS2 member.

The complete 0.0.105 device report is preserved in project history.

## iOS detour decision rule

The stop rule remains unchanged in substance but is now stated more precisely: a framework-member trimming failure does **not** count as a Harmony detour failure. Once T6a/T6b proves the required post-publish framework callable surface, let `PatchProcessor.Patch()` proceed. If replacement generation/dynamic execution or the actual `PatchTools.DetourMethod -> DetourFactory.Current.CreateDetour` path then fails for an iOS execution reason, perform the one representative post-publish interpreted fixture experiment. Pivot to ahead-of-load transforms only if that representative patch path also fails.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.106 (106)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Codemagic must pass the hash-pinned official Harmony 2.4.2 normalizer regression and the complete host suite before publish. Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
