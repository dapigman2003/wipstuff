# Step 27.0.23 — Dynamic-Payload No-Trim Host Policy

## Evidence

Physical 0.0.106 reached Gate T after the raw `HarmonySharedState` normalization and the System.Linq preservation correction. The prior `Enumerable.Union<T>` failure was gone. `PatchProcessor.Patch()` advanced into `HarmonyLib.MethodPatcherTools.CreateDynamicMethod`, where `MonoMod.Utils.DynamicMethodDefinition` type initialization failed because `System.Diagnostics.DebuggableAttribute` could not be resolved from the trimmed host framework. `PatchTools.DetourMethod` was not reached.

## Decision

Do not add a one-off `DebuggableAttribute` preservation annotation. Two sequential physical failures on unrelated ordinary BCL surface (`System.Linq.Enumerable.Union<T>` and `System.Diagnostics.DebuggableAttribute`) demonstrate that publish-time member trimming is incompatible with the launcher's intentional post-publish managed payload model.

The iOS host therefore changes from `TrimMode=full` to `MtouchLink=None` + `TrimMode=copy`. `MtouchInterpreter=-all` remains. This is a master-plan platform-policy change, not a Harmony/Step-28 pivot.

## Scope

The Harmony runtime experiment itself remains unchanged except for diagnostic wording. No StS2 member is reflected, patched, or invoked. The next physical run should establish whether the patch path can continue beyond `DynamicMethodDefinition` once ILLink member removal is no longer an ambiguity.
