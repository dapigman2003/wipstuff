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

Step 29.0 exists because Step 28 is physically closed but the repository does not preserve the exact old Step-17 physical source→target samples needed to justify a real semantic rewrite. The active regression contract is therefore read-only target re-audit/selection:

- Gate A requires a fresh process, OfflineReady, exactly one receipt-backed `data_sts2_macos_arm64/sts2.dll`, receipt SHA-1 verification, diagnostic SHA-256, `ReadingMode.Deferred`, a rejecting Cecil resolver, zero resolver requests, and no CLR-resident `sts2`;
- Gate B inspects concrete primary `sts2.dll` IL only and fingerprints candidate sites by source method, metadata token, IL offset/opcode, target scope/member and canonical method-body SHA-256;
- candidate categories are bounded to Harmony runtime patch APIs, MonoMod runtime detours, Reflection.Emit, PrepareMethod, dynamic assembly loading, selected platform/native managed APIs, and indirect `calli`;
- `Expression.Compile` is counted but excluded from candidacy because the Step-19 host-interpreter compatibility boundary is already physically closed;
- Gate C deterministically selects at most one audit candidate; `NO DIRECT PRIMARY TARGET` is a valid evidence outcome and must not be replaced by an ad-hoc broad target;
- Gate C selection is **audit evidence only** and Step 29 performs zero Cecil writes;
- Gate D re-hashes the source, re-proves OfflineReady, requires zero `sts2` CLR load/invocation and zero Cecil resolver requests, and retains the no-Harmony/no-Godot/no-native-game boundary.

A physical **4/4 PASS** closes target selection only. The next candidate must inspect the selected exact method semantics and predeclare one transformation before any real StS2 write is introduced.
