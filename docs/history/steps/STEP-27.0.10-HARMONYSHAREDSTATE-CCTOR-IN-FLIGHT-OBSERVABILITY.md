# Step 27.0.10 — HarmonySharedState cctor in-flight observability

Candidate: `0.0.94 (94)`

## Physical evidence entering this candidate

Physical `0.0.93 (93)` supplied a self-identifying crash checkpoint with matching installed/source provenance and the bounded Gate-S marker. Its last durable progress record is **Gate T / PatchEngineExecution / T5** immediately before:

`RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)`

`PatchProcessor.Patch()` and the launcher target were still uninvoked. Because T5 is emitted only after T1–T4 complete, the physical device has now crossed:

- T1/T2 bounded `Reflection.Emit` / `RuntimeMethodHandle` host preservation preflight; and
- T3/T4 exact `HarmonySharedState` runtime Type/.cctor/internalVersion/actualVersion reflection with the initializer still unrun.

The hard stop is therefore inside `HarmonyLib.HarmonySharedState::.cctor`, not in Gate-S descriptor registration, not in the T1 host-preservation preflight, and not in the T3/T4 runtime-reflection operation.

The raw checkpoint is preserved at `docs/history/reports/STEP-27.0.9-PHYSICAL-GATE-T5-CRASH-CHECKPOINT.txt`.

## Why this candidate observes rather than pre-executes

Harmony 2.4.2's exact shared-state initializer first obtains/creates the process-visible `HarmonySharedState` singleton type, then on Mono may enter the `StackFrame.methodAddress` / `AccessTools.FieldRefAccess<StackFrame,long>` dynamic-code branch before it copies/initializes the shared dictionaries and version field.

A diagnostic candidate could manually invoke those internals ahead of the cctor, but that would prime Harmony/MonoMod caches or generated assemblies and would change the state seen by the real initializer. Step 27.0.10 instead leaves the cctor call itself unchanged and adds output-only observation around it.

## Gate T refinement

T1–T4 remain behavior-identical to 0.0.93.

Immediately before the unchanged `RunClassConstructor` call:

- **T5a** requires that no process-visible generated `HarmonySharedState` or `MonoMod.Utils.Cil.ILGeneratorProxy` assembly already exists, then arms two bounded observers.
- The dedicated Step-27 `AssemblyLoadContext` reports managed resolver requests and completed private/host loads only while the cctor is active.
- `AppDomain.CurrentDomain.AssemblyLoad` reports only dynamic assemblies or the two exact receipt/source-backed generated names: `HarmonySharedState` and `MonoMod.Utils.Cil.ILGeneratorProxy`.
- Every such observation is sent through the existing synchronous progress/checkpoint channel, so the last durable file can advance *inside* the cctor even if iOS terminates the process without a managed exception.
- **T5b** then enters the same exact `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)` call used by 0.0.93.
- **T6** is emitted only if the cctor returns. Observers are removed before the existing version/generated-assembly/hash/isolation validation proceeds.

T7–T9 remain unchanged: exactly one public `PatchProcessor.Patch()` call follows only after T6 validation, and the launcher target remains uninvoked until Gate V.

## Interpreting the next hard-stop breadcrumb

The observer does not claim to identify a source line; it provides causal milestones without mutating Harmony state ahead of the real initializer.

- no observer event after T5b: the stop occurred before any relevant dynamic assembly-load event or dedicated-ALC resolver activity was observed;
- `HarmonySharedState` assembly load observed, but no `ILGeneratorProxy`: the singleton image loaded far enough to raise `AssemblyLoad`; the later initializer region, including the Mono field-ref branch, remains the narrower frontier;
- `MonoMod.Utils.Cil.ILGeneratorProxy` observed: the initializer reached MonoMod IL-generator proxy creation; any subsequent hard stop is later than that assembly-load milestone;
- T6 survives: the whole shared-state initializer returned, moving the physical frontier to validation or the unchanged T7 public `PatchProcessor.Patch()` call.

These are milestone interpretations, not diagnoses. The cctor may perform work before or after an assembly-load event, so the next candidate must continue to use the exact last durable record rather than infer an unobserved source instruction.

## Preserved policy

The 26 gates remain A–Z. Gate O, Gate S, the T1–T4 preservation/reflection sequence, the actual `HarmonySharedState` cctor call, public `PatchProcessor.Patch()`, and all StS2 prohibitions remain intact. `TrimMode=full`, `MtouchInterpreter=-all`, the bounded framework preservation anchors, fresh-process rule, and trusted/prepared-byte immutability are unchanged.

This is routine Step-27 physical-evidence/candidate refinement. `docs/MASTER-PLAN.md` is intentionally unchanged.
