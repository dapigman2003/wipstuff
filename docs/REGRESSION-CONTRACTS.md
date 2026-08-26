# Regression Contracts

This document defines how historical physical proofs are translated into **current** regression expectations.

## Rule: validate the canonical present state, preserve the historical state in history

A compatibility step may intentionally change a runtime characteristic that an earlier step observed. When that happens:

- the historical step document remains unchanged as evidence of what was proven at the time;
- the current regression must test the capability that still matters, not an incidental intermediate value;
- any intentionally changed assertion must be documented in `CURRENT-STATUS.md` and a history record;
- static/unit tests should encode the new contract so a stale historical assertion cannot silently return.

This rule prevents a later improvement from making an earlier regression permanently red even though the underlying capability remains healthy.

## Step 19 — expression runtime compatibility

Historical Step 19.2 ran before the dynamic-managed-execution foundation and physically observed:

- `RuntimeFeature.IsDynamicCodeSupported=false`;
- `RuntimeFeature.IsDynamicCodeCompiled=false`;
- `Compile()`, `Compile(false)`, and `Compile(true)` all executed successfully.

Step 20 intentionally enabled the Mono interpreter with `MtouchInterpreter=-all`. The canonical Step-20+ runtime may therefore report dynamic-code **support** while still performing no dynamic native-code/JIT compilation.

Current Step 19 regression contract:

- all three expression compile call shapes execute successfully and return the expected value;
- on iOS, `RuntimeFeature.IsDynamicCodeCompiled` must be `false`;
- `RuntimeFeature.IsDynamicCodeSupported` is diagnostic;
- `supported=false, compiled=false` is accepted as the historical no-dynamic-code fallback mode;
- `supported=true, compiled=false` is accepted as the canonical interpreter-enabled mode;
- any iOS state with `compiled=true` fails the regression.

The current production policy is centralized in `ExpressionRuntimeCompatibilityPolicy` and covered by host unit tests.

## Step 20 — dynamic managed execution

The canonical build keeps `MtouchInterpreter=-all`: build-time assemblies remain AOT-targeted while interpreter capability remains available for post-publish managed IL and dynamic-code scenarios.

Step 20 regression remains authoritative for actual post-publish IL execution and private dependency resolution.

## Step 22 — host framework binding closure

The current contract remains:

- the measured 22 direct framework identities remain the authoritative binding frontier; under the current copy/no-link host policy all published framework members are preserved rather than relying on those roots for member survival;
- Step 22 A–D pass;
- explicit binding blockers are zero;
- runtime closure readiness is YES;
- no real StS2 CLR load occurs during the Step 22 foundation.

The wider 44-name host probe is diagnostic; transitive-only desktop implementation names are not independent private-runtime requirements.

## Step 24 — controlled `0Harmony` automatic initialization

Step 24.0.6 is physically closed. Its current regression contract is capability-level rather than candidate-history-specific:

- exact sole initializer-bearing prepared target remains `0Harmony 2.4.2.0`;
- the exact physically measured automatic-initializer/conditional MonoMod logger policy remains fail-closed;
- the dedicated private context may admit exact `0Harmony` only after the initializer-free Step-23 closure has been reproduced;
- `RuntimeHelpers.RunModuleConstructor` completes;
- the physically proven `System.Collections.Concurrent` preservation evidence remains recorded; the active host policy is `MtouchLink=None` + `TrimMode=copy`, while `MtouchInterpreter=-all` remains unchanged;
- native resolution and rejected/unplanned managed resolution remain zero;
- final private-context membership equals the Step-23 initializer-free closure plus exactly `0Harmony`;
- plan/prepared/live bytes and OfflineReady remain exact;
- Harmony patch/processor APIs, StS2 game invocation/reflection, Godot/game startup, and native game loading are not part of the Step-24 regression.

Later steps may use the loaded/initialized Harmony assembly, but they must replay or otherwise protect this capability before advancing a new boundary.

## Changing a contract later

Before changing one of these contracts:

