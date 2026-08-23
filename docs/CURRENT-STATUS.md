# Current Status — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Physically closed boundary

**Steps 01–26 are closed on a physical iPhone.** The latest fully closed runtime boundary is **Step 26.0 / 0.0.83 (83)**: A–N 14/14 PASS, OfflineReady PASS, Foundation 5/5 PASS.

## Step 27 physical evidence

### 0.0.84 (84) — execution frontier

Physical Step 27.0 reached **17/25**: Gates A–Q PASS, Gate R PrefixRegistration FAIL. `HarmonyMethod(MethodInfo)` implicitly triggered `HarmonyLib.AccessTools::.cctor`, which threw before any `PatchProcessor.Patch()` call. This remains the furthest clean Step-27 execution evidence.

### 0.0.85 (85) — first AccessTools metadata correction

Physical Step 27.0.1 reached **14/26** and failed safely at Gate O before AccessTools initialization. It exposed the actual runtime-detection/cache initializer: exact BindingFlags state, `Mono.Runtime`, string/reflection access to `RuntimeInformation.FrameworkDescription`, an add-handler dictionary, and `ReaderWriterLockSlim`.

### 0.0.86 (86) — instruction-count correction

Physical Step 27.0.2 again reached **14/26** at Gate O and established that the receipt-backed `AccessTools::.cctor` is **57 instructions**, not 56, with exactly one `ldc.i4.1`. No Gate R execution or patch occurred.

### 0.0.87 (87) — operand-attribution correction

Physical Step 27.0.3 again reached **14/26** at Gate O. The 57-instruction/opcode fingerprint matched, but the semantic operand check failed. Both `Type.GetType("System.Runtime.InteropServices.RuntimeInformation", bool)` calls use `throwOnError=false`; the single `ldc.i4.1` belongs to `ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion)`.

### 0.0.88 (88) — unstable N–Q region / fresh-process clarification

The user observed repeated abrupt app termination around the **N–Q** region without a surviving managed Step-27 report. One retry produced a normal **0/26 Gate-A failure** because `sts2` was already loaded in `StS2Launcher-Step27-HarmonyPatchExecution`. That rejection is expected: once Gate B has loaded the private context, the same app process is no longer a valid fresh-process Step-27 environment, whether or not Patch() was ever reached.

The 0.0.88 observation does **not** revoke the earlier 0.0.84 A–Q success; it shows that the current expanded Gate-O path needs better crash attribution before we infer a new runtime boundary.

### 0.0.89 (89) — Gate-S hard crash localized

Physical Step 27.0.5 crash telemetry finally localized the abrupt termination. The last synchronously flushed checkpoint was **Gate S / PrefixRegistration / S1**, immediately after entering exact `PatchProcessor.AddPrefix(MethodInfo)` reflection invocation and before S2 could record a return. Therefore Gate R had completed far enough for Gate S to begin, and Gate T (`PatchProcessor.Patch()`) was still not reached. The crash is inside the AddPrefix convenience path, whose measured body constructs `HarmonyMethod(MethodInfo)` and stores it into `PatchProcessor.prefix`.


### 0.0.90 (90) — Gate-T hard crash localized inside PatchProcessor.Patch()

Physical Step 27.0.6 advanced through the bounded annotation-free prefix descriptor path and reached **Gate T / PatchEngineExecution / T1**. The last synchronously flushed checkpoint records entry into the first exact `PatchProcessor.Patch()` reflection invocation while the launcher target was still uninvoked. No T2 checkpoint survived. Therefore the physical frontier moved from AddPrefix into the public patch engine itself; the exact internal failing operation remains unproven.

The raw breadcrumb is preserved at `docs/history/reports/STEP-27.0.6-PHYSICAL-GATE-T-CRASH-CHECKPOINT.txt`.


### 0.0.91 (91) — clean Gate-O regression, no Gate-T execution

Physical Step 27.0.7 replayed **Gates A–N PASS** and then failed normally at **Gate O / HarmonyPatchApiResolution (14/26)**. The failure was `System.IO.InvalidDataException: Targeted patch API reflection unexpectedly changed resolver/load counters.` No hard crash occurred. Because Gate O failed, Gate P and every later gate—including the new HarmonySharedState initialization and the former Patch() crash boundary—were not reached.

