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

## Step 27 — retired runtime-Harmony architecture decision

Step 27 is physically closed as a **negative architecture result** by 0.0.108. The active regression contract now protects the **decision and evidence**, not the ability to rerun the retired experiment in every future IPA:

- physical 0.0.108 remains the decisive result: exact public `PatchProcessor.Patch()` against the representative post-publish interpreted target fails at Gate T with `System.NotImplementedException: Arg_NotImplementedException` surfaced from `HarmonyLib.PatchFunctions.UpdateWrapper`;
- runtime Harmony/MonoMod method replacement remains retired from the active compatibility path; no candidate may resume Harmony-internal workaround iteration merely to continue Step 27;
- the copy/no-link host policy (`MtouchLink=None`, `TrimMode=copy`) and `MtouchInterpreter=-all` remain active because later architecture still depends on post-publish/dynamic managed execution;
- Step 24 controlled `0Harmony` initialization remains separately protected and active; retiring Steps 25–27 does not erase that proven dependency-initialization capability;
- from Step 32.0.3 onward, the old Step-25/26/27 implementation, dedicated tests/UI, DynamicDependency anchors, Harmony-Fat host-fixture acquisition, and interpreted patch fixture are historical-only and must be absent from the active compile/package graph;
- historical step documents, physical reports, and the inert pre-trim source snapshot preserve reconstructability without imposing recurring build/AOT cost.

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

## Step 32.0.3 — retired Harmony active-surface maintenance trim

Physical 0.0.117 is preserved as **device evidence at 1/4**: Gate A passed with the exact receipt-backed source and ten sites; Gate B failed closed before mutation because the verified module contains external constant metadata scoped to exact `Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0`.

0.0.118 is maintenance-only and must:

- leave `src/StS2Launcher.Core/Compatibility/RealStS2PrepareMethodRewrite.cs` byte-for-byte unchanged from 0.0.117;
- make no attempt to admit Sentry or broaden the Step-32 writer resolver;
- remove the retired Step-25/26/27 runtime-Harmony Core files, dedicated host tests, iOS UI partials, and preservation-anchor sources from the active tree;
- remove the Step-27 interpreted patch fixture from active fixtures and final IPA packaging;
- remove the Harmony-Fat network download and all Step-27 fixture environment variables from `scripts/test.sh`;
- retain Step 24 controlled initialization, Step 28 ahead-of-load transformation, and all current runtime build policies;
- keep historical evidence reconstructable through `docs/history` and the inert archive, without making active validation/runtime depend on that archive.

Passing 0.0.118 CI/IPA validation proves only that the project can carry its current architecture with a smaller active surface. It does **not** close Step 32 or supersede the Sentry physical finding.

## Step 32.0.4 — audited multi-scope constant-metadata resolver

The exact Step-32 `sts2.dll` static audit is preserved in `docs/history/reports/STEP-32-STATIC-STS2-CONSTANT-METADATA-AUDIT.txt`. Under the active non-null requirement scan, the source has exactly three external type/storage requirements: exact System.Runtime 9.0.0.0 / `System.Reflection.BindingFlags` / `Int32`; exact Sentry 5.0.0.0 / `Sentry.BreadcrumbLevel` / `Int32`; exact Sentry 5.0.0.0 / `Sentry.SentryLevel` / `Int16`. User-confirmed Codemagic success for 0.0.118 establishes the lean baseline before this correction.

0.0.119 must:

- keep the exact 6 + 4 PrepareMethod→Pop rewrite unchanged;
- require the observed distinct non-null external constant requirement set to equal those three audited entries exactly before the rewrite loop;
- reject any missing, changed-storage, nested, or additional external constant requirement before producing transformed output;
- require exactly one matching source AssemblyRef for each approved exact identity;
- synthesize only the three audited enum definitions, using per-exact-assembly in-memory surrogates for System.Runtime and Sentry;
- satisfy only write-time assembly-resolution requests whose full identity is one of those configured exact audited surrogate identities;
- record every write-time resolver request and reject all other identities;
- keep GodotSharp 4.5.1.0 and System.Collections 9.0.0.0 unauthorized despite their null-only Constant rows in the whole-table audit;
- open zero external framework/game assembly bytes from the write resolver and continue to forbid DefaultAssemblyResolver/search-directory fallback;
- retain the full source/transformed constant-metadata fingerprint equality check in Gate C;
- retain all Step-32 exact source/hash/MVID/token/site, branch-target, semantic fingerprint, Pop/instruction/EH, trusted-install immutability, no-CLR-admission, and OfflineReady invariants.

