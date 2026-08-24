# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.23 / `0.0.107 (107)` — dynamic-payload no-trim host policy**

Physical 0.0.106 proved the `System.Linq` whole-assembly root fixed the previous `Enumerable.Union<T>` blocker. `PatchProcessor.Patch()` advanced into `HarmonyLib.MethodPatcherTools.CreateDynamicMethod`, where `MonoMod.Utils.DynamicMethodDefinition` type initialization failed because `System.Diagnostics.DebuggableAttribute` could not be resolved from the trimmed host framework.

This is the second independent ordinary-BCL trimming failure caused by the real Harmony/MonoMod payload arriving only after iOS publish. It still occurs before `PatchTools.DetourMethod -> DetourFactory.Current.CreateDetour`, so it is not evidence that Harmony's detour backend itself is incompatible with iOS.

0.0.107 changes the host policy instead of adding another one-off root:

- `MtouchLink=None`;
- `TrimMode=copy`;
- `MtouchInterpreter=-all` remains unchanged;
- the raw-PE `HarmonySharedState` normalization and public `PatchProcessor.Patch()` boundary remain otherwise unchanged;
- prior measured roots remain in the project as historical/protection descriptors but are no longer the mechanism relied upon for post-publish member survival;
- no StS2 member is reflected, patched, or invoked.

The master plan is revised only for this trimming-policy architecture change. Harmony remains on trial until the patch path reaches replacement generation and the real MonoMod detour boundary without linker-induced missing framework members.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.107 (107)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
