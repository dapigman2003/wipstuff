# Current Status — Step 28 Ahead-of-Load Managed Transformation

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

### 0.0.104 (104) — real production normalizer reached; Cecil whole-module writer fails on enum constant resolution

Codemagic 0.0.104 compiled production and tests and executed all **212** host tests. **211 passed / 1 failed**. This is the first run where the exact hash-pinned official Harmony-Fat 2.4.2 net9.0 surrogate actually entered `CreateIosNormalizedHarmonyRuntimeImage`.

The Deferred Cecil source read succeeded. The failure occurred later at `Mono.Cecil.ModuleDefinition.Write` while Cecil rebuilt metadata. Its `MetadataBuilder.GetConstantType` path attempted `TypeReference.Resolve()` for the enum-typed constant `System.Reflection.BindingFlags`, and the Step-27 fail-closed metadata resolver rejected that external resolution. This is a genuine normalizer-design failure, not another surrogate acquisition/provenance assertion. The raw evidence is preserved at `docs/history/reports/STEP-27.0.20-CODEMAGIC-CECIL-WRITER-ENUM-CONSTANT-FAILURE.txt`.

The correct conclusion is broader than “allow BindingFlags”: Cecil 0.11.6 whole-module serialization can require external enum definitions for unrelated Constant rows. Whitelisting individual framework enums would create another moving target and would weaken Gate A's resolution-free boundary.

### 0.0.105 (105) — raw HarmonySharedState normalization physically succeeds; T7 exposes trimmed System.Linq member

Physical Step 27.0.21 reached Gate T after the raw-PE normalized `HarmonySharedState::.cctor` completed successfully. Gate A reported a byte-distinct runtime image with direct `state` / `originals` / `originalsMono`, `actualVersion=102`, null `methodAddressRef`, no generated shared-state/FieldRef path, and no prepared/source/live mutation.

The first public `PatchProcessor.Patch()` call then threw `MissingMethodException` for `System.Linq.Enumerable.Union<T>`. The stack stopped in `HarmonyLib.MethodCreator..ctor -> PatchFunctions.UpdateWrapper -> PatchProcessor.Patch`, before `PatchTools.DetourMethod`. This established the first post-publish BCL member-trimming failure and did not satisfy the Harmony detour stop rule. The full report is preserved at `docs/history/reports/STEP-27.0.21-PHYSICAL-T7-SYSTEM-LINQ-TRIM-FAILURE.txt`.

### 0.0.106 (106) — System.Linq preservation works; DynamicMethodDefinition exposes second trim failure

Physical Step 27.0.22 again reached **19/26 / Gate T**. The exact System.Linq preservation change did its job: the previous `Enumerable.Union<T>` failure disappeared and Harmony advanced into `HarmonyLib.MethodPatcherTools.CreateDynamicMethod`.

`MonoMod.Utils.DynamicMethodDefinition` then failed type initialization with:

`System.TypeLoadException: Could not resolve ... System.Diagnostics.DebuggableAttribute ... System.Runtime, Version=9.0.0.0`

The stack is `MethodPatcherTools.CreateDynamicMethod -> MethodCreatorConfig.Prepare -> MethodCreator..ctor -> PatchFunctions.UpdateWrapper -> PatchProcessor.Patch`. `PatchTools.DetourMethod -> DetourFactory.Current.CreateDetour` was still not reached. This is the second independent ordinary-framework trimming failure caused by the real Harmony/MonoMod payload arriving only after publish. The full report is preserved at `docs/history/reports/STEP-27.0.22-PHYSICAL-DYNAMICMETHODDEFINITION-DEBUGGABLEATTRIBUTE-TRIM-FAILURE.txt`.

Two successive failures now establish that adding one framework root/member at a time is not an acceptable dynamic-plugin preservation architecture.

## Physical 0.0.107 result — trimming ambiguity removed; Patch() now reaches unsupported runtime behavior

Physical `0.0.107 (107)` preserved the normalized HarmonySharedState proof and removed the prior full-trim BCL failures under `MtouchLink=None + TrimMode=copy`. The exact public `PatchProcessor.Patch()` call then failed at Gate T with `System.NotImplementedException: Arg_NotImplementedException`, surfaced from `HarmonyLib.PatchFunctions.UpdateWrapper`. The complete report is preserved at `docs/history/reports/STEP-27.0.23-PHYSICAL-NOTIMPLEMENTED-PATCHENGINE.txt`.

This is materially different from 0.0.105/106: no missing LINQ or `DebuggableAttribute` member appears. However, `UpdateWrapper` encompasses both replacement generation and later detour installation, so this stack alone does not prove which substage is unsupported.

