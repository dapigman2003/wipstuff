# Release Checklist — Step 27.0.10

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve physical 0.0.89 AddPrefix, 0.0.90 Patch(), 0.0.91 Gate-O, and 0.0.93 HarmonySharedState T5 evidence.
- Physical 0.0.93 crossed T1–T4 and hard-terminated after entering `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)`; `PatchProcessor.Patch()` and launcher target were uninvoked.
- Gate O and Gate S behavior remain unchanged.
- T1–T4 behavior remains unchanged and physically crossed.
- T5a may only arm bounded output-only observation; it must not manually invoke HarmonySharedState internals or pre-generate the singleton/proxy.
- T5b must contain the single existing `RuntimeHelpers.RunClassConstructor(harmonySharedStateType.TypeHandle)` call.
- Dedicated-ALC diagnostic callbacks are active only around the cctor and report resolver/load activity through the existing crash-checkpoint progress channel.
- Process `AssemblyLoad` observation is restricted to dynamic assemblies or exact generated names `HarmonySharedState` / `MonoMod.Utils.Cil.ILGeneratorProxy`.
- T6 removes observers before version/generated-assembly/hash/isolation validation.
- T7/T8/T9 retain exactly one public `PatchProcessor.Patch()` path; no internal patch method substitutes for acceptance.
- Crash checkpoints self-identify installed/source version, candidate, Gate-S implementation, and Gate-T implementation.
- No protected Step 23/24/25/26 behavior is weakened.
- `TrimMode=full`, `MtouchInterpreter=-all`, established roots/preservation policies remain active; `UseInterpreter=true` and NativeAOT remain prohibited.
- StS2 reflection/patching/invocation, broad Harmony discovery, Godot startup, and native game libraries remain absent.
- The master document is unchanged.

## Build identity / visible app identity

- version: `0.0.94 (94)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.10**, bundle-derived **Version 0.0.94**, current HarmonySharedState cctor-observability status.

## Device-run discipline

- Force-quit/relaunch before the run.
- Once Gate B starts, force-quit before any retry.
- Preserve `Step27-CrashCheckpoint.txt` before another attempt after any abrupt termination.
- Interpret the final checkpoint conservatively:
  - last T5b with no observer event => no relevant dynamic assembly-load/dedicated-ALC event survived before termination;
  - `process AssemblyLoad: HarmonySharedState...` => the generated singleton assembly loaded far enough to raise the event;
  - `process AssemblyLoad: MonoMod.Utils.Cil.ILGeneratorProxy...` => MonoMod proxy generation reached its assembly-load event;
  - T6 => the cctor returned;
  - T7 => inside public `PatchProcessor.Patch()`;
  - T8/T9 => Patch returned / Gate-T validation advanced.
- Observer milestones are not source-line diagnoses.

## Authority

Require static validation, host tests, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