Host regression coverage must include both a representative exact three-requirement fixture and an unaudited-external-requirement fail-closed fixture. Physical Step-32 closure still requires A–D **4/4 PASS**; a pass does not itself authorize transformed-real-StS2 CLR admission/execution.

## Step 32.0.5 — stable transformed-method verification

Physical 0.0.119 proved Gate A and Gate B, advancing Step 32 to **2/4**. The private real-StS2 rewrite serialized successfully with the exact audited resolver contract, but Gate C failed before semantic verification at the transformed method identity/body check. The 0.0.119 verifier had reused source MethodDef token `0x06007D05` as a post-Cecil-write transformed locator.

0.0.120 must:

- keep source token `0x06007D05` authoritative for Gate-A/Gate-B source binding only;
- reopen the transformed method by exactly one matching declaring type `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization` plus full signature `System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()`;
- fail closed if that stable identity is missing, duplicated, or lacks a body;
- treat transformed MethodDef token preservation as diagnostic rather than semantic authority;
- report the transformed token, whether the original source token survived serialization, and the old-token occupant;
- retain the exact expected transformed semantic fingerprint check, zero PrepareMethod requirement, constant-metadata fingerprint equality, instruction/EH shape, exact Pop delta, assembly identity/MVID, source/transformed hashes, zero reopen resolution, and zero CLR admission;
- make no change to the 6 + 4 rewrite or the exact audited System.Runtime/Sentry resolver authority.

Host coverage must explicitly protect the stable-identity lookup against historical-token assumptions. Physical closure remains A–D **4/4 PASS**; 2/4 is progress, not closure.

## Step 32.0.5 physical closure

Physical 0.0.120 is **CLOSED POSITIVE — 4/4**. Preserve these exact closure facts:

- source SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, 9,363,456 bytes, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`;
- transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, 9,304,576 bytes;
- transformed PrewarmJit semantic fingerprint `47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a` and zero PrepareMethod references;
- transformed MethodDef token `0x0600AFEA` is diagnostic only; the source token `0x06007D05` is not a post-write identity contract;
- source/transformed constant-metadata semantic fingerprints are identical at `945f87ca177cabe587a3c8d6eef3b0ef419cbe4327349c0ffc7c541a4652ad37`;
- write-time Cecil resolution remains exactly bounded to the audited System.Runtime/Sentry requirements; no external dependency assembly bytes are opened;
- trusted install/private source remain unchanged and no real-StS2 CLR admission/invocation occurs in Step 32.

The authoritative report is `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt`.

## Step 33.0 — verified transformed real-StS2 CLR admission

0.0.121 must:

- start from a fresh process with no `sts2` already CLR-resident and no active Godot process-global state;
- re-run Step 32 A–D and require the exact physically closed transformed hash, length, assembly identity, MVID, transformed semantic fingerprint, and zero PrepareMethod references before CLR admission;
- re-run the physically proven Step-23 prepared-runtime preflight so the persisted Step-21/22 zero-blocker plan and prepared/live bytes are current;
- immediately re-hash the transformed image before `LoadFromStream`;
- load only that transformed primary into a dedicated `StS2Launcher-Step33-TransformedGame` AssemblyLoadContext;
- verify loaded assembly identity/MVID/context ownership and require it to be the unique resident `sts2` assembly;
- permit only exact preplanned host-framework bindings from `AssemblyLoadContext.Default` if demanded by primary admission;
- reject private prepared dependency requests in Step 33 rather than broadening the admission-only boundary;
- reject unplanned managed requests and all unmanaged-library resolution;
- require the private Step-33 context to contain transformed `sts2` only;
- re-prove OfflineReady and original/transformed/runtime-plan hashes after admission;
- perform no game type/member reflection or invocation, no `PrewarmJit`, no entry point, no Godot/game startup, no native game load, and no Harmony/MonoMod runtime patching.

Host coverage must protect four-gate ordering and prove the Step-33 admission context can load a primary while refusing a private dependency request. Physical closure requires one exact 0.0.121 A–D **4/4 PASS** report.

## Step 33.0 physical closure — verified transformed real-StS2 CLR admission

Physical 0.0.121 CLOSED Step 33 positively at **4/4 PASS**. Preserve `docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt` as authoritative evidence. The closed contract requires the exact Step-32 transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`, and transformed semantic fingerprint `47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a`; only transformed primary bytes may be the CLR load input; primary admission itself must cause zero managed/private/native resolution; the private context must initially contain transformed `sts2` only; the receipt-backed/prepared original must remain outside the CLR; and no game member may be reflected or invoked by Step 33.