The only newly added runtime-reflection block after Gate O's resolver/load snapshot was the HarmonySharedState Type/.cctor/version-field inspection. The candidate therefore proved that this newly introduced runtime reflection has an observable loader/resolver effect on the physical iOS runtime. It did **not** prove that HarmonySharedState initialization or PatchProcessor.Patch() was fixed. The full raw report is preserved at `docs/history/reports/STEP-27.0.7-PHYSICAL-GATE-O-REPORT.txt`.

### 2026-08-22 supplied Gate-S checkpoint — provenance conflict, not a new runtime frontier

A supplied checkpoint had a fresh UTC timestamp but reported **Gate S / S1 — entering exact `PatchProcessor.AddPrefix(MethodInfo)` reflection invocation**. That text is the archived 0.0.89 Gate-S/S1 path, not executable 0.0.92 source. Because that legacy schema lacked app/source identity, it was treated as a provenance mismatch rather than new runtime evidence. Step 27.0.9 / 0.0.93 added fail-closed release identity and self-identifying crash telemetry without changing the patch path.

### 0.0.93 (93) — provenance confirmed; hard crash is inside HarmonySharedState::.cctor

The next physical checkpoint self-identifies **App version 0.0.93 (93)**, **Expected source version 0.0.93 (93)**, the Step 27.0.9 candidate, and the bounded Gate-S implementation marker. Its last durable record is **Gate T / PatchEngineExecution / T5 — entering `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)`**, with `PatchProcessor.Patch()` and the launcher target still uninvoked.

That advances the physical frontier past T1–T4: the bounded host Reflection.Emit/RuntimeMethodHandle preservation preflight returned, and the exact HarmonySharedState runtime Type/.cctor/version-field reflection also returned. The abrupt termination is therefore inside `HarmonyLib.HarmonySharedState::.cctor` before T6. The raw breadcrumb is preserved at `docs/history/reports/STEP-27.0.9-PHYSICAL-GATE-T5-CRASH-CHECKPOINT.txt`.

### 0.0.94 (94) — original cctor survives host netstandard binding, still terminates before T6

Physical Step 27.0.10 self-identified **App version 0.0.94 (94)** and **Expected source version 0.0.94 (94)**. Its last durable T5 observer record shows the dedicated Step-27 load context completed the host binding `netstandard, Version=2.0.0.0` => `netstandard, Version=2.1.0.0` while the original `HarmonyLib.HarmonySharedState::.cctor` was running. The process then still terminated before T6; `PatchProcessor.Patch()` and the launcher target remained uninvoked.

This proves the observed netstandard request itself was satisfied and moves the actionable problem back inside the remaining original cctor work. The raw checkpoint is preserved at `docs/history/reports/STEP-27.0.10-PHYSICAL-GATE-T5-OBSERVER-CRASH-CHECKPOINT.txt`.

### 0.0.95 (95) — Codemagic compile stop; runtime fix not exercised

Codemagic never reached iOS publish or device packaging for Step 27.0.11. The host-test compilation failed with eleven `CS0104` errors in `ControlledHarmonyPatchExecution.cs`: bare `OpCodes` in the new Cecil initializer rewrite was ambiguous between `System.Reflection.Emit.OpCodes` and `Mono.Cecil.Cil.OpCodes`. This is a compile-only defect introduced by the normalizer source; it provides no negative runtime evidence about the HarmonySharedState AOT-normalization design. The exact compiler output is preserved at `docs/history/reports/STEP-27.0.11-CODEMAGIC-CS0104-HOST-COMPILE-FAILURE.txt`.

### 0.0.96 (96) — compile fixed; 209/211 host tests pass; synthetic preflight scope regression