1. identify the later step that intentionally changed the runtime behavior;
2. preserve the older physical evidence under `docs/history/steps/`;
3. define the capability-level invariant that remains important;
4. add or update a pure unit-testable policy where possible;
5. update static validation to reject the obsolete assertion;
6. require a new physical regression pass before advancing the project boundary.

## Step 27 — controlled launcher-owned Harmony patch decision

Step 27 is physically closed as a **negative architecture result** by 0.0.108. The current regression contract preserves the evidence needed to prevent the retired runtime-patch path from being accidentally reinterpreted as viable:

- the copy/no-link host policy (`MtouchLink=None`, `TrimMode=copy`) and `MtouchInterpreter=-all` remain the dynamic-payload execution baseline established after the physical 0.0.105/106 trimming failures;
- the final Step-27 target/prefix remain the launcher-owned `StS2Launcher.Step27.InterpretedPatchFixture.dll`, absent from iOS/test ProjectReference graphs and copied into the app only after publish;
- Gate P binds a fresh processor to the exact interpreted `Target` through public `Harmony.CreateProcessor(MethodBase)`; Gate Q proves both Target reflection and the in-fixture direct managed IL call through `InvokeTarget` execute the unpatched post-publish image;
- physical 0.0.108 must remain preserved as the decisive result: exact public `PatchProcessor.Patch()` fails at Gate T with `System.NotImplementedException: Arg_NotImplementedException` surfaced from `HarmonyLib.PatchFunctions.UpdateWrapper`;
- the failure is architecture-decisive because it occurs against the representative post-publish interpreted target after the prior trimming ambiguity was removed; identifying the deeper unsupported MonoMod primitive is not a prerequisite for the product architecture;
- runtime Harmony/MonoMod method replacement is therefore retired from the active compatibility path. No new candidate may modify Harmony internals or force an alternate MonoMod detour backend merely to continue Step 27;
- the old Step-27 implementation, tests, preservation anchors, fixture, and physical reports remain regression/evidence assets even though active release/version/CI wiring advances.

## Step 28 — ahead-of-load managed transformation

Step 28 replaces runtime detouring with a deterministic transform-before-load contract. Step 28.0 proves the architecture on a project-owned post-publish fixture before any real StS2 behavior is changed.

Current Step-28.0 regression contract:

- OfflineReady is required and re-verified after transformed execution; the trusted Step-12 managed install remains immutable;
- `StS2Launcher.Step28.AheadOfLoadFixture.dll` is built outside the iOS project/AOT graph and copied into the `.app` only after `dotnet publish`, with an exact SHA-256 manifest;
- Gate A reads the fixture only as Cecil metadata, verifies its exact source IL, and clones verified bytes into launcher-private scratch storage; the fixture assembly identity must not already be CLR-loaded;
- Gate B changes exactly one audited semantic point in a new private image: `Adjustment()` constant `1` becomes `1000`; bundle/source hashes remain unchanged and no CLR load occurs during transformation;
- Gate C reopens source and transformed images and requires source `Adjustment()==1`, transformed `Adjustment()==1000`, and preserved direct calls `Target -> Adjustment` and `InvokeTarget -> Target`;
- Gate D loads only the verified transformed bytes into a dedicated private `AssemblyLoadContext` and requires `Adjustment()==1000`, `Target(41)==1041`, and `InvokeTarget(41)==1041`; this proves an ordinary in-fixture direct managed IL call observes the transformed image;
- the private context may delegate framework contracts to the host but must fail closed on unexpected non-framework dependency fallback;
- Gate E re-hashes bundle/source/transformed images, re-proves OfflineReady, and requires exactly one Step-28 fixture identity resident in the dedicated private context;
- Harmony/MonoMod runtime patching, real StS2 member reflection/transformation/invocation, Godot/game startup, and native game loading remain outside Step 28.0;
- after Gate D is reached, a fresh process is required before another Step-28 run because the private context is intentionally non-collectible for physical evidence accounting.

Physical 0.0.111 closed this combined rewrite-before-load + interpreted-execution boundary at **5/5 PASS**. That result is now protected evidence and authorizes a later candidate to select one narrowly audited real StS2 compatibility transformation.

### Step 28.0.1 compile-integrity contract