## Step 34.0 — controlled transformed PrewarmJit execution

0.0.122 must:

- preserve the physically closed Step-32 transformed artifact exactly and re-run Step-32 A–D before any Step-34 CLR admission;
- preserve the Step-33 physical admission invariant that loading the transformed primary itself produces zero managed resolver requests, zero private loads, zero rejected requests and zero native attempts;
- hard-pin transformed `PrewarmJit()` to exact declaring type/signature, MVID, semantic fingerprint and MethodDef token `0x0600AFEA`;
- invoke exact transformed `PrewarmJit()` exactly once and no other game method intentionally;
- service only exact persisted Step-21/22 host-framework bindings and exact hash-pinned prepared private dependencies whose module initializer count is zero;
- keep the sole measured initializer-bearing private dependency exact `0Harmony 2.4.2.0` outside the Step-34 CLR context and fail closed if it or any other initializer-bearing dependency is requested;
- fail closed on any unplanned managed request or native resolution request;
- preserve the full target exception/inner-exception and resolver state if invocation fails rather than widening authority;
- re-prove OfflineReady, receipt-backed source hash, transformed hash, runtime-plan hash, loaded-private hashes and exact context residency after invocation;
- keep receipt-backed/prepared original `sts2.dll`, game entry point, Harmony/MonoMod patching, Godot/game startup and native game loading unauthorized.

Host coverage must protect four-gate ordering, successful initializer-free private dependency loading, initializer-bearing dependency refusal, and the closed transformed target constants. Physical closure requires one exact 0.0.122 A–D **4/4 PASS** report named `Step34-TransformedRealStS2PrewarmJitExecution.txt`.


## Step 34.0 physical closure — controlled transformed PrewarmJit execution

Physical 0.0.122 CLOSED Step 34 positively at **4/4 PASS**. Preserve `docs/history/reports/STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt` as authoritative evidence. The exact transformed `OneTimeInitialization::PrewarmJit()` MethodDef token `0x0600AFEA` must remain invocable once and return normally under the strict prepared resolver. The closed physical resolver result is 8 managed requests = 6 exact planned host-framework loads + 2 hash-pinned initializer-free prepared private loads, with 0 initializer-bearing requests, 0 unplanned managed requests and 0 native attempts. The receipt-backed/prepared original remains outside the CLR; no game entry point, Harmony/MonoMod patching or Godot/game startup is authorized by this closure.

## Step 35.0 / 35.0.1 / 35.0.2 / 35.0.3 — controlled transformed very-early initialization, crash localization, and same-run telemetry

Physical 0.0.123 attempted the Step-35 boundary and hard-terminated while the UI still appeared near Gate B. Physical 0.0.124 proved Gate B PASS and localized the same main-thread PC=`0x0` hard kill after `C_INVOKE_START`, after successful planned `GodotSharp`/`Steamworks.NET`/host-framework resolution, and before `C_INVOKE_RETURNED`. Physical 0.0.125 reproduced the same `CODESIGNING / Invalid Page` failure family but exposed that fixed-name diagnostics could be missing/stale across process runs. None of these observations closes Step 35 or revokes Steps 32–34.