Codemagic proved the Cecil `OpCodes` compile correction works: the project compiled, the host test runner executed all **211** tests, and **209 passed**. The two failures were `SyntheticStep26ReplayThroughEmptyProcessorStillPassesBeforePatchBoundary` and `GateCReportsThrowingModuleInitializerAndDoesNotAdvance`. Both failed at Gate A with the same message: the new HarmonySharedState normalization attempted the full exact 0Harmony 2.4.2 patch-engine audit against a randomized minimal synthetic Harmony-like fixture, where the internal patch-engine types intentionally do not exist.

This is a host-test scope defect, not physical runtime evidence and not a failure of the normalized production cctor. The public production constructor is already pinned to exact `0Harmony` 2.4.2; Step 27.0.13 therefore restores the synthetic A–N replay by applying the production normalizer only to that canonical target, while internal non-canonical test targets retain their exact original bytes. The full 0.0.96 host report is preserved at `docs/history/reports/STEP-27.0.12-CODEMAGIC-HOST-TEST-FAILURE.txt`.

### 0.0.97 (97) — deterministic Gate-A Cecil eager-read regression

Physical Step 27.0.13 failed **0/26 at Gate A / InitializationPreflight** before the private load context, Harmony runtime initialization, `PatchProcessor.Patch()`, or launcher-target invocation. The exact stage was `HarmonySharedState iOS runtime-image normalization`. Mono.Cecil attempted to decode an enum-valued custom-attribute argument and the deliberately rejecting `Step27MetadataOnlyResolver` correctly stopped an external `System.ComponentModel.EditorBrowsableState` type resolution.

The failure is not evidence against the 11-instruction normalized cctor. It is a deterministic implementation regression introduced when the 0.0.95 normalizer used `ReadingMode.Immediate` even though the established Step-24/27 metadata-only auditors use `ReadingMode.Deferred`. Cecil Immediate mode eagerly reads custom-attribute constructor arguments; Deferred mode keeps those unrelated blobs opaque, and Cecil's writer completes a deferred module with attribute resolution disabled. The raw report is preserved at `docs/history/reports/STEP-27.0.13-PHYSICAL-GATE-A-REPORT.txt`.


### 0.0.98 (98) — production core compiles; real-Harmony CI regression test does not compile

Codemagic 0.0.98 compiled `StS2Launcher.Core` successfully, including the Deferred-Cecil production normalizer, but stopped while compiling `StS2Launcher.Core.Tests`. The newly added real-Harmony regression imported both `System.Reflection` and `Mono.Cecil` and used the bare interface name `ICustomAttributeProvider`, which is defined by both namespaces. Roslyn reported CS0104 at the helper declaration and follow-on CS1503 errors at its call sites. The real Harmony 2.4.2 normalizer regression never executed, no IPA was published, and this provides no new physical runtime evidence. The raw Codemagic report is preserved at `docs/history/reports/STEP-27.0.14-CODEMAGIC-TEST-COMPILE-FAILURE.txt`.

### 0.0.99 (99) — production and test source compile; real-Harmony fixture acquisition fails before test execution

Codemagic 0.0.99 advanced past the 0.0.98 `ICustomAttributeProvider` namespace collision. `StS2Launcher.Core` compiled, `StS2Launcher.Core.Tests` compiled, and the test assembly was emitted. The build then stopped in the test project's `CopyStep27RealHarmonyNormalizerFixture` MSBuild target with: `Step 27.0.14 requires exact merged Lib.Harmony 2.4.2 netstandard2.0/0Harmony.dll as a quarantined host-test fixture.` The target had hard-coded `$(NuGetPackageRoot)lib.harmony/2.4.2/lib/netstandard2.0/0Harmony.dll`; that path assumption does not hold for the restored package. No MSTest case executed, no IPA was published, and there is still no new physical runtime evidence. The raw report is preserved at `docs/history/reports/STEP-27.0.15-CODEMAGIC-REAL-HARMONY-FIXTURE-ACQUISITION-FAILURE.txt`.

### 0.0.100 (100) — official Harmony-Fat download succeeds; archive-root member assumption fails before build/test