Codemagic 0.0.109 is preserved as compile-only evidence: static validation passed 845/845 and all external fixtures built, but `StS2Launcher.Core` stopped before MSTest with CS0246 because `AheadOfLoadManagedTransformation` referenced an undeclared `CallbackProgress<T>`. It establishes no host, IPA, or physical runtime result.

Step 28.0.1 / 0.0.110 must retain a private callback-backed `IProgress<T>` adapter in `AheadOfLoadManagedTransformation`: the constructor rejects a null callback and `Report(T)` forwards synchronously to that callback. This helper exists only to bridge established OfflineReady progress into the Step-28 progress surface. It must not alter Gate A admission semantics or any Gate B–E transform/execution contract. Static validation pins the declaration and forwarding behavior so this exact compile defect cannot silently recur.

### Step 28.0.2 metadata-only Cecil admission contract

Codemagic 0.0.110 compiled and ran all 217 host tests; 216 passed. The sole Step-28 failure occurred at Gate A before rewrite/load because `ReadingMode.Immediate` eagerly decoded unrelated custom-attribute arguments and requested `System.Runtime, Version=9.0.0.0` through the deliberately rejecting Cecil resolver. This is preserved as implementation evidence, not transformed-execution evidence.

Step 28.0.2 / 0.0.111 must read the Step-28 fixture with `ReadingMode.Deferred` while retaining the rejecting resolver. The boundary must never broaden Cecil dependency search paths merely to satisfy metadata not required by the experiment. The exact assembly/type/method/body/direct-call/PInvoke checks remain required, and any actual assembly-resolution attempt remains fail-closed. Static validation must reject a return to `ReadingMode.Immediate` in `AheadOfLoadManagedTransformation`.


### Step 28.0.2 physical-closure contract

The raw physical 0.0.111 report is preserved at `docs/history/reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt`. Regression validation must retain the decisive values: A–E **5/5 PASS**, `Adjustment()==1000`, `Target(41)==1041`, `InvokeTarget(41)==1041`, exactly one transformed fixture identity CLR-loaded, original source bytes never CLR-loaded, and post-execution OfflineReady **428/428**.

The Step-28 implementation is now a protected positive runtime boundary. Active candidates may reuse the architecture, but must not silently weaken source immutability, transformed-image verification, transformed-only admission, fail-closed private dependency policy, or final OfflineReady/isolation proof.

## Step 29 — real StS2 compatibility target audit