0.0.126 must preserve every 0.0.125 compatibility contract while adding fail-visible run correlation:

- preserve all physically closed Step-32, Step-33 and Step-34 contracts;
- hard-pin exact source `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()` MethodDef `0x06007D02`, static parameterless `System.Threading.Tasks.Task`, plus `<ExecuteVeryEarly>d__7::MoveNext` source token `0x0600BC71`;
- re-run the exact closed Step-32 A–D transform and require the exact closed source/transformed hashes/MVID;
- independently prove source/transformed semantic fingerprint equality for the `ExecuteVeryEarly` wrapper and its async MoveNext with zero Cecil dependency resolution;
- preserve Step-33 zero-resolution primary admission when exact transformed primary enters `StS2Launcher-Step35-VeryEarly`;
- reflect only exact transformed `ExecuteVeryEarly()`, invoke it once, require a non-null exact `Task`, and await it for at most 60 seconds;
- service only exact persisted host-framework bindings and exact hash-pinned initializer-free private dependencies;
- fail closed on initializer-bearing, unplanned managed, or native requests, synchronous target exception, Task fault, or timeout;
- treat operator cancellation as **INCONCLUSIVE**, not PASS/compatibility FAIL;
- intentionally invoke no `ExecuteEssential`, `ExecuteDeferred`, `PrewarmJit`, game entry point, Harmony/MonoMod API or Godot/game startup;
- before Gate A, establish a unique Run ID/PID and durably create `Step35-CrashCheckpoint-<RunId>.txt`, `Step35-CurrentRun.txt`, and `Step35-LastCheckpoint.txt`; refuse the experiment visibly if the initial journal cannot be durably created/flushed;
- after Gate-A semantic verification and before CLR admission, durably create `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt` carrying the same Run ID/PID; stop before Gate B if that same-run map cannot be established;
- append every B/C/resolver checkpoint to the run-specific journal with UTC, Run ID, PID and managed thread ID, then independently flush the same latest line to `Step35-LastCheckpoint.txt`;
- never infer that a fixed-name/static artifact belongs to a crash process without matching Run ID/PID;
- never consume any telemetry as trusted runtime input.

Host coverage continues to protect four-gate ordering, first-failure stopping, exact target constants, initializer-bearing refusal, checkpoint callback coverage, and static-map callsite/await tagging. Static validation additionally protects the run-correlated telemetry filenames, Run ID/PID propagation, fail-visible initialization, and 0.0.126 release identity. Physical closure still requires one exact A–D **4/4 PASS** report.


## Step 35.0.5 / 0.0.128 diagnostic-clone contract
0.0.128 must preserve the exact Step-32 transformed source SHA-256 and never overwrite or CLR-load that file. After re-verification it may create one separate diagnostic clone that must preserve assembly identity/MVID and the exact `ExecuteVeryEarly` signature. The clone may add only the Step-35 diagnostic bridge and `INMETHOD_*` entry markers for the current pre-first-await localization set. The prepared resolver, initializer-bearing refusal, native refusal, one-Invoke rule, <=60-second await, cancellation semantics, and later-boundary prohibitions remain unchanged.

Additional 0.0.128 evidence-semantics contract:

- Gate C is `DiagnosticExecuteVeryEarlyInvocation`, not exact transformed execution;
- the exact closed transformed source is re-hashed unchanged immediately after diagnostic-clone emission and before Gate-B admission;
- Gate B/C CLR-load/execute only the separately hash-pinned diagnostic clone;
- active UI/report summaries must label diagnostic A–D 4/4 as **NOT STEP 35 CLOSURE**;
- no 0.0.128 derivative result may close Step 35, even if all diagnostic gates complete;
- after localization, compatibility work must return to a separately defined authoritative transformed artifact before physical closure can be claimed.

