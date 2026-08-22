# Step 27.0.8 — Gate-O purity restoration + Gate-T runtime-resolution decomposition

Candidate: `0.0.92 (92)`

## Physical evidence entering this candidate

Physical `0.0.91 (91)` completed Gates A–N and failed normally at Gate O / 14 of 26 with:

`System.IO.InvalidDataException: Targeted patch API reflection unexpectedly changed resolver/load counters.`

No hard crash occurred. Gate P and every later gate were not run, so 0.0.91 did **not** test its new HarmonySharedState initializer or the earlier 0.0.90 `PatchProcessor.Patch()` crash boundary. The full output is preserved at `docs/history/reports/STEP-27.0.7-PHYSICAL-GATE-O-REPORT.txt`.

The failure occurred after Gate O had snapshotted the private-context resolver/load counters. Relative to physical 0.0.90, the new post-snapshot runtime operation was exact `HarmonyLib.HarmonySharedState` Type/.cctor/internalVersion/actualVersion field reflection. The correct conclusion is therefore narrow: this runtime reflection has an observable resolver/load effect on the physical iOS runtime. The result does not identify which resolver/load counter changed, because 0.0.91 failed before reporting the deltas.

## Correction principle

Do not weaken Gate O by allowing arbitrary resolver/load changes. Restore Gate O's runtime-reflection surface to the one that physically passed in 0.0.90, while retaining the broader patch-engine audit as metadata-only admission. Move all newly introduced runtime operations into Gate T, where they can be individually checkpointed, measured, and causally attributed.

This preserves the gate method's two goals at once: high evidence per physical run and exact first-failure localization.

## Gate O

Gate O still performs three receipt-backed Cecil audits:

1. exact public PatchProcessor/HarmonyMethod patch surface;
2. exact 57-instruction AccessTools initializer fingerprint; and
3. exact HarmonySharedState -> MethodCreatorConfig.Prepare -> PatchFunctions.UpdateWrapper -> PatchTools.DetourMethod -> UpdatePatchInfo patch-engine chain.

Runtime reflection after the resolver/load snapshot is restored to the physically passing 0.0.90 surface: exact PatchProcessor AddPrefix/Patch/Unpatch, HarmonyMethod constructors/fields, and AccessTools type/cctor/cache fields. It still requires zero resolver/load/native/context-membership side effects for that proven runtime surface.

The Reflection.Emit/RuntimeMethodHandle preservation preflight and HarmonySharedState runtime reflection are **not** performed in Gate O.

## Gate T

Gate T groups the related patch-engine runtime frontier into nine durable substages:

- **T1** — enter bounded host Reflection.Emit/RuntimeMethodHandle preservation preflight.
- **T2** — require return with private-context membership unchanged, no native/rejected request, unchanged prepared bytes/probe counters, and persist exact managed/private/host load deltas.
- **T3** — enter exact HarmonySharedState runtime Type/.cctor/internalVersion/actualVersion reflection. The initializer is not run and actualVersion is not read.
- **T4** — require exact internal-static runtime shape, `internalVersion == 102`, unchanged private membership, no native/rejected request, unchanged bytes/probe counters, and persist exact resolver/load deltas. Nonzero resolver/load deltas are measured evidence for this exact operation, not a global allowlist.
- **T5** — enter exactly one `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)`.
- **T6** — require return, `actualVersion == 102`, unchanged bytes/probe counters, no private native/rejected request, and only the bounded generated assembly simple names `HarmonySharedState` / `MonoMod.Utils.Cil.ILGeneratorProxy`; reject removals, duplicates, or any other addition.
- **T7** — enter the first exact public `PatchProcessor.Patch()` invocation, exactly once.
- **T8** — Patch returned; begin exact replacement/isolation validation.
- **T9** — replacement/isolation validation completed. The launcher target remains uninvoked until Gate V.

## Why the runtime-reflection delta is admitted only in T3/T4

0.0.91 demonstrated that the exact HarmonySharedState runtime-reflection operation changes resolver/load counters on device, but it did not tell us the exact delta. 0.0.92 therefore does not guess a magic count. Instead it constrains the effect by stronger invariants: private-context membership must remain unchanged, no native resolution may occur, no rejected managed request may occur, receipt-backed bytes and probe counters must remain unchanged, and the exact deltas are emitted into the durable T4 checkpoint/report. A later candidate can pin a stable exact delta if physical evidence justifies doing so.

## Patch-engine preservation retained

The bounded `Step27PatchEngineFrameworkPreservation` DynamicDependency anchor remains because Harmony 2.4.2's audited patch path reaches dynamic method generation, Reflection.Emit helpers, and `RuntimeMethodHandle.GetFunctionPointer()` from a post-publish assembly invisible to the app trimmer. The anchor remains a bounded type/member-category list, not a Reflection.Emit assembly root. `TrimMode=full`, `MtouchInterpreter=-all`, the prohibition on broad `UseInterpreter=true`, and the NativeAOT prohibition are unchanged.

## Upstream source model

Harmony 2.4.2 `HarmonySharedState` creates/fetches a dynamic singleton type in its static constructor, may create an `AccessTools.FieldRef<StackFrame,long>` on Mono, and later stores replacement function-pointer information in `UpdatePatchInfo`. These remain source/metadata-informed candidate boundaries, not physical conclusions until the device crosses them.

References retained from the prior candidate:

- Harmony 2.4.2 `HarmonySharedState.cs`: https://github.com/pardeike/Harmony/blob/v2.4.2.0/Harmony/Internal/HarmonySharedState.cs
- Harmony 2.4.2 `MethodCreatorConfig.cs`: https://github.com/pardeike/Harmony/blob/v2.4.2.0/Harmony/Internal/MethodCreatorConfig.cs
- Harmony 2.4.2 `PatchTools.cs`: https://github.com/pardeike/Harmony/blob/v2.4.2.0/Harmony/Internal/PatchTools.cs
- Harmony issue #642: https://github.com/pardeike/Harmony/issues/642
- Harmony issue #741: https://github.com/pardeike/Harmony/issues/741
- Microsoft iOS/Mono interpreter guidance: https://learn.microsoft.com/dotnet/maui/macios/interpreter

## Still forbidden

No StS2 type/member reflection, patching, or invocation; no StS2 entry point; no Harmony broad discovery/PatchAll/category/class processor; no game/Godot startup; no native game-library load; no trusted/prepared-byte mutation.

## Documentation policy

This is routine Step-27 candidate/physical evidence. `docs/MASTER-PLAN.md` is intentionally unchanged.
