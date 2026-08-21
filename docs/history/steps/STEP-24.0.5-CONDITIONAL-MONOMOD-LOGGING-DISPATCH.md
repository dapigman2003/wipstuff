# Step 24.0.5 — Conditional MonoMod Logging Dispatch Classification

## Physical evidence that motivated this candidate

Step 24.0.4 / `0.0.77 (77)` reached a physical iPhone and failed safely **0/4 at Gate A** during the target-only `0Harmony.dll` automatic-initialization closure audit. This is materially different from builds 75 and 76: the previous Cecil `GodotSharp` resolution failure is gone, and Gate A produced the actual bounded closure evidence.

The physical audit found exactly seven conservative dispatch findings, all within the merged MonoMod logging implementation:

- one bodyless `IDebugFormattable.TryFormatInto` interface dispatch;
- two bodyless logging-delegate `Invoke` dispatches;
- two logging-delegate constructors;
- two corresponding indirect function/delegate targets in `TryInitializeLogToFile` and `TryInitializeMemoryLog`.

The same physical report measured exactly four automatically triggerable type/module initializers:

1. `<Module>..cctor` — two instructions: call `MMDbgLog::LogVersion()`, then return;
2. `MonoMod.Switches::.cctor` — environment-switch initialization;
3. `MonoMod.Logs.DebugLog::.cctor` — logger singleton/cache initialization;
4. `MonoMod.Logs.DebugLog/LevelSubscriptions::.cctor` — construction of the `None` subscription state.

Gate B never ran. No Step 24 real assembly entered the CLR, and Step 23.4.3 remains the physically closed boundary.

## Correction

Step 24.0.5 / `0.0.78 (78)` does **not** globally allow delegates, interface dispatch, bodyless methods, or function pointers. The original conservative Cecil audit remains intact and continues to emit the raw findings.

A separate fail-closed policy may classify the **exact seven physically measured findings** as conditionally dormant only when every one of these conditions holds:

- target identity is exactly `0Harmony 2.4.2.0`;
- the raw conservative hazard set exactly equals the seven findings measured by physical build 77 — no missing or additional finding;
- the automatic-initializer set is exactly the four physically measured methods;
- those four initializers retain the physically measured structural markers, including the two-instruction `<Module>..cctor -> MMDbgLog.LogVersion() -> ret` shape;
- no managed debugger is attached;
- no environment-variable name beginning with `MONOMOD_` is present;
- none of the measured MonoMod logging AppContext keys is overridden.

Only names of environment/AppContext overrides are reported; values are intentionally excluded.

If the conditional policy passes, Gate A reports both layers of evidence:

- raw conservative findings: **7**;
- conditionally dormant MonoMod logging findings: **7**;
- effective blocking initializer hazards: **0**;
- conditional automatic-initialization policy: **PASS**.

Any fingerprint drift, additional/missing finding, changed initializer shape, P/Invoke, `calli`, generic function/delegate indirection outside the exact measured set, native-loader API, reflection/dynamic execution, unresolved local call, unexpected non-framework edge, debugger attachment, or MonoMod logging override still stops Gate A before any Step 24 CLR load.

## Gates B–D

Unchanged from Step 24.0.4:

- Gate B reproduces the physically accepted Step 23 initializer-free private CLR state and keeps `0Harmony` absent.
- Gate C is the sole new execution boundary: admit exactly receipt-verified `0Harmony`, load it in the Step 24 private context, and use `RuntimeHelpers.RunModuleConstructor` as the explicit module-constructor completion barrier.
- Gate D re-hashes the plan/prepared/live bytes, re-proves OfflineReady, and requires exact context membership plus zero native/unplanned requests.

No Harmony patch API, StS2 member/entry-point invocation, Godot/game startup, or native game-library loading is added.

## Candidate identity

- step: **24.0.5**
- version: **0.0.78 (78)**
- workflow: **`ios-step-24`**
- IPA: **`artifacts/StS2-Launcher-Step-24.ipa`**
- device report: `Documents/StS2Launcher/Reports/Step24-ControlledManagedInitialization.txt`