## Step 35.0.6 / 0.0.129 deferred-open diagnostic-clone contract
All Step-35.0.5 derivative/evidence restrictions remain in force. In addition, the diagnostic clone source open must use Cecil `ReadingMode.Deferred`; `ReadingMode.Immediate` is forbidden in `CreateInstrumentedDiagnosticClone`. The bounded writer resolver must observe zero requests before `Configure(module)`. The audited external constant requirement set must be validated before `module.Write`, and every write-time request must be serviced only by the configured in-memory `System.Runtime`/`Sentry` surrogates. Post-write verification must continue to use a rejecting resolver. Physical 0.0.127/0.0.128 Gate-A failures are diagnostic tooling failures and do not advance the 0.0.126 runtime frontier.


## Step 35.0.7 / 0.0.130 generic delegate MemberRef contract

All Step-35.0.6 derivative/evidence restrictions remain in force. The diagnostic bridge must model open `System.Action<T>` with one type generic parameter, construct the callback field as `System.Action<string>`, and encode its `Invoke` MemberRef parameter as the declaring-type generic variable `!0`, not concrete `System.String`. After serialization, rejecting-resolver verification must find exactly one bridge `callvirt` whose declaring type is `Action<string>` and whose sole parameter is a type `GenericParameter` at position 0. The physically disproven synthetic `invoke.Parameters.Add(new ParameterDefinition(module.TypeSystem.String))` form is forbidden.

## Step 35.0.8 / 0.0.131 Save/Platform/Godot localization contract

Physical 0.0.130 proved the generic-delegate MemberRef correction and localized the hard termination beneath `SaveManager.get_Instance`, before either settings-init method. The active 0.0.131 derivative must preserve all exact Step-35 authority constraints and may add only output-only instrumentation needed to distinguish the statically verified Save/Platform/Godot path.

Required invariants:

- exact closed Step-32 transformed source hash/identity/MVID remains unchanged and outside the CLR;
- diagnostic source open remains file-backed `ReadingMode.Deferred`, with zero resolver requests before bounded writer-resolver configuration;
- the diagnostic bridge is produced by the same helper covered by a synthetic serialize/reopen regression and must remain `Action<string>::Invoke(!0)`, never `Invoke(string)`;
- marker targets include exact managed methods `SaveManager.ConstructDefault`, `UserDataPathProvider.GetAccountScopedBasePath`, `PlatformUtil.get_PrimaryPlatform`, `NullPlatformUtilStrategy..ctor`, `GodotFileIo..ctor`, and `GodotFileIo.CreateDirectory`;
- production clone verification requires exactly one `Godot.DirAccess.DirExistsAbsolute(System.String)` and one `Godot.DirAccess.MakeDirRecursiveAbsolute(System.String)` callsite inside `GodotFileIo.CreateDirectory`;
- each selected Godot callsite is wrapped by one pre and one post `INMETHOD_*` marker and the serialized adjacency is reverified after write;
- the helper refuses a selected callsite that is already a branch target before instrumentation;
- a synthetic serialize/reopen host regression protects callsite-marker adjacency;
- no Godot bootstrap/startup, native game loading, later OneTimeInitialization phase, game entry point, initializer-bearing dependency admission, arbitrary resolver fallback, or Harmony/MonoMod runtime patching is introduced;
- `System.Collections.Concurrent` or other resolver traffic may be recorded as frontier context but must not be promoted to root cause without callsite/native evidence;
- a 0.0.131 4/4 result is diagnostic completion only and **NOT Step-35 closure**.

## Step 35.0.9 / 0.0.132 NullPlatform constructor callsite-sweep contract

Physical 0.0.131 reached `INMETHOD_024 — NullPlatformUtilStrategy..ctor entered` and never reached `INMETHOD_025 — GodotFileIo..ctor entered`. The active 0.0.132 derivative must preserve every exact Step-35 authority constraint and may add only output-only instrumentation necessary to distinguish the existing call-like instructions inside that constructor.

Required invariants:

- exact closed Step-32 transformed source hash/identity/MVID remains unchanged and outside the CLR;
- diagnostic source open remains file-backed `ReadingMode.Deferred`, with zero resolver requests before bounded writer-resolver configuration;
- the diagnostic bridge remains serialized and verified as `Action<string>::Invoke(!0)`;
- all Step-35.0.8 entry markers and selected Godot callsite marker pairs remain present and verified;
- `NullPlatformUtilStrategy..ctor()` is found by exact stable type + full signature, not by assuming a post-write MethodDef token;
- the new sweep enumerates only the constructor's original `call`, `callvirt`, and `newobj` instructions and never calls Cecil `Resolve`;
- the direct base `.ctor` call is intentionally excluded from the sweep;
- diagnostic bridge calls are excluded from the sweep;
- each other original call-like instruction is wrapped by exactly one unique `INMETHOD_NPxxx_PRE/POST` pair using the original constructor CALLSITE ordinal;
- a selected callsite that is a branch target is rejected before write;
- serialized reopen under a rejecting resolver must verify each NP pair immediately around the same opcode and callee;
- the run-specific static map includes `[NULL PLATFORM CTOR IL]` and exact `CALLSITE#xxx` ordinals;
- a synthetic serialize/reopen host regression covers skipped base `.ctor` plus instrumented `newobj` and `call` instructions;
- no Godot bootstrap/startup, native game loading, later OneTimeInitialization phase, game entry point, initializer-bearing dependency admission, arbitrary resolver fallback, or Harmony/MonoMod runtime patching is introduced;
- the final `System.Collections.Concurrent` resolver record may be preserved as frontier context but cannot be promoted to root cause merely because it is last;
- a 0.0.132 4/4 result is diagnostic completion only and **NOT Step-35 closure**.

## Step 35.0.12 — corrected ordinals, MaxStack safety, and CommandLine localization

- Physical 0.0.132 proved `INMETHOD_NP003_PRE` corresponded to exact-source `CALLSITE#002`; injected bridge calls must be excluded **before** ordinal accounting.
- NullPlatform's direct base `.ctor` remains unwrapped but still consumes exact-source CALLSITE#001.
- Production ordering is entry marker first, sweep second; a host regression must reproduce that ordering.
- `CommandLineHelper.TryGetValue` has `INMETHOD_027`; its type initializer has the existing `INMETHOD_CCTOR` entry marker.
- `CommandLineHelper..cctor` uses ordered `INMETHOD_CLxxx_PRE/POST`; `TryGetValue` uses `INMETHOD_CLTVxxx_PRE/POST`.
- Same-run exact-source output includes `[COMMAND LINE HELPER CCTOR IL]` and `[COMMAND LINE HELPER TRYGETVALUE IL]`.
- The cctor plan must contain `Godot.OS.GetCmdlineArgs`; otherwise Gate A fails before CLR admission.
- New CommandLine sweeps may skip unrelated branch-target callsites, but skipped calls still consume exact-source ordinals; the required Godot call cannot be silently skipped.
- Physical 0.0.133 is pinned as a diagnostic instrumentation failure: corrected `NP002`, no CommandLine cctor/CL marker, nested `InvalidProgramException`, normal `RUN_END`; it must not be reclassified as a Godot compatibility result.
- Every live-stack diagnostic callsite sweep reserves one additional `MaxStack` slot; targeted callsite markers are hardened likewise.
- Gate A must record exact-source CommandLine cctor MaxStack and require serialized diagnostic MaxStack = source + 1.
- Four stack-neutral critical markers must bracket `_args` dictionary construction/assignment and `Godot.OS.GetCmdlineArgs` invocation/result storage.
- Host regressions must include actual CLR loading/execution of a generated tight-MaxStack rewritten cctor, not only Cecil round-trip inspection.
- Exact Step-32 transformed source, resolver/native prohibitions, one-invocation rule, 60-second await, fresh-process rule, and no-Godot-bootstrap contract remain unchanged.
- Diagnostic 4/4 is not exact Step-35 closure.