Codemagic 0.0.100 successfully ran static validation (738/738) and downloaded the exact tagged `Harmony-Fat.2.4.2.0.zip`. The canonical host-test script then required an archive member equal to `netstandard2.0/0Harmony.dll` and found zero, so it exited before any `dotnet build`, MSTest execution, IPA publish, or device runtime. Official Harmony-Fat distributions wrap framework folders under a release-root directory, so the failure is a fixture-selector path-shape bug rather than a normalizer or production-code failure. The raw host report is preserved at `docs/history/reports/STEP-27.0.16-CODEMAGIC-HARMONY-FAT-ARCHIVE-MEMBER-FAILURE.txt`.

### 0.0.101 (101) — official fat archive inspected; no netstandard2.0 runtime implementation exists

Codemagic 0.0.101 passed static validation (741/741), downloaded the exact official `Harmony-Fat.2.4.2.0.zip`, and used the new drift diagnostic to print every `0Harmony.dll` member. The archive contains `netcoreapp3.0`, `netcoreapp3.1`, `net5.0` through `net10.0`, and .NET Framework implementations, but no `netstandard2.0` implementation. The script therefore stopped before any `dotnet build`, MSTest execution, IPA publish, or device runtime. This proves the remaining failure was the fixture model itself, not the Deferred production normalizer. The raw report is preserved at `docs/history/reports/STEP-27.0.17-CODEMAGIC-HARMONY-FAT-NETSTANDARD-ABSENCE.txt`.

### 0.0.102 (102) — real Harmony surrogate runs; 211/212 host tests pass

Codemagic 0.0.102 finally acquired the exact official `Harmony-Fat.2.4.2.0.zip`, selected `net9.0/0Harmony.dll`, compiled the production core and test assembly, and executed all **212** host tests. **211 passed and exactly one failed**: `OfficialHarmony242Net9FatNormalizerUsesDeferredMetadataAndPreservesSourceBytes`.

The failure occurred before the production normalizer invocation. The test incorrectly asserted that a net9 implementation must not contain any `netstandard` AssemblyRef. The official net9 Harmony implementation legitimately retains a netstandard compatibility reference, so that negative reference-graph assertion is not a valid target-framework proof. This is test-harness evidence only; it is not negative evidence about Deferred Cecil normalization, the 11-instruction cctor image, iOS T5/T6, or `PatchProcessor.Patch()`. The raw host report is preserved at `docs/history/reports/STEP-27.0.18-CODEMAGIC-NET9-SURROGATE-REFERENCE-ASSERTION-FAILURE.txt`.

### 0.0.103 (103) — 211/212 again; merged AssemblyRef uniqueness assumption fails before normalizer

Codemagic 0.0.103 compiled production and tests and again executed all **212** host tests. **211 passed / 1 failed**. The same real-Harmony regression now failed at its positive `System.Runtime` check with `System.InvalidOperationException: Sequence contains more than one matching element`. The official dependency-merged Harmony-Fat net9 binary contains multiple `System.Runtime` AssemblyRef rows, so `SingleOrDefault` is not a valid provenance or target-framework check.

This failure also occurred before `CreateIosNormalizedHarmonyRuntimeImage` was invoked. Codemagic recorded stable content identities for the exact official inputs: archive SHA-256 `a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774`, selected member `net9.0/0Harmony.dll`, and extracted DLL SHA-256 `a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab`. Those hashes are stronger and more relevant provenance evidence than merged AssemblyRef topology. The raw report is preserved at `docs/history/reports/STEP-27.0.19-CODEMAGIC-DUPLICATE-SYSTEM-RUNTIME-ASSEMBLYREF-FAILURE.txt`.

## Active candidate — Step 27.0.20 / 0.0.104 (104)

- Workflow: **`ios-step-27`**
- IPA: **`artifacts/StS2-Launcher-Step-27.ipa`**
- Main device report: `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`
- Crash checkpoint: `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt`
- Closed prerequisite: Step 26.0 + OfflineReady + Foundation 5/5
- StS2 reflection/patching/invocation: **forbidden**

### Gate A — unchanged production normalizer; hash-pinned official host input