Step 29.0 is physically closed at **4/4 PASS** on `0.0.112 (112)`. Preserve the raw report `docs/history/reports/STEP-29.0-PHYSICAL-CLOSURE.txt` and the exact selected fingerprint: source SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`, `ModManager.TryLoadMod(Mod)` token `0x06007927`, `IL_0D9D Callvirt -> Harmony.PatchAll(Assembly)`, body SHA-256 `50c8c4394082f3c73df414fad8675540cfc00a99ccc4f350b616cec574cdbcbd`. Step 29 performed zero writes, zero CLR load/invocation, zero resolver requests, and retained OfflineReady 428/428. Selection remains **audit evidence only**.

## Step 30 — selected Harmony target semantic context audit

Physical 0.0.113 closed Step 30 at **4/4 PASS**. Preserve `docs/history/reports/STEP-30.0-PHYSICAL-CLOSURE.txt`. The closed Step-30 contract is read-only:

- Gate A binds the exact physical Step-29 source/token/offset/target/body fingerprint to the same receipt-backed ARM64 `sts2.dll` under OfflineReady and fresh-process conditions;
- Cecil remains `ReadingMode.Deferred` with an explicit rejecting resolver and zero resolution requests;
- Gate B inspects only the exact selected method context and records a bounded IL window, branch sources, covering exception regions, nearby strings, method-body shape, Harmony-call count and dynamic-load-call count;
- no `ModuleDefinition.Write`, `Assembly.Load`, private `AssemblyLoadContext` admission, StS2 reflection/invocation, Harmony/MonoMod patching, Godot/game startup or native game loading is permitted;
- Gate C may defer the selected site only when it remains structurally `MegaCrit.Sts2.Core.Modding.ModManager::TryLoadMod(Mod) -> Harmony.PatchAll(Assembly)`; the required disposition is **`DEFER — MOD/HARMONY COMPATIBILITY PATH; NO BASE-GAME REWRITE AUTHORIZED`**;
- Step 30 makes no runtime-reachability claim and predeclares **no behavior change** for the selected PatchAll site;
- Gate D re-hashes source, re-proves OfflineReady, and reasserts zero CLR load/write/resolver activity.

Physical **4/4 PASS** closes this semantic-context/product-scope audit only. The selected Harmony/mod site is formally deferred from the base-game frontier and remains unauthorized for rewrite.


## Step 31 — PrepareMethod semantic context audit

Step 31.0 is the next read-only evidence boundary for the first non-mod Step-29 family. Its regression contract is:

- preserve the exact receipt-backed `sts2.dll` source SHA-1 `e424ace9399a82edea4dd7e0fa5761635dfd6c5d`, SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, byte count `9,363,456`, and MVID `518e4758-52d7-47c2-b776-471a0e29e49d`;
- bind exactly `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()` token `0x06007D05`, body SHA-256 `7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9`;
- bind exactly ten `RuntimeHelpers.PrepareMethod` sites at `IL_003D`, `IL_0052`, `IL_007A`, `IL_00A2`, `IL_00CA`, `IL_00F2`, `IL_0136`, `IL_014C`, `IL_0162`, and `IL_0178`, with the one-argument/two-argument signatures physically recorded by Step 29;
- retain `ReadingMode.Deferred` plus an explicitly rejecting Cecil resolver and require zero resolver requests;
- Gate B records bounded per-site IL, incoming branch, and covering-exception context without resolving dependencies;
- no `ModuleDefinition.Write`, CLR admission/invocation of `sts2`, Harmony/MonoMod runtime patching, Godot/game startup, or native game loading is permitted;
- Gate C may record `BASE-GAME COMPATIBILITY FAMILY CONFIRMED — ELIGIBLE FOR EXPLICIT REWRITE DESIGN; NO WRITE AUTHORIZED` only when the exact method/body/site set remains intact;
- Step 31 makes no runtime-reachability claim and predeclares **no behavior change**; rewrite-design eligibility is not rewrite authorization;
- Gate D re-hashes the source and re-proves OfflineReady/isolation.

Physical **4/4 PASS** is required before a following candidate may design one exact ahead-of-load semantic transformation for this fingerprinted family.

## Step 31 physical closure — PrepareMethod semantic context audit

Physical 0.0.114 closed Step 31 **4/4 PASS**; Step 31 is now CLOSED POSITIVE 4/4. The exact source SHA/MVID, `PrewarmJit()` token `0x06007D05`, body fingerprint `7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9`, and all ten exact PrepareMethod offsets/signatures are now protected physical evidence. The closure preserved zero writes/zero CLR load and recorded `BASE-GAME COMPATIBILITY FAMILY CONFIRMED — ELIGIBLE FOR EXPLICIT REWRITE DESIGN; NO WRITE AUTHORIZED`.

## Step 32 — first real StS2 PrepareMethod rewrite

Step 32.0 / 0.0.115 is the first real-game semantic transformation under the physically proven Step-28 transform-before-load architecture. Its candidate regression contract is:

- hard-pin the physical Step-31 source identity, method token/body fingerprint, and ten exact PrepareMethod sites;
- require zero incoming branches to each selected call before writing;
- clone the exact source to launcher-private storage and never mutate the receipt-backed Step-12 install;
- use `ReadingMode.Deferred`; source-admission and reopened-verification resolvers remain fully rejecting with zero requests; serialization may use only the separately pinned Step-32.0.2 constant-metadata surrogate policy;
- replace exactly six `PrepareMethod(RuntimeMethodHandle)` calls with one `Pop`;
- replace exactly four `PrepareMethod(RuntimeMethodHandle, RuntimeTypeHandle[])` calls with `Pop + Pop`;
- preserve preceding reflection/GetMethod/get_MethodHandle/generic-instantiation-array construction; no launcher helper dependency is added;
- reopen source and transformed images, require source/transformed PrepareMethod counts 10/0, unchanged source body fingerprint, preserved assembly identity/MVID and exception-handler count, and exact transformed semantic-fingerprint match;
- perform no real-StS2 CLR load/invocation, Harmony/MonoMod runtime patching, Godot/game startup, or native loading;
- re-prove OfflineReady and all source/transformed hashes at Gate D;
- a Step-32 PASS authorizes only a separately gated transformed-real-StS2 CLR admission/execution boundary.

## Step 32.0.1 — serialized-fingerprint verification correction

Codemagic 0.0.115 is preserved as host-test evidence: static validation passed **996/996**, compilation succeeded, and the complete host suite reached **230/231 PASS**. The sole failure was `ExactPrewarmJitPrepareMethodFamilyIsRewrittenOnPrivateCopyOnly` at Gate C after the private transformed image had already been written. It provides no device evidence and does not invalidate the predeclared 6+4 rewrite.

0.0.115 incorrectly treated the offset-bearing `ComputeMethodBodyFingerprint` result calculated before `ModuleDefinition.Write` as a serialization-stable expected value. Step 32.0.1 / 0.0.116 must instead:

- retain the exact six one-argument + four two-argument Pop rewrite unchanged;
- retain `ComputeMethodSemanticFingerprint` as the exact pre-write→reopen invariant because it addresses instructions by ordinal and exception-handler boundaries by instruction index rather than physical byte offset;
- never store or compare an `ExpectedTransformedBodySha256` derived from pre-write `Instruction.Offset` values;
- compute the transformed method-body SHA-256 only after reopening the serialized image and record it as post-write physical IL evidence;
- require that reopened transformed body fingerprint to differ from the unchanged source body fingerprint;
- retain all existing 10/0 PrepareMethod, instruction-count, Pop-count, exception-handler, identity/MVID, rejecting-resolver, trusted-source immutability, and no-CLR-admission checks.

Static validation pins this distinction so a pre-serialization offset-sensitive body hash cannot silently return as a Gate-C acceptance condition.
## Step 32.0.2 — bounded Cecil write-time constant-metadata resolver

Physical 0.0.116 is preserved as **device evidence at 1/4**: Gate A passed with OfflineReady 428/428, exact source identity, all ten PrepareMethod sites, zero Cecil read-time resolution, zero CLR admission, and unchanged trusted bytes. Gate B then failed inside `Mono.Cecil.MetadataBuilder.GetConstantType` during `ModuleDefinition.Write` when Cecil requested exact `System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a` to determine an unrelated field constant's serialized element type.

Step 32.0.2 / 0.0.117 uses a **write-only in-memory** constant-metadata surrogate and must:

- keep the exact 6 + 4 PrepareMethod→Pop rewrite unchanged;
- keep Gate-A and Gate-C Cecil readers on `ReadingMode.Deferred` with fully rejecting resolvers and zero dependency requests;
- forbid `DefaultAssemblyResolver`, resolver search directories, filesystem framework probing, and CLR assembly loading in the Step-32 transformation;
- before `module.Write`, enumerate constant-bearing fields/properties/parameters without resolution and derive primitive Constant-table storage types only from values already decoded from the verified source metadata;
- permit the write resolver to satisfy **only** exact `System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a`; every other assembly identity is blocking;
- synthesize only the required enum metadata in memory and open zero external framework/game assembly bytes;
- require at least one physical/write-test request through that bounded surrogate so the regression actually exercises the 0.0.116 failure path;
- fingerprint all Constant-table providers before write and require the reopened transformed image to match the source fingerprint exactly;
- preserve the Step-32.0.1 offset-independent method semantic fingerprint as the pre-write→reopen IL invariant and the reopened body hash as post-write evidence;
- continue to forbid real-StS2 CLR admission/invocation, Harmony/MonoMod runtime patching, Godot/game startup, native loading, or trusted-install mutation.

A Step-32.0.2 PASS still authorizes only the next separately gated transformed-real-StS2 admission/execution experiment.
