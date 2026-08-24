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
