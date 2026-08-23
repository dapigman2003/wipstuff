# Step 27.0.12 — Cecil OpCodes compile hardening

Candidate: `0.0.96 (96)`

## Trigger

Codemagic rejected Step 27.0.11 / `0.0.95 (95)` during host-test compilation before iOS publish or any device runtime. `ControlledHarmonyPatchExecution.cs` imports both `System.Reflection.Emit` and `Mono.Cecil.Cil`; the newly added eleven-instruction cctor rewrite used bare `OpCodes`, so Roslyn emitted eleven `CS0104` ambiguity errors between `System.Reflection.Emit.OpCodes` and `Mono.Cecil.Cil.OpCodes`.

This is build evidence only. The 0.0.95 HarmonySharedState normalization was never executed, so the latest physical runtime evidence remains 0.0.94.

## Fix

The runtime design from Step 27.0.11 is unchanged. The source adds the explicit alias:

```csharp
using CecilOpCodes = Mono.Cecil.Cil.OpCodes;
```

and all eleven `Instruction.Create(...)` calls in the normalized `HarmonySharedState::.cctor` use `CecilOpCodes`. No Gate ordering, normalized IL intent, Harmony admission rule, private-load behavior, T5/T6 validation, or `PatchProcessor.Patch()` acceptance boundary changes.

The static validator now requires the alias, requires exactly eleven `Instruction.Create(CecilOpCodes.` uses in the production boundary, and forbids any bare `Instruction.Create(OpCodes.` use there.

The master plan is unchanged.
