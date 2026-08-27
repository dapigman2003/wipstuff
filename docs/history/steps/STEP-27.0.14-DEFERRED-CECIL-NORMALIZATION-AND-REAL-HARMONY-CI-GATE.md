# Step 27.0.14 — Deferred Cecil normalization + real Harmony CI gate

Candidate: `0.0.98 (98)`

## Trigger

Physical 0.0.97 failed at Gate A before the private Step-27 load context started. The exact failure was `Step27MetadataOnlyResolver.Resolve(TypeReference)` for `System.ComponentModel.EditorBrowsableState` while `CreateIosNormalizedHarmonyRuntimeImage` was reading the receipt-backed `0Harmony.dll`.

The stack is characteristic of Mono.Cecil eager custom-attribute decoding: `ImmediateModuleReader` -> `ReadCustomAttributes` -> `CustomAttribute.ConstructorArguments` -> `ReadCustomAttributeEnum` -> `TypeReference.Resolve`. The same project already uses `ReadingMode.Deferred` for its metadata-only Harmony audits, but the new 0.0.95 normalizer had introduced two isolated `ReadingMode.Immediate` reads: the source rewrite and the post-write audit.

Raw physical evidence: `docs/history/reports/STEP-27.0.13-PHYSICAL-GATE-A-REPORT.txt`.

## Correction

`CreateIosNormalizedHarmonyRuntimeImage` now uses `ReadingMode.Deferred` for both reads.

This is not a permissive resolver change. `Step27MetadataOnlyResolver` remains fail-closed and still refuses external type/field/method resolution. The reason Deferred mode is correct here is that the normalizer needs only local `0Harmony` definitions and IL bodies; it does not need to deserialize unrelated custom-attribute argument values. Mono.Cecil's writer explicitly completes a deferred module via its immediate reader with `resolve_attributes: false`, so the rewrite can serialize the untouched metadata without resolving enum-valued attribute arguments.

The stale resolver exception label is also corrected from `Step 24 Gate A` to `Step 27 Gate A` so future evidence identifies the owning boundary correctly.

## Recurrence prevention

Recent Step-27 candidates exposed a process weakness: synthetic/textual invariants could pass while the exact production input failed before the intended runtime frontier.

0.0.98 adds a quarantined host-test fixture from exact upstream `Lib.Harmony 2.4.2`. It is restored only for tests, is excluded from normal compile/runtime references, and its `netstandard2.0/0Harmony.dll` is copied to a dedicated test folder. The host suite then invokes the actual private production normalizer against that real binary and requires:

- exact `0Harmony` / `2.4.2.0` identity;
- the `EditorBrowsableAttribute` metadata surface that exposed the 0.0.97 eager-read bug;
- successful normalization with the production fail-closed resolver;
- source bytes unchanged byte-for-byte;
- runtime image SHA different from source SHA;
- exact normalized cctor audit containing `instructions=11`.

This turns the real Harmony metadata shape into a pre-IPA CI contract instead of discovering it first on-device.

## Runtime architecture decision

The HarmonySharedState normalization remains a compatibility bridge, not proof that Harmony's native runtime detour backend is viable on iOS.

The relevant external constraints are:

1. Microsoft documents that iOS device builds are AOT because dynamically generated executable code is restricted. `MtouchInterpreter=-all` AOT-compiles build-time assemblies while retaining the Mono interpreter for dynamic managed-code generation.
2. Harmony is explicitly a runtime method-patching system, and its `HarmonySharedState` initializer creates/loads a dynamic shared-state assembly and may build a Mono `StackFrame.methodAddress` field-ref delegate.
3. MonoMod's Apple-Silicon detour work requires executable-page/JIT-memory handling such as `MAP_JIT` and JIT write-protection transitions. Apple documents a special JIT-memory model and notes that `pthread_jit_write_protect_np` is not available on iOS.
4. The Android StS2 launchers run under a normal Android .NET/Mono environment and intentionally load `STS2Mobile.dll` to apply Harmony runtime patches. Their success therefore validates the patch set and Android runtime design, not the iOS native-detour mechanism.
5. Mature iOS projects such as PPSSPP and UTM ship App-Store-compatible modes without JIT and use interpreter-based alternatives; their JIT-enabled variants require different distribution/runtime conditions.

Therefore the Step-27 stop rule is now explicit:

- First, 0.0.98 must prove the deterministic Cecil fix and reach the normalized T5/T6 boundary.
- If T6 passes and the public `PatchProcessor.Patch()` path also passes, Harmony remains the accepted runtime path.
- If T6 passes but T7/T8 fails at runtime detouring, do **one** representative launcher-owned test against a post-publish interpreted fixture (not the build-time AOT `HarmonyPatchProbe.Target`). This better matches the eventual dynamically loaded `sts2.dll` execution model while remaining inside the master-plan requirement for launcher-owned deterministic probes.
- If Harmony also cannot patch/unpatch that interpreted fixture, stop spending releases on Harmony internals. That is the threshold for a major architecture change: perform required game adaptations as deterministic ahead-of-load Mono.Cecil transforms on derived runtime copies, retain source/prepared Steam bytes immutable and hash-verified, and load only the transformed managed image. Such a pivot would require updating the master plan because it changes the patching architecture.

The current master plan is unchanged because 0.0.98 is still inside its existing launcher-owned Harmony characterization boundary.

## External references used for the decision

- Mono.Cecil `AssemblyReader.cs`: https://github.com/jbevain/cecil/blob/master/Mono.Cecil/AssemblyReader.cs
- Mono.Cecil `AssemblyWriter.cs`: https://github.com/jbevain/cecil/blob/master/Mono.Cecil/AssemblyWriter.cs
- Harmony `HarmonySharedState.cs`: https://github.com/pardeike/Harmony/blob/master/Harmony/Internal/HarmonySharedState.cs
- Harmony 2.4.2 thin package: https://www.nuget.org/packages/Lib.Harmony/2.4.2
- Ekyso StS2 Launcher: https://github.com/Ekyso/StS2-Launcher
- Ekyso Harmony fork: https://github.com/Ekyso/Harmony
- Microsoft Mono interpreter on iOS: https://learn.microsoft.com/dotnet/maui/macios/interpreter
- Apple JIT memory guidance: https://developer.apple.com/documentation/apple-silicon/porting-just-in-time-compilers-to-apple-silicon
- MonoMod Apple-Silicon detour issue: https://github.com/MonoMod/MonoMod/issues/90
- PPSSPP iOS limitations: https://www.ppsspp.org/docs/reference/ios-support/
- UTM iOS runtime modes: https://docs.getutm.app/installation/ios/
