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

Only after this combined rewrite-before-load + interpreted-execution boundary closes physically may a later Step-28 candidate select a narrowly audited real StS2 compatibility transformation.
