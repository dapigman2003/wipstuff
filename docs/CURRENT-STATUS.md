# Current Status — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Physically closed boundary

**Steps 01–26 are closed on a physical iPhone.** The latest fully closed runtime boundary is **Step 26.0 / 0.0.83 (83)**: A–N 14/14 PASS, OfflineReady PASS, Foundation 5/5 PASS.

## Step 27 physical evidence

### 0.0.84 (84) — execution frontier

Physical Step 27.0 reached **17/25**: Gates A–Q PASS, Gate R PrefixRegistration FAIL. `HarmonyMethod(MethodInfo)` implicitly triggered `HarmonyLib.AccessTools::.cctor`, which threw before any `PatchProcessor.Patch()` call. This remains the furthest Step-27 execution evidence.

### 0.0.85 (85) — metadata correction evidence

Physical Step 27.0.1 reached **14/26** and failed safely at **Gate O — HarmonyPatchApiResolution** before AccessTools initialization. The new Cecil audit proved the earlier BindingFlags-only model was wrong and measured the real AccessTools initializer:

- 56 instructions, zero locals/handlers;
- `allTypesCached = null`;
- exact `all=15420` and `allDeclared=15422` BindingFlags initialization;
- `Mono.Runtime` probe;
- string lookup of `System.Runtime.InteropServices.RuntimeInformation`;
- reflected `FrameworkDescription` checks for legacy `.NET Framework` / `.NET Core` classification;
- `Dictionary<Type, HarmonyLib.FastInvokeHandler>` construction;
- `ReaderWriterLockSlim(LockRecursionPolicy)` construction.

Gate R did not run in build 85 and no patch was installed.

## Active candidate — Step 27.0.2 / 0.0.86 (86)

- Workflow: **`ios-step-27`**
- IPA: **`artifacts/StS2-Launcher-Step-27.ipa`**
- Device report: `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`
- Closed prerequisite: Step 26.0 + OfflineReady + Foundation 5/5
- StS2 reflection/patching/invocation: **forbidden**

### Gates A–N

Replay the physically closed Step 26 chain exactly.

### Gate O — HarmonyPatchApiResolution

Metadata-audit exact patch APIs plus the exact physically measured AccessTools runtime-detection/cache initializer. Require the 56-instruction fingerprint, exact fields/opcodes/strings/calls/stores, exact BindingFlags values, and no P/Invoke/handlers/locals. Before AccessTools executes, prove `RuntimeInformation` resolves by Harmony's exact string and that `FrameworkDescription`, `Dictionary<,>()`, and `ReaderWriterLockSlim(LockRecursionPolicy)` survived trimming. Do not read AccessTools static state or construct a HarmonyMethod.

### Gates P–Q

Resolve only launcher-owned `HarmonyPatchProbe.Target(int)` + `Prefix(int, ref int __result)`, then establish the unpatched direct/reflection baseline of 42.

### Gate R — AccessToolsTypeInitialization

Explicitly run only `RuntimeHelpers.RunClassConstructor(HarmonyLib.AccessTools.TypeHandle)`. Verify exact BindingFlags, runtime-classification bools, `allTypesCached == null`, empty add-handler cache, initialized/unheld cache lock, framework description, unchanged 0Harmony hash/context, zero native/unplanned requests, and unchanged launcher probe counters.

### Gates S–Z

S registers the exact launcher prefix descriptor without patching. T is the first real `PatchProcessor.Patch()` call. U audits before execution. V proves patched behavior. W exactly unpatches. X audits. Y proves restored behavior. Z performs final hashes/OfflineReady/context/native isolation.

## Still forbidden

- `Harmony.Patch`, `PatchAll`, patch classes/categories or broad discovery;
- postfix/transpiler/finalizer/inner patches;
- any StS2 member reflection, patching, or invocation;
- Godot/game startup or native game-library loading;
- mutation of trusted live/prepared bytes.

## Acceptance

Fresh process: Codemagic + host tests + publish + IPA verification PASS; install `0.0.86 (86)`; run A–Z to **26/26 PASS**; then OfflineReady PASS and Foundation 5/5 PASS.

If Gate T or later runs, force-quit before retrying. If Step 27 closes, Step 28 is the first targeted StS2 member-reflection boundary; Android reference repositories remain advisory only and every target must be re-verified against the exact receipt-backed payload.
