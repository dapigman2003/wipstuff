# Step 27.0.11 — iOS HarmonySharedState AOT normalization

Candidate: `0.0.95 (95)`

## Physical trigger

Physical `0.0.94 (94)` self-identified correctly and again terminated inside Gate T before T6. Its final durable event was a successful dedicated-load-context host binding of requested `netstandard, Version=2.0.0.0` to host `netstandard, Version=2.1.0.0` while `HarmonyLib.HarmonySharedState::.cctor` was running. `PatchProcessor.Patch()` and the launcher target were still uninvoked.

That event disproves `netstandard` resolution as the immediate blocker. Combined with the exact receipt-backed Harmony 2.4.2 metadata audit, the remaining cctor contains two runtime-only mechanisms that are unnecessary for this launcher's single-version, private-Harmony execution model and are poor fits for iOS AOT/interpreter constraints:

1. `GetOrCreateSharedStateType()` constructs a new `HarmonySharedState` assembly with Mono.Cecil and loads it with MonoMod `ReflectionHelper.Load`.
2. On Mono, the cctor may construct `AccessTools.FieldRefAccess<StackFrame,long>` for `StackFrame.methodAddress`.

The local Harmony state dictionaries and `actualVersion` are the actual state required by the later patch/update path. `methodAddressRef` is only an optional stack-frame fallback; Harmony checks it for null before use.

## Fix

Gate A now performs a bounded compatibility rewrite against an **in-memory runtime image only** after the exact original 0Harmony 2.4.2 patch-engine fingerprint passes.

The original `HarmonySharedState::.cctor` is replaced by exactly eleven instructions:

- construct `Dictionary<MethodBase, byte[]>` and store `state`;
- construct `Dictionary<MethodInfo, MethodBase>` and store `originals`;
- construct `Dictionary<long, MethodBase[]>` and store `originalsMono`;
- store null into `methodAddressRef`;
- store `102` into `actualVersion`;
- return.

The normalizer then writes the module to memory, reopens it through Cecil, verifies the exact eleven-instruction fingerprint, preserves the original assembly identity, computes a runtime-image SHA-1, and retains the bytes in the Gate-A preflight snapshot.

No source, receipt-backed live file, Step-21 source copy, or Step-21 prepared file is mutated.

## Load-boundary change

The Step-27 private `AssemblyLoadContext` still verifies the prepared 0Harmony file against its persisted plan SHA-1 immediately before load. For exactly that 0Harmony identity it additionally verifies the retained normalized runtime-image SHA-1 and loads the normalized bytes from a read-only `MemoryStream`. Every other prepared assembly remains loaded from its verified prepared file.

This makes the runtime-image delta explicit and bounded instead of silently mutating the trusted prepared tree.

## Gate-T acceptance

T1–T4 retain their preservation/reflection role. T5/T6 now mean:

- **T5a** — re-hash the retained normalized runtime image and require zero pre-existing known generated patch-engine assemblies;
- **T5b** — execute exactly one `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)` against the normalized direct-state cctor;
- **T6** — require `state`, `originals`, and `originalsMono` non-null, `methodAddressRef == null`, `actualVersion == 102`, zero generated `HarmonySharedState`/`ILGeneratorProxy` assemblies, unchanged prepared bytes, and unchanged launcher-probe counters.

Only after T6 may the pre-existing T7/T8 exact single `PatchProcessor.Patch()` invocation run. The launcher target remains uninvoked until Gate V.

## Scope

This is a targeted runtime-compatibility substitution, not a Harmony API fork and not a relaxation of the gate model. The exact original Harmony 2.4.2 cctor and downstream patch-engine chain remain metadata-audited. The fix deliberately gives up Harmony's cross-version/app-domain shared-state singleton semantics inside this dedicated private context; Step 27 already requires a fresh process and exactly one verified 0Harmony 2.4.2 identity, so those semantics are outside the admitted execution model.

The master plan is unchanged.
