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

### 0.0.89 (89) — Gate-S hard crash localized

Physical Step 27.0.5 crash telemetry finally localized the abrupt termination. The last synchronously flushed checkpoint was **Gate S / PrefixRegistration / S1**, immediately after entering exact `PatchProcessor.AddPrefix(MethodInfo)` reflection invocation and before S2 could record a return. Therefore Gate R had completed far enough for Gate S to begin, and Gate T (`PatchProcessor.Patch()`) was still not reached. The crash is inside the AddPrefix convenience path, whose measured body constructs `HarmonyMethod(MethodInfo)` and stores it into `PatchProcessor.prefix`.


### 0.0.90 (90) — Gate-T hard crash localized inside PatchProcessor.Patch()

Physical Step 27.0.6 advanced through the bounded annotation-free prefix descriptor path and reached **Gate T / PatchEngineExecution / T1**. The last synchronously flushed checkpoint records entry into the first exact `PatchProcessor.Patch()` reflection invocation while the launcher target was still uninvoked. No T2 checkpoint survived. Therefore the physical frontier moved from AddPrefix into the public patch engine itself; the exact internal failing operation remains unproven.

The raw breadcrumb is preserved at `docs/history/reports/STEP-27.0.6-PHYSICAL-GATE-T-CRASH-CHECKPOINT.txt`.

## Active candidate — Step 27.0.7 / 0.0.91 (91)

- Workflow: **`ios-step-27`**
- IPA: **`artifacts/StS2-Launcher-Step-27.ipa`**
- Main device report: `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`
- Crash checkpoint: `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt`
- Closed prerequisite: Step 26.0 + OfflineReady + Foundation 5/5
- StS2 reflection/patching/invocation: **forbidden**

### Gates A–N

Replay the physically closed Step 26 chain exactly. The crash checkpoint is flushed before and after each gate; asynchronous audit progress is also checkpointed.

### Gate O — HarmonyPatchApiResolution

Gate O remains admission/resolution only. It retains the exact 57-instruction AccessTools fingerprint and additionally audits the receipt-backed Harmony patch-engine closure now known to sit behind Gate T:

- exact public `PatchProcessor.AddPrefix(MethodInfo)`, `Patch()`, and `Unpatch(MethodInfo)` plus bounded `HarmonyMethod` fields/constructors;
- exact internal `HarmonySharedState` static type, `.cctor`, `GetOrCreateSharedStateType`, `GetPatchInfo`, `UpdatePatchInfo`, `internalVersion == 102`, and `actualVersion` field shape;
- exact `MethodCreatorConfig.Prepare`, `MethodPatcherTools.CreateDynamicMethod`, `PatchFunctions.UpdateWrapper`, and `PatchTools.DetourMethod` call relationships;
- bounded host preflight for `RuntimeInformation`, `DynamicMethod`, `ILGenerator`, Reflection.Emit builders, method/field reflection, and `RuntimeMethodHandle.GetFunctionPointer`;
- no `HarmonySharedState` initialization, dynamic replacement generation, detour installation, or launcher target invocation.

Sensitive O1–O12 substages are synchronously checkpointed.

### Gates P–Q

Resolve launcher-owned `HarmonyPatchProbe.Target(int)` + `Prefix(int, ref int __result)`, then establish the unpatched direct/reflection baseline of 42.

### Gate R — AccessToolsTypeInitialization

Gate R owns the first reflected execution of the preserved `RuntimeInformation.FrameworkDescription` getter, immediately followed by explicit `RuntimeHelpers.RunClassConstructor(HarmonyLib.AccessTools.TypeHandle)`. Physical 0.0.90 reached Gate T, so this AccessTools boundary was traversed by that device run.

### Gate S — bounded prefix descriptor

Gate S keeps the 0.0.90 path: exact `HarmonyMethod()` construction, require `priority=-1` and `method=null`, assign only the launcher Prefix `MethodInfo`, then assign only `PatchProcessor.prefix`. `AddPrefix(MethodInfo)`, `HarmonyMethod(MethodInfo)`, and `ImportMethod` remain reference-audited but uninvoked.

### Gate T — explicit shared-state initialization + public Patch()

Gate T now decomposes the physically crashing patch-engine boundary while keeping the public acceptance call intact:

- T1: enter exactly one `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)`;
- T2: require return, `actualVersion == 102`, unchanged bytes/probe counters and no private native/rejected request; admit only the exact runtime-generated `HarmonySharedState` / `MonoMod.Utils.Cil.ILGeneratorProxy` names, reject removals/duplicates/any other addition, then snapshot the resulting private-context membership;
- T3: enter the first exact public `PatchProcessor.Patch()` reflection invocation, exactly once;
- T4: require return, validate the replacement, apply the same bounded generated-assembly transition rule, and snapshot the exact post-patch membership;
- T5: replacement/isolation validation complete.

The candidate adds only bounded `DynamicDependency` preservation for the exact post-publish Reflection.Emit/MethodHandle framework surface used by the audited Harmony/MonoMod patch-engine path. `TrimMode=full`, `MtouchInterpreter=-all`, and the prohibition on broad `UseInterpreter=true` remain unchanged.

### Gates U–Z

U audits before patched execution. V proves patched behavior. W removes exactly the prefix by `MethodInfo`. X audits. Y proves restored behavior. Z performs final hashes/OfflineReady/context/native isolation. No StS2 member is touched.

## Fresh-process rule

**Force-quit/relaunch before every Step-27 retry once any previous attempt reached Gate B.** Gate A intentionally rejects a process where `sts2` or Harmony remains loaded. If Gate T or later ran, also assume launcher patch state may remain process-resident.

If the app hard-crashes, copy `Step27-CrashCheckpoint.txt` before starting another Step-27 run so the last durable stage is not overwritten.

## Acceptance

Codemagic + host tests + publish + IPA verification PASS; install `0.0.91 (91)`; from a fresh process run A–Z to **26/26 PASS**; then OfflineReady PASS and Foundation 5/5 PASS. If Step 27 closes, Step 28 is the first targeted StS2 member-reflection boundary.
