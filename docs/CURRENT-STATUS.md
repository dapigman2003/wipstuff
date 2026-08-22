# Current Status — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Physically closed boundary

**Steps 01–26 are closed on a physical iPhone.** The latest fully closed runtime boundary is **Step 26.0 / 0.0.83 (83)**: A–N 14/14 PASS, OfflineReady PASS, Foundation 5/5 PASS.

## Step 27 physical evidence

### 0.0.84 (84) — execution frontier

Physical Step 27.0 reached **17/25**: Gates A–Q PASS, Gate R PrefixRegistration FAIL. `HarmonyMethod(MethodInfo)` implicitly triggered `HarmonyLib.AccessTools::.cctor`, which threw before any `PatchProcessor.Patch()` call. This remains the furthest clean Step-27 execution evidence.

### 0.0.85 (85) — first AccessTools metadata correction

Physical Step 27.0.1 reached **14/26** and failed safely at Gate O before AccessTools initialization. It exposed the actual runtime-detection/cache initializer: exact BindingFlags state, `Mono.Runtime`, string/reflection access to `RuntimeInformation.FrameworkDescription`, an add-handler dictionary, and `ReaderWriterLockSlim`.

### 0.0.86 (86) — instruction-count correction

Physical Step 27.0.2 again reached **14/26** at Gate O and established that the receipt-backed `AccessTools::.cctor` is **57 instructions**, not 56, with exactly one `ldc.i4.1`. No Gate R execution or patch occurred.

### 0.0.87 (87) — operand-attribution correction

Physical Step 27.0.3 again reached **14/26** at Gate O. The 57-instruction/opcode fingerprint matched, but the semantic operand check failed. Both `Type.GetType("System.Runtime.InteropServices.RuntimeInformation", bool)` calls use `throwOnError=false`; the single `ldc.i4.1` belongs to `ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion)`.

### 0.0.88 (88) — unstable N–Q region / fresh-process clarification

The user observed repeated abrupt app termination around the **N–Q** region without a surviving managed Step-27 report. One retry produced a normal **0/26 Gate-A failure** because `sts2` was already loaded in `StS2Launcher-Step27-HarmonyPatchExecution`. That rejection is expected: once Gate B has loaded the private context, the same app process is no longer a valid fresh-process Step-27 environment, whether or not Patch() was ever reached.

The 0.0.88 observation does **not** revoke the earlier 0.0.84 A–Q success; it shows that the current expanded Gate-O path needs better crash attribution before we infer a new runtime boundary.

## Active candidate — Step 27.0.5 / 0.0.89 (89)

- Workflow: **`ios-step-27`**
- IPA: **`artifacts/StS2-Launcher-Step-27.ipa`**
- Main device report: `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`
- Crash checkpoint: `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt`
- Closed prerequisite: Step 26.0 + OfflineReady + Foundation 5/5
- StS2 reflection/patching/invocation: **forbidden**

### Gates A–N

Replay the physically closed Step 26 chain exactly. The crash checkpoint is flushed before and after each gate; asynchronous audit progress is also checkpointed.

### Gate O — HarmonyPatchApiResolution

Keep the exact 57-instruction AccessTools fingerprint and exact `false/false` RuntimeInformation probes plus `SupportsRecursion (1)`. Gate O performs admission/resolution only:

- Cecil audit of exact patch APIs and AccessTools initializer;
- string resolution of `RuntimeInformation`;
- resolution of `FrameworkDescription` PropertyInfo **without invoking its getter**;
- resolution of exact `Dictionary<,>()` and `ReaderWriterLockSlim(LockRecursionPolicy)` constructor metadata;
- exact runtime reflection of PatchProcessor/HarmonyMethod/AccessTools members without reading AccessTools static fields.

Sensitive O1–O9 substages are synchronously checkpointed.

### Gates P–Q

Resolve launcher-owned `HarmonyPatchProbe.Target(int)` + `Prefix(int, ref int __result)`, then establish the unpatched direct/reflection baseline of 42.

### Gate R — AccessToolsTypeInitialization

Gate R now owns the first **reflected execution** of the preserved `RuntimeInformation.FrameworkDescription` getter, immediately followed by the explicit `RuntimeHelpers.RunClassConstructor(HarmonyLib.AccessTools.TypeHandle)` barrier. R1/R2/R3 are individually crash-checkpointed. If the process disappears, the checkpoint distinguishes reflected getter execution from AccessTools `.cctor` entry/return.

### Gates S–Z

S registers the exact launcher prefix descriptor without patching and has pre/post invocation breadcrumbs. T remains the first real `PatchProcessor.Patch()` call and has pre/post invocation breadcrumbs. U audits before patched execution. V proves patched behavior. W exactly unpatches. X audits. Y proves restored behavior. Z performs final hashes/OfflineReady/context/native isolation.

## Fresh-process rule

**Force-quit/relaunch before every Step-27 retry once any previous attempt reached Gate B.** Gate A intentionally rejects a process where `sts2` or Harmony remains loaded. If Gate T or later ran, also assume launcher patch state may remain process-resident.

If the app hard-crashes, copy `Step27-CrashCheckpoint.txt` before starting another Step-27 run so the last durable stage is not overwritten.

## Acceptance

Codemagic + host tests + publish + IPA verification PASS; install `0.0.89 (89)`; from a fresh process run A–Z to **26/26 PASS**; then OfflineReady PASS and Foundation 5/5 PASS. If Step 27 closes, Step 28 is the first targeted StS2 member-reflection boundary.
