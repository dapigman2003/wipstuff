# Step 27.0.24 — physical negative closure / architecture decision

Physical `0.0.108 (108)` executed the single stop-rule experiment defined by Step 27.0.24.

The candidate admitted `StS2Launcher.Step27.InterpretedPatchFixture.dll`, which was copied into the app only after `dotnet publish` and was not an iOS project/content/AOT input. Gate Q proved both reflection invocation of `Target(41)` and the fixture's own direct managed IL call through `InvokeTarget(41)` returned the original value `42` before patching.

Gate S then registered the exact annotation-free prefix through the bounded `HarmonyMethod()` descriptor path. Gate T invoked the exact public `PatchProcessor.Patch()` boundary against a fresh processor whose `original` was the post-publish interpreted `Target` MethodInfo. The call threw `System.NotImplementedException: Arg_NotImplementedException` from `HarmonyLib.PatchFunctions.UpdateWrapper`.

Result: **19/26, first failure PatchEngineExecution**.

This removes the earlier AOT-target ambiguity. Runtime Harmony/MonoMod method replacement is no longer the active compatibility architecture for this project. Per the pre-declared stop rule, no further Harmony-internal workaround candidate follows. Step 28 begins deterministic ahead-of-load managed IL transformation on verified launcher-private images before CLR admission.

Raw physical evidence is preserved in `docs/history/reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt`.