## Physical 0.0.108 result — Step 27 closed negative; runtime Harmony replacement retired

Physical `0.0.108 (108)` executed the pre-declared final Step-27 discriminator and reached **19/26**, first failure **Gate T / PatchEngineExecution**. This time the target was not a launcher AOT method: Gate P admitted `StS2Launcher.Step27.InterpretedPatchFixture.dll`, which was copied into the `.app` only after `dotnet publish` and was not an iOS project/content/AOT input. Gate Q then proved both reflection invocation of `Target(41)` and the fixture's own direct managed IL call through `InvokeTarget(41)` returned the original value `42` before patching.

Gate S registered the exact annotation-free prefix through the bounded `HarmonyMethod()` descriptor path. Gate T invoked the exact public `PatchProcessor.Patch()` boundary against a fresh processor whose `original` was that post-publish interpreted `Target` MethodInfo. The call threw `System.NotImplementedException: Arg_NotImplementedException` from `HarmonyLib.PatchFunctions.UpdateWrapper`.

This removes the remaining AOT-target ambiguity. Per the Step-27 stop rule, **Step 27 is closed negative** and no further Harmony-internal workaround candidate follows. The exact unsupported lower-level primitive inside `UpdateWrapper` remains unspecified, but that distinction is no longer required for the architecture decision. Runtime Harmony/MonoMod replacement is retired from the active path.

Raw physical evidence: `docs/history/reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt`. Closure note: `docs/history/steps/STEP-27.0.24-PHYSICAL-NEGATIVE-CLOSURE.md`.

## Codemagic 0.0.109 result — compile stop before host tests

Step 28.0 / `0.0.109 (109)` passed canonical static validation **845/845**. Codemagic then acquired the exact hash-pinned Harmony host fixture and successfully built every external managed fixture, including `StS2Launcher.Step28.AheadOfLoadFixture.dll`.

Compilation of `StS2Launcher.Core` stopped before MSTest at `src/StS2Launcher.Core/Compatibility/AheadOfLoadManagedTransformation.cs(88,23)` with:

`error CS0246: The type or namespace name 'CallbackProgress<>' could not be found`

The defect is implementation-local: Gate A already constructs a callback-backed `IProgress<SteamOfflineInstallProgress>` adapter to translate OfflineReady progress into Step-28 progress, but this class omitted the same private `CallbackProgress<T>` helper used by established compatibility/runtime boundaries. The remaining compiler diagnostics were pre-existing nullable/async warnings and were not the blocking Step-28 error.

No host-test verdict exists for 0.0.109. No iOS publish, IPA, or physical-device runtime evidence exists for 0.0.109. The raw Codemagic output is preserved at `docs/history/reports/STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt`.

## Codemagic 0.0.110 result — compile fixed; 216/217 host tests pass; Gate-A Cecil eager-read regression

Step 28.0.1 / `0.0.110 (110)` passed canonical static validation **850/850**, built every external managed fixture, compiled `StS2Launcher.Core` and the test project, and executed all **217** host tests. **216 passed**. This proves the 0.0.109 `CallbackProgress<T>` compiler defect is corrected.

The sole failure was `VerifiedSourceIsRewrittenBeforeLoadAndOnlyTransformedBehaviorExecutes` at Gate A. Before any rewrite or CLR admission, `ReadFixtureModule(...)` opened the separately built net9.0 fixture with Mono.Cecil `ReadingMode.Immediate` and the intentionally rejecting `RejectingAssemblyResolver`. Immediate mode eagerly decoded custom-attribute constructor arguments and attempted to resolve `System.Runtime, Version=9.0.0.0`, producing `Mono.Cecil.AssemblyResolutionException`.

This is a deterministic host implementation defect, not negative evidence about transform-before-load execution. Gate B was never reached, no transformed image was written, and no iOS publish/IPA/device verdict exists for 0.0.110. The raw Codemagic host report is preserved at `docs/history/reports/STEP-28.0.1-CODEMAGIC-HOST-TEST-FAILURE.txt`.

## Active candidate — Step 28.0.2 / 0.0.111 (111)

- Workflow: **`ios-step-28`**
- IPA: **`artifacts/StS2-Launcher-Step-28.ipa`**
- Main device report: `Documents/StS2Launcher/Reports/Step28-AheadOfLoadManagedTransformation.txt`
- Closed prerequisite: Steps 01–26 physical closure + Step-27 negative architecture decision + OfflineReady
- 0.0.109 evidence: **static 845/845 PASS; external fixtures built; Core compile CS0246; no host/iOS/device verdict**
- 0.0.110 evidence: **static 850/850 PASS; compile PASS; host 216/217; sole Gate-A `System.Runtime` Cecil eager-read failure; no iOS/device verdict**
- 0.0.111 correction: **Step-28 fixture Cecil reads use `ReadingMode.Deferred`; rejecting resolver retained; gates/fixture/runtime model unchanged**
- Harmony/MonoMod runtime patching: **not used by Step 28**
- Real StS2 member reflection/invocation: **forbidden in this mechanism-closing candidate**

