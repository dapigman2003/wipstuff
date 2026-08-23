# Release Checklist — Step 27.0.12

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve physical 0.0.89 AddPrefix, 0.0.90 Patch(), 0.0.91 Gate-O, 0.0.93 original-cctor entry, and 0.0.94 successful netstandard-binding-before-crash evidence.
- Physical 0.0.94 still terminated before T6; `PatchProcessor.Patch()` and the launcher target were uninvoked.
- Codemagic 0.0.95 stopped in host compilation with eleven CS0104 `OpCodes` ambiguities; no 0.0.95 runtime evidence exists.
- The cctor normalizer must import `CecilOpCodes = Mono.Cecil.Cil.OpCodes`, use that alias for all eleven generated instructions, and contain no bare `Instruction.Create(OpCodes.` calls.
- Gate O and Gate S behavior remain unchanged.
- Gate A may normalize only after the exact original Harmony 2.4.2 patch-engine metadata fingerprint passes.
- The source/live/prepared 0Harmony files must never be rewritten; normal prepared SHA/length verification remains authoritative.
- The runtime image must preserve the exact 0Harmony assembly identity, be byte-distinct from the source image, and expose exactly the audited 11-instruction `HarmonySharedState::.cctor`.
- The normalized cctor may only initialize `state`, `originals`, `originalsMono`, null `methodAddressRef`, set `actualVersion=102`, and return. It must not call `GetOrCreateSharedStateType`, `ReflectionHelper.Load`, or `FieldRefAccess`.
- Gate B may use the retained memory image only for the exact admitted 0Harmony identity; all other prepared assemblies load normally.
- T5a must reverify the runtime-image hash and reject pre-existing known generated patch-engine assemblies.
- T5b must contain the single `RuntimeHelpers.RunClassConstructor(harmonySharedStateType.TypeHandle)` call.
- T6 must prove the three dictionaries are non-null, `methodAddressRef` is null, `actualVersion == 102`, generated shared-state/proxy assemblies are absent, prepared bytes are unchanged, and isolation/probe counters remain valid.
- T7/T8/T9 retain exactly one public `PatchProcessor.Patch()` path; no internal patch method substitutes for acceptance.
- Crash checkpoints self-identify installed/source version, candidate, Gate-S implementation, and Gate-T implementation.
- No protected Step 23/24/25/26 behavior is weakened.
- `TrimMode=full`, `MtouchInterpreter=-all`, established roots/preservation policies remain active; `UseInterpreter=true` and NativeAOT remain prohibited.
- StS2 reflection/patching/invocation, broad Harmony discovery, Godot startup, and native game libraries remain absent.
- The master document is unchanged.

## Build identity / visible app identity

- version: `0.0.96 (96)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.12**, bundle-derived **Version 0.0.96**, current iOS HarmonySharedState AOT-normalization status.

## Device-run discipline

- Force-quit/relaunch before the run.
- Once Gate B starts, force-quit before any retry.
- Preserve `Step27-CrashCheckpoint.txt` before another attempt after any abrupt termination.
- Interpret the final checkpoint conservatively: T5a means the normalized image was reverified; T5b means its cctor was entered; T6 means that cctor returned and direct-state validation began; T7 means public `PatchProcessor.Patch()` was entered; T8/T9 mean Patch returned / Gate-T validation advanced.

## Authority

Require static validation, host tests, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