Step 27.0.20 keeps production `ControlledHarmonyPatchExecution.cs` byte-for-byte unchanged. The production normalizer is still restricted to exact `0Harmony, Version=2.4.2.0`, the exact prepared StS2 patch-engine metadata fingerprint, Deferred Cecil reads, the rejecting metadata resolver, and the exact 11-instruction direct-state `HarmonySharedState::.cctor` rewrite. Internal randomized synthetic fixtures retain byte-identical passthrough.

The host regression no longer infers target framework or provenance from merged AssemblyRef rows. The canonical host script downloads the exact tagged Harmony-Fat 2.4.2 release, requires exactly one root-or-wrapped `net9.0/0Harmony.dll`, and now fail-closes on exact content hashes observed by Codemagic 0.0.103:

- archive SHA-256: `a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774`
- selected DLL SHA-256: `a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab`

The C# test independently re-hashes the selected DLL, verifies exact 0Harmony 2.4.2 identity and the `EditorBrowsableAttribute` surface without reading attribute arguments, and then immediately invokes `CreateIosNormalizedHarmonyRuntimeImage`. It deliberately makes no uniqueness/version/absence assertion about `System.Runtime` or `netstandard` AssemblyRef rows. Physical StS2 metadata remains the production authority.

### Gates B–S

Unchanged from 0.0.103. Gate B loads the retained normalized bytes only for the exact admitted production identity. Gate O remains the physically passing runtime-reflection surface plus metadata-only patch-engine audit. Gate R initializes the measured AccessTools surface. Gate S uses the bounded annotation-free `HarmonyMethod()` descriptor and never invokes `AddPrefix(MethodInfo)`.

### Gate T — normalized HarmonySharedState compatibility boundary

Unchanged from 0.0.103:

- **T1/T2** — bounded Reflection.Emit/RuntimeMethodHandle host-preservation preflight.
- **T3/T4** — exact runtime reflection of the normalized `HarmonySharedState` type/cctor/state fields without initialization.
- **T5a** — re-hash the retained normalized runtime image and require zero pre-existing generated patch-engine assemblies.
- **T5b** — execute exactly one `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)` against the direct-state cctor.
- **T6** — require the three dictionaries non-null, `methodAddressRef == null`, `actualVersion == 102`, no generated shared-state/proxy assemblies, unchanged prepared bytes, and bounded isolation.
- **T7/T8/T9** — only after T6, invoke exactly one public `PatchProcessor.Patch()` and validate replacement/isolation. The launcher target remains uninvoked until Gate V.

### Detour stop rule

`MtouchInterpreter=-all` is still sufficient to justify the normalized managed-code experiment but is not proof that MonoMod's native detour backend can modify iOS executable code. If 0.0.104 reaches T6 but fails at T7/T8, the next candidate performs one post-publish interpreted fixture patch/unpatch experiment on a launcher-owned target. If that representative interpreted target also cannot be patched, stop iterating Harmony internals and propose ahead-of-load Cecil transformation; that would require a master-plan update.

### Gates U–Z

Unchanged. U audits before patched execution; V proves patched launcher behavior; W removes exactly the prefix; X audits; Y proves restoration; Z performs final byte/OfflineReady/context/native isolation. No StS2 member is touched.

## Fresh-process rule

**Force-quit/relaunch before every Step-27 retry once any previous attempt reached Gate B.** Gate A intentionally rejects a process where `sts2` or Harmony remains loaded. If Gate T or later ran, also assume launcher/shared patch-engine state may remain process-resident.

If the app hard-crashes, copy `Step27-CrashCheckpoint.txt` before starting another run. Candidate 0.0.104 checkpoints include installed app version/build, expected source version/build, active candidate identity, the exact Gate-S implementation marker, and the Gate-T deferred-normalized-cctor implementation marker.

## Acceptance

Codemagic must compile and run the full host suite, including the official-fat-release real-Harmony-2.4.2 normalizer regression, before iOS publish/IPA verification. Then install `0.0.104 (104)` and from a fresh process run A–Z to **26/26 PASS**, followed by OfflineReady PASS and Foundation 5/5 PASS. T6 is the first proof that the normalized cctor fix actually ran on-device; T7/T8 is the first proof point for the public runtime detour boundary. If Step 27 closes, Step 28 is the first targeted StS2 member-reflection boundary.

