# Current Status — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Physically closed boundary

**Steps 01–26 are closed on a physical iPhone.**

The latest closed runtime boundary is **Step 26.0 / 0.0.83 (83)**.

Physical Step 26 evidence reported by the user:

- Gates A–N: **14/14 PASS**;
- OfflineReady afterward: **PASS**;
- Foundation afterward: **5/5 PASS**.

This physically establishes the full Step-25 Harmony state plus exact `Harmony.CreateProcessor(MethodBase)` / `HarmonyLib.PatchProcessor` metadata admission, explicit `PatchProcessor` type initialization, launcher-owned inert target `MethodInfo` resolution, and creation of one empty `PatchProcessor` retaining the exact Harmony instance and launcher-owned target. No patch was applied and no StS2 member was reflected or invoked.

## Active candidate — Step 27

Step 27.0 / `0.0.84 (84)` crosses the first real Harmony method-replacement boundary, **only against a launcher-owned deterministic probe**.

- Codemagic workflow: **`ios-step-27`**
- IPA: **`artifacts/StS2-Launcher-Step-27.ipa`**
- Device report: `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`
- Closed prerequisite: Step 26.0 + OfflineReady + Foundation 5/5
- New frontier: exact launcher-owned prefix registration, one `PatchProcessor.Patch()` call, observed patched execution, exact prefix unpatch, and observed restoration
- StS2 member reflection/patching/invocation: **forbidden**

### Gates A–N — Proven Step 26 replay

Reproduce the complete closed Step 26 chain in the Step 27 private context, ending with the exact empty `PatchProcessor` retaining the exact Harmony instance and launcher-owned Step-26 processor probe.

### Gate O — HarmonyPatchApiResolution

Cecil-audit and targeted-reflect only the exact measured `PatchProcessor.AddPrefix(MethodInfo)`, parameterless `Patch() -> MethodInfo`, `Unpatch(MethodInfo)`, `HarmonyMethod(MethodInfo)`, `PatchProcessor.prefix`, and `HarmonyMethod.method` surface. No patch descriptor is constructed and no patch is applied.

### Gate P — LauncherPatchProbeResolution

Resolve only launcher-owned `HarmonyPatchProbe.Target(int)` and `HarmonyPatchProbe.Prefix(int, ref int __result)` in the default host context. Require the exact parameter names needed by Harmony. Do not invoke either method and do not reflect StS2.

### Gate Q — BaselineProbeInvocation

Reset probe counters and establish original launcher behavior through both direct and reflection routes: input `41` must return `42`, target body count must become 2, and prefix count must remain 0.

### Gate R — PrefixRegistration

Invoke only exact `PatchProcessor.AddPrefix(MethodInfo)` with the launcher prefix. Verify the constructed exact `HarmonyMethod` retains that exact prefix `MethodInfo`. `Patch()` remains uncalled and neither launcher method may execute.

### Gate S — PatchEngineExecution

This is the first real Harmony patch-engine boundary. Re-hash exact `0Harmony`, then invoke **exactly one** `PatchProcessor.Patch()` against the launcher-owned target. Retain the returned replacement `MethodInfo`, but do not invoke the patched target yet.

### Gate T — PostPatchAudit

Before any patched execution, re-hash the plan/prepared/live managed bytes, re-prove OfflineReady, require exact private-context membership, and require zero native or rejected/unplanned managed requests.

### Gate U — PatchedProbeInvocation

Invoke the launcher target once through reflection and once directly. Both must return `1041`; the launcher prefix must run twice; the original target body count must stay at the pre-patch value, proving the prefix returned `false` and skipped the original body.

### Gate V — ExactPrefixUnpatch

Invoke only exact `PatchProcessor.Unpatch(MethodInfo)` with the exact launcher prefix `MethodInfo`. Do not invoke the launcher target yet.

### Gate W — PostUnpatchAudit

Audit exact hash/context/native/resolver state after unpatch and before restored invocation.

### Gate X — RestoredProbeInvocation

Invoke the launcher target through reflection and direct routes again. Both must return original value `42`; target-body count must advance while prefix count remains unchanged, proving original behavior is restored.

### Gate Y — FinalIsolationAudit

Re-hash plan/prepared/live bytes, re-prove OfflineReady, require exact Step-26 private-context membership, exact retained Harmony/processor/prefix identities, `Harmony.DEBUG=false`, zero native attempts, zero rejected/unplanned requests, and the exact restored-behavior snapshot.

## Still forbidden

- `Harmony.Patch`, `PatchAll`, `PatchCategory`, `PatchClassProcessor`, or broad patch discovery;
- postfix/transpiler/finalizer/inner-patch registration;
- StS2 entry-point/type/member reflection, patching, or invocation;
- broad `Activator`/`CreateInstance`;
- Godot/game startup;
- native game-library loading;
- mutation of trusted live/prepared game bytes.

## Acceptance required for Step 27 closure

From a fresh process:

1. Codemagic static validation + host tests + iOS publish + IPA verification = PASS;
2. install `0.0.84 (84)`;
3. run Step 27 A–Y and stop at the first failure;
4. require summary **25/25 PASS**;
5. run OfflineReady = **PASS**;
6. run Foundation = **5/5 PASS**.

After Gate B the real managed context remains process-resident. If Gate S or later runs, assume launcher probe patch state may also remain process-resident until force-quit. Do not retry Step 27 or run earlier fresh-process runtime regressions in the same process after a failure at or beyond Gate S.

## Next frontier if Step 27 closes

Step 28 should cross the **first targeted StS2 member-reflection boundary only**: resolve and inspect one carefully selected real StS2 type/member without invoking or patching it. Actual StS2 patch installation remains a later boundary.
