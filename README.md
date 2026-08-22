# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 has progressively localized the iOS/AOT compatibility frontier from `PatchProcessor.AddPrefix(MethodInfo)` in 0.0.89, into public `PatchProcessor.Patch()` in 0.0.90, through the 0.0.91 Gate-O loader-effect regression, and now into the exact `HarmonyLib.HarmonySharedState::.cctor` boundary.

## Active candidate

**Step 27.0.10 / `0.0.94 (94)`** follows physical `0.0.93 (93)`, whose self-identifying crash checkpoint crossed T1–T4 and hard-terminated after T5 entered `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)`. `PatchProcessor.Patch()` and the launcher target were still uninvoked.

- Gate S remains the bounded `HarmonyMethod()` descriptor path and never invokes `PatchProcessor.AddPrefix(MethodInfo)`.
- Gate O remains on the physically passing 0.0.90 runtime-reflection surface while retaining receipt-backed Cecil audit of the HarmonySharedState/replacement/detour chain.
- Gate T1/T2 retains the bounded host `Reflection.Emit`/`RuntimeMethodHandle` preservation preflight.
- Gate T3/T4 retains the exact HarmonySharedState runtime Type/.cctor/version-field reflection that 0.0.93 physically crossed.
- Gate T5a now requires no pre-existing generated `HarmonySharedState`/`MonoMod.Utils.Cil.ILGeneratorProxy` assembly and arms bounded output-only resolver/assembly-load observers.
- Gate T5b enters the **unchanged** `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)` call. No HarmonySharedState internal operation is manually invoked or pre-run.
- If the cctor loads a generated singleton/proxy assembly or requests an assembly through the dedicated Step-27 ALC, that milestone is synchronously written to `Step27-CrashCheckpoint.txt` before control continues.
- T6–T9 remain the existing validation + exactly one public `PatchProcessor.Patch()` path. The launcher target is not invoked until Gate V.
- Crash checkpoints include installed/source release identity, active candidate, Gate-S implementation, and the Gate-T cctor-observer marker.
- `TrimMode=full`, `MtouchInterpreter=-all`, the fresh-process rule, and all StS2/Godot/native-game prohibitions remain unchanged.

The master document is unchanged; this is a Step-27 evidence/localization refinement.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.94 (94)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
