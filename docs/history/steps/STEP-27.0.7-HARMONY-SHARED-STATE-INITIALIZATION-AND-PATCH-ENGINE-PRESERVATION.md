# Step 27.0.7 — Harmony shared-state initialization + patch-engine preservation

Candidate: `0.0.91 (91)`

## Physical evidence entering this candidate

Physical `0.0.90 (90)` advanced beyond the `0.0.89` AddPrefix crash. Its last synchronously flushed checkpoint is Gate T / PatchEngineExecution / T1:

`entering the first exact PatchProcessor.Patch() reflection invocation; launcher target is still not invoked.`

No T2 checkpoint survived. Therefore A–S completed far enough for Gate T to begin and the process terminated inside the first exact public `PatchProcessor.Patch()` invocation. This does **not** prove which internal operation failed. The launcher target was not invoked. The raw physical breadcrumb is preserved at `docs/history/reports/STEP-27.0.6-PHYSICAL-GATE-T-CRASH-CHECKPOINT.txt`.

## Exact patch-engine source/metadata model

The candidate uses two independent inputs:

1. exact Harmony `v2.4.2.0` upstream source as design/reference documentation; and
2. Gate O Cecil inspection of the receipt-backed post-publish `0Harmony 2.4.2.0` assembly as runtime admission authority.

The exact public `PatchProcessor.Patch()` body first accesses `HarmonySharedState.GetPatchInfo(original)`. `HarmonySharedState::.cctor` creates or finds the dynamic `HarmonySharedState` singleton and, on Mono when `StackFrame.methodAddress` exists, constructs an `AccessTools.FieldRef` through MonoMod dynamic method generation. Later patch creation enters `MethodCreatorConfig.Prepare`, creates a `DynamicMethodDefinition`, obtains an `ILGenerator`, creates the replacement, detours the original through `DetourFactory.Current.CreateDetour`, and finally `HarmonySharedState.UpdatePatchInfo` may read the replacement `MethodHandle.GetFunctionPointer()` on Mono.

These are candidate boundaries, not physical conclusions. Gate O rejects metadata drift before executing them.

## Candidate change

A–S remain causally unchanged from `0.0.90` except Gate O gains additional admission checks. The public patch API is **not bypassed**.

### Gate O

Gate O now additionally:

- audits exact internal `HarmonySharedState`, `PatchFunctions`, `MethodCreatorConfig`, `MethodPatcherTools`, and `PatchTools` members;
- requires `HarmonySharedState.internalVersion == 102` and records the exact `.cctor`/shared-state/replacement/detour/update IL audits;
- proves the expected call-chain relationships without executing them;
- preflights the bounded host `Reflection.Emit` and `RuntimeMethodHandle` surface required by that exact patch-engine closure;
- still does not initialize `HarmonySharedState`, create replacement code, install a detour, or invoke the launcher target.

### Gate T

Gate T is decomposed into one related patch-engine experiment:

- **T1** — enter exactly one `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)`; public `Patch()` remains uninvoked.
- **T2** — shared-state initializer returned; require `actualVersion == 102`, unchanged prepared hash, unchanged launcher counters, no private native/rejected managed request, and a bounded private-context transition. Exact Harmony/MonoMod-generated assemblies are admitted only by simple-name allowlist (`HarmonySharedState`, `MonoMod.Utils.Cil.ILGeneratorProxy`); removals, duplicates, or any other private-context addition fail closed. The exact post-T membership is snapshotted for U–Z.
- **T3** — enter the first exact public `PatchProcessor.Patch()` reflection invocation, exactly once.
- **T4** — public `Patch()` returned; validate the replacement `MethodInfo`, apply the same bounded generated-assembly transition rule, and snapshot the exact resulting private-context membership.
- **T5** — replacement/isolation validation completed; launcher target remains uninvoked until Gate V.

This grouping deliberately uses the gate method to maximize evidence from one physical run while preserving exact causal localization.

## Candidate trimming/AOT adaptation

`Step27PatchEngineFrameworkPreservation` roots only a bounded list of framework member categories used from post-publish Harmony/MonoMod IL: `DynamicMethod`, `ILGenerator`, method/field reflection, `RuntimeMethodHandle`, and the narrow `AssemblyBuilder`/`ModuleBuilder`/`TypeBuilder`/`MethodBuilder` surface visible in the audited patch-engine source. It does **not** root a whole Reflection.Emit assembly, does not enable `UseInterpreter=true`, and does not enable NativeAOT.

The established iOS policy remains `TrimMode=full` + `MtouchInterpreter=-all`: assemblies stay AOT-targeted while the Mono interpreter remains available for runtime/dynamic managed-code generation.

## Anticipated next boundaries

If T1 returns, T2 proves the shared-state initializer itself survived. A later hard stop can then be attributed more narrowly:

- after T3 but before T4: still inside public `Patch()`, with shared state already initialized;
- replacement-generation path: `DynamicMethodDefinition` / `ILGenerator` / `MethodCreator.CreateReplacement`;
- detour path: `DetourFactory.Current.CreateDetour`;
- shared-state update path: replacement `MethodHandle.GetFunctionPointer()`.

No candidate claim says any of those later boundaries work until physical evidence crosses them.

## Source-review risks carried forward

Two upstream load-context issues are relevant enough to watch but are **not** treated as diagnoses for the physical 0.0.90 crash:

- Harmony issue #642 documented a Godot exported-build failure where MonoMod generated `ILGeneratorProxy` into the wrong `AssemblyLoadContext`; the reported fix loads the generated module into the executing assembly's context. Because Step 27 intentionally hosts `0Harmony` in a dedicated ALC, the candidate records and bounds the generated singleton/proxy assemblies rather than incorrectly requiring Step-26 membership to remain byte-for-byte unchanged once the patch engine starts.
- Harmony issue #741 remains open and describes duplicate `HarmonySharedState` behavior across multiple loading contexts in non-Unity processes. Step 27 therefore requires a fresh process and rejects duplicate generated singleton/proxy assemblies. This is an advisory compatibility risk, not evidence that #741 caused the present iOS termination.

References reviewed for this candidate:

- Harmony 2.4.2 exact `HarmonySharedState.cs`: https://github.com/pardeike/Harmony/blob/v2.4.2.0/Harmony/Internal/HarmonySharedState.cs
- Harmony 2.4.2 exact `MethodCreatorConfig.cs`: https://github.com/pardeike/Harmony/blob/v2.4.2.0/Harmony/Internal/MethodCreatorConfig.cs
- Harmony 2.4.2 exact `PatchTools.cs`: https://github.com/pardeike/Harmony/blob/v2.4.2.0/Harmony/Internal/PatchTools.cs
- Harmony issue #642: https://github.com/pardeike/Harmony/issues/642
- Harmony issue #741: https://github.com/pardeike/Harmony/issues/741
- Microsoft iOS/Mono interpreter guidance (`MtouchInterpreter=-all`): https://learn.microsoft.com/dotnet/maui/macios/interpreter

## Still forbidden

No StS2 type/member reflection, patching, or invocation; no StS2 entry point; no Harmony broad discovery/PatchAll/category/class processor; no game/Godot startup; no native game-library load; no trusted/prepared-byte mutation.

## Documentation policy

This is a Step-27 candidate/history update. The master document is intentionally unchanged.
