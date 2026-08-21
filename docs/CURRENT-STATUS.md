# Current Status — Step 26 Controlled Empty Harmony PatchProcessor Creation

## Physically closed boundary

**Steps 01–25 are closed on a physical iPhone.**

The latest closed runtime boundary is **Step 25.0.2 / 0.0.82 (82)**.

Physical Step 25.0.2 evidence reported by the user:

- Gates A–I: **9/9 PASS**;
- OfflineReady afterward: **PASS**;
- Foundation afterward: **5/5 PASS**.

This physically establishes exact Harmony API resolution, explicit completion of the measured `HarmonyLib.Harmony` type initializer, and construction of one inert `HarmonyLib.Harmony` object in the strict private context. The bounded Step-25 `DynamicDependency` preservation anchor for the measured `Harmony(string)` framework surface is now protected platform policy, alongside the previously proven `System.Collections.Concurrent` root. `TrimMode=full` and `MtouchInterpreter=-all` remain unchanged.

## Active candidate — Step 26

Step 26.0 / `0.0.83 (83)` advances to the smallest patch-engine object boundary without patching anything.

- Codemagic workflow: **`ios-step-26`**
- IPA: **`artifacts/StS2-Launcher-Step-26.ipa`**
- Device report: `Documents/StS2Launcher/Reports/Step26-ControlledHarmonyProcessorCreation.txt`
- Closed prerequisite: Step 25.0.2 + OfflineReady + Foundation 5/5
- New frontier: exact `Harmony.CreateProcessor(MethodBase)` / `HarmonyLib.PatchProcessor` creation using one launcher-owned inert probe method
- Patch execution: **forbidden**
- StS2 member reflection/invocation: **forbidden**

### Gates A–I — Proven Step 25 replay

Reproduce the complete closed Step 25 chain in the Step 26 context: exact input/initializer preflight, Step 23 load state, Step 24 module initialization, Step 25 Harmony API resolution/type initialization/inert instance construction, and post-construction audit.

### Gate J — HarmonyProcessorApiResolution

Cecil-audit and then targeted-reflect only the exact `Harmony.CreateProcessor(MethodBase)` / `PatchProcessor` surface. Require the measured thin factory, the measured field-storage-only processor constructor, exact retained fields, and the exact `PatchProcessor::.cctor` locker initializer. Do not initialize or construct `PatchProcessor`.

### Gate K — PatchProcessorTypeInitialization

Explicitly complete only the measured `PatchProcessor::.cctor` using `RuntimeHelpers.RunClassConstructor`, with exact hash/context/native/resolver isolation.

### Gate L — LauncherProbeResolution

Resolve only launcher-owned `HarmonyProcessorProbe.Target(int)` in the default host context. Do not invoke it and do not reflect StS2.

### Gate M — HarmonyProcessorCreation

Invoke only exact `Harmony.CreateProcessor(MethodBase)` using the retained Step-25 Harmony object and launcher probe `MethodInfo`. Verify the returned exact `PatchProcessor` retains the exact Harmony instance and exact probe `MethodBase`. Do not call `Patch()`.

### Gate N — PostProcessorAudit

Re-hash plan/prepared/live bytes, re-prove OfflineReady, and require exact retained processor/context/native/resolver state.

## Still forbidden

- `PatchProcessor.Patch`;
- `Harmony.Patch`, `PatchAll`, patch categories, patch-class discovery, or unpatching;
- `HarmonyMethod`, prefix/postfix/transpiler/finalizer creation;
- StS2 entry-point/type/member reflection or invocation;
- broad `Activator`/`CreateInstance`;
- Godot/game startup;
- native game-library loading;
- mutation of trusted live/prepared game bytes.

## Acceptance required for Step 26 closure

From a fresh process:

1. Codemagic static validation + host tests + iOS publish + IPA verification = PASS;
2. install `0.0.83 (83)`;
3. run Step 26 A–N and stop at the first failure;
4. require summary **14/14 PASS**;
5. run OfflineReady = **PASS**;
6. run Foundation = **5/5 PASS**.

After Gate B, the real managed game/Harmony context remains process-resident. Force-quit before rerunning fresh-process Step 21/22/23/24/25 regressions.

## Next frontier if Step 26 closes

Do not immediately patch StS2. The next candidate should separate patch-description (`HarmonyMethod` / prefix object) construction from any actual method replacement. A launcher-owned probe target should remain the preferred target until the patch engine itself is physically characterized.
