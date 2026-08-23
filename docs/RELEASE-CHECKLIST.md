# Release Checklist — Step 27.0.19

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve physical 0.0.89 AddPrefix, 0.0.90 Patch(), 0.0.91 Gate-O, 0.0.93 original-cctor entry, 0.0.94 successful netstandard-binding-before-crash, and 0.0.97 Gate-A Cecil evidence.
- Physical 0.0.97 failed before Gate B in `CreateIosNormalizedHarmonyRuntimeImage` because `ReadingMode.Immediate` eagerly decoded unrelated custom-attribute arguments and hit forbidden `System.ComponentModel.EditorBrowsableState` resolution.
- Both normalizer reads must use `ReadingMode.Deferred`; `ReadingMode.Immediate` must remain absent from `ControlledHarmonyPatchExecution.cs`.
- The Step-27 metadata-only resolver must remain fail-closed. Do not add a special framework/type resolution exception for `EditorBrowsableState`.
- The public constructor remains pinned to exact `0Harmony` / `2.4.2.0`; only that canonical target may enter production HarmonySharedState normalization.
- Internal non-canonical synthetic targets retain byte-identical runtime bytes and may not be forced through the real-Harmony patch-engine fingerprint.
- The canonical production path still requires the exact original Harmony 2.4.2 patch-engine metadata fingerprint and a byte-distinct runtime image.
- The cctor normalizer must import `CecilOpCodes = Mono.Cecil.Cil.OpCodes`, use that alias for all eleven generated instructions, and contain no bare `Instruction.Create(OpCodes.` calls.
- The source/live/prepared 0Harmony files must never be rewritten; normal prepared SHA/length verification remains authoritative.
- The normalized production cctor may only initialize `state`, `originals`, `originalsMono`, null `methodAddressRef`, set `actualVersion=102`, and return. It must not call `GetOrCreateSharedStateType`, `ReflectionHelper.Load`, or `FieldRefAccess`.
- Gate B may use retained memory bytes only for the exact admitted target identity.
- T5a must reverify the runtime-image hash and reject pre-existing known generated patch-engine assemblies.
- T5b contains the single `RuntimeHelpers.RunClassConstructor(harmonySharedStateType.TypeHandle)` call.
- T6 proves the three dictionaries non-null, `methodAddressRef` null, `actualVersion == 102`, generated shared-state/proxy assemblies absent, prepared bytes unchanged, and isolation/probe counters valid.
- T7/T8/T9 retain exactly one public `PatchProcessor.Patch()` path; no internal patch method substitutes for acceptance.
- The real-Harmony regression fixture must come from the exact tagged official `Harmony-Fat.2.4.2.0.zip` URL in `scripts/test.sh`; MSBuild/NuGet package layout must not be used. Because physical Codemagic 0.0.101 proved the release has no netstandard2.0 implementation, the script must select exactly one `net9.0/0Harmony.dll` member at archive root or under a release wrapper as a **host-only structural surrogate**, retain the original archive member name, print all `0Harmony.dll` members on mismatch, extract only the unique match to `artifacts/host-step27-fixtures`, and pass its absolute path through `STS2_STEP27_REAL_HARMONY_FIXTURE`. Production admission remains governed only by the exact prepared StS2 0Harmony fingerprint.
- The real-Harmony host regression must invoke `CreateIosNormalizedHarmonyRuntimeImage`, require exact 2.4.2 identity, require the net9 `System.Runtime` profile and no `netstandard` reference, detect the `EditorBrowsableAttribute` metadata surface without resolving its argument values, preserve source bytes, and produce an 11-instruction byte-distinct runtime image.
- Crash checkpoints self-identify installed/source version, candidate, Gate-S implementation, and Gate-T implementation.
- `TrimMode=full`, `MtouchInterpreter=-all`, established roots/preservation policies remain active; `UseInterpreter=true` and NativeAOT remain prohibited.
- StS2 reflection/patching/invocation, broad Harmony discovery, Godot startup, and native game libraries remain absent.
- The master document is unchanged.

## Build identity / visible app identity

- version: `0.0.103 (103)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.19**, bundle-derived **Version 0.0.103**, net9 structural-surrogate reference-graph assertion-fix status.

## Device-run discipline

- Force-quit/relaunch before the run.
- Once Gate B starts, force-quit before any retry.
- Preserve `Step27-CrashCheckpoint.txt` before another attempt after any abrupt termination.
- T5a means normalized production image reverified; T5b means normalized cctor entered; T6 means normalized cctor returned and direct-state validation began; T7 means public `PatchProcessor.Patch()` was entered; T8/T9 mean Patch returned / Gate-T validation advanced.
- If T6 passes but T7/T8 fails, do not add another speculative Harmony-internal workaround. The next experiment is one post-publish interpreted launcher-owned patch/unpatch fixture. Failure there is the documented threshold for proposing ahead-of-load Cecil transformation and changing the master plan.

## Authority

Require static validation, full Codemagic host tests including the real-Harmony fixture, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