### Step 28 architecture

The canonical compatibility pipeline remains:

1. verify immutable receipt-backed/source bytes;
2. clone required managed images into launcher-private storage;
3. apply deterministic, audited Mono.Cecil transformations **before CLR admission**;
4. reopen and verify transformed metadata/bytes;
5. load only the transformed image into the private runtime context;
6. execute through the already-proven post-publish Mono interpreter path;
7. preserve exact source/install hashes and fail closed on unexpected dependency/native behavior.

The Step-27 copy/no-link host policy remains active. `MtouchLink=None + TrimMode=copy` is still required because the real StS2/mod managed world arrives after publish and ILLink cannot see its complete framework usage. `MtouchInterpreter=-all` also remains unchanged so build-time launcher assemblies stay AOT-targeted while runtime-loaded managed IL can execute.

### 0.0.111 correction

`AheadOfLoadManagedTransformation.ReadFixtureModule(...)` now uses `ReadingMode.Deferred` instead of `ReadingMode.Immediate`, while keeping `RejectingAssemblyResolver.Instance`. This matches the established metadata-only audit pattern: Step 28 reads only the exact assembly/type/method/IL surface needed by Gates A–C and does not force unrelated custom-attribute argument resolution. If code later attempts dependency resolution, the same resolver still fails closed.

No OfflineReady contract, source admission identity/hash checks, rewrite semantics, transformed-image verification, private `AssemblyLoadContext`, framework delegation policy, physical gate order, or acceptance value changes.

Local canonical static validation for 0.0.111 is **859/859 PASS**. The current local environment still has no .NET SDK, so `scripts/test.sh` records only `ERROR: dotnet is required to run host tests.` Codemagic is therefore the next compile/full-host-test authority.

### Unchanged Step-28 physical question

Step 28 deliberately does **not** rewrite a real StS2 method yet. It first proves the new architecture end-to-end with a project-owned fixture built separately from the iOS project and copied into the app only after `dotnet publish`.

The source fixture exposes:

- `Adjustment() => 1`
- `Target(value) => value + Adjustment()`
- `InvokeTarget(value) => Target(value)` with a direct managed IL call to `Target`.

Ordered gates A–E:

- **Gate A — FixtureAdmissionAndOfflineReady:** re-prove OfflineReady; verify the post-publish source fixture SHA-256 and exact Cecil IL shape using deferred metadata-only reading; clone it to `Step28-AheadOfLoadTransformation/source`; require that the fixture identity has not entered the CLR.
- **Gate B — DeterministicRewrite:** use Mono.Cecil to change only `Adjustment()` from constant `1` to `1000` in a new `transformed` image; source/bundle hashes must remain unchanged; no Assembly.Load/Harmony/StS2 behavior.
- **Gate C — TransformedImageVerification:** reopen both images; require source `Adjustment()==1`, transformed `Adjustment()==1000`, and preserved direct-call topology; re-hash all bytes before load.
- **Gate D — TransformedExecution:** load **only** the transformed bytes into a dedicated private `AssemblyLoadContext`; require `Adjustment()==1000`, `Target(41)==1041`, and `InvokeTarget(41)==1041`.
- **Gate E — FinalIsolationAudit:** re-hash bundle/source/transformed images, re-prove OfflineReady, and require exactly one Step-28 fixture identity resident in the dedicated private context with no unexpected private dependency fallback.

## Fresh-process rule

Step 28 Gate D loads the fixture identity into a non-collectible private `AssemblyLoadContext`. After any run that reaches Gate D, **force-quit/relaunch before another Step-28 run** so Gate A can prove that the source identity was not previously resident.

## Acceptance / next authority

Run Codemagic workflow `ios-step-28`. It must prove canonical static validation, **217/217 host tests**, iOS publish, and IPA verification. The final `.app` must contain exactly one byte-identical `StS2Launcher.Step28.AheadOfLoadFixture.dll` under `Step28AheadOfLoadFixture/`, copied only after publish and accompanied by its SHA-256 manifest.

If Codemagic passes, install `0.0.111 (111)` from a fresh process and run Step 28 A–E. Physical closure requires **5/5 PASS**. Gate D must report **1000 / 1041 / 1041**, and Gate E must re-prove OfflineReady. No real StS2 member is reflected, rewritten, or invoked by this candidate.
