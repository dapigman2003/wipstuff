# Step 27.0.15 — Real-Harmony test namespace compile hardening

## Evidence

Codemagic build 0.0.98 compiled `StS2Launcher.Core` successfully but stopped while compiling `StS2Launcher.Core.Tests`. The newly added real-Harmony regression imported both `System.Reflection` and `Mono.Cecil` and declared its local custom-attribute helper with the bare name `ICustomAttributeProvider`. Roslyn reported CS0104 because both namespaces define that interface; the follow-on CS1503 diagnostics were consequences of the unresolved helper parameter type. The real Harmony 2.4.2 normalizer test therefore never executed, and no IPA/device runtime evidence was produced.

## Correction

Step 27.0.15 / 0.0.99 adds an explicit test-only alias:

```csharp
using CecilCustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
```

`HasEditorBrowsableAttributeSurface` uses only that alias. Production `ControlledHarmonyPatchExecution`, its Deferred-Cecil reads, the exact 11-instruction `HarmonySharedState::.cctor` normalization, and Gates S/T are unchanged from 0.0.98.

## Regression protection

The static validator now requires the Cecil custom-attribute-provider alias in the real-Harmony test and rejects the ambiguous bare helper declaration. The 0.0.98 Codemagic compiler report is retained under `docs/history/reports`.

## Acceptance

Codemagic must compile the full host-test project and actually execute the real merged Lib.Harmony 2.4.2 normalizer regression before IPA publication. If that passes, the physical objective remains T6 and then the single public `PatchProcessor.Patch()` boundary at T7/T8.
