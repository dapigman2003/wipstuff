# Current Status — Step 24 Controlled 0Harmony Module Initialization Boundary

## Physically closed boundary

**Steps 01–23 are closed on a physical iPhone.**

The latest closed runtime boundary is **Step 23.4.3 / 0.0.72 (72)**.

Physical Step 23.4.3 evidence:

- Gate A — PreparedLoadPreflight: **PASS**;
- Gate B — PrimaryAssemblyLoad: **PASS**;
- Gate C — PlannedDependencyResolution: **PASS**;
- Gate D — LoadIsolationAudit: **PASS**;
- Step 23 summary: **4/4 PASS**;
- OfflineReady after Step 23: **PASS**;
- Foundation after Step 23: **5/5 PASS**.

This closes the first-real-load boundary. The receipt-backed real `sts2.dll` plus the maximal initializer-free prepared managed closure are physically proven CLR-loadable in one dedicated private iPhone load context. The known initializer-bearing dependency remained outside the CLR during Step 23.

## Proven Step 23 frontier

The sole deferred initializer-bearing prepared dependency observed by Step 23 is:

- `0Harmony, Version=2.4.2.0, Culture=neutral, PublicKeyToken=null`;
- `<Module>..cctor` count: **1**.

The primary `sts2.dll` has zero module initializers under the accepted Step 23 plan.

Step 23 did not intentionally invoke a game entry point, inspect/invoke game types or members, call Harmony APIs, start Godot/game state, or resolve a native game library.

## Active candidate — Step 24

- Version: **0.0.75 (75)**
- Codemagic workflow: **`ios-step-24`**
- IPA: **`artifacts/StS2-Launcher-Step-24.ipa`**
- Live iOS project: `src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`
- Trusted source: Step 12 receipt-backed managed install
- Execution input: physically proven Step 21/22 zero-blocker prepared runtime and persisted binding plan
- Closed prerequisite: physical Step 23.4.3 4/4 + OfflineReady + Foundation 5/5

Step 24.0 / `0.0.73 (73)` did **not** reach host tests or iOS build. Codemagic static validation passed 281/281, then Core compilation failed because the new Step 24 subsystem called a nonexistent `SteamOfflineInstallInspection.InspectAsync` API at its pre/post OfflineReady checks.

Step 24.0.1 / `0.0.74 (74)` corrected that compile issue and reached the full host suite. Canonical static validation passed **287/287** and Core/test compilation succeeded, but host tests finished **160/162**. The two failures were both intentional Gate A safety assertions: `GateARejectsReachablePInvokeBeforeAnyStep24ClrLoad` and `GateARejectsImplicitTypeInitializerPInvokeBeforeAnyStep24ClrLoad`. The fixtures correctly encoded P/Invoke; the production audit resolved the same-assembly native stub but then skipped it because P/Invoke methods have no managed `MethodBody`. No IPA was produced and no physical Step 24 evidence exists for build 74.

Step 24.0.2 / `0.0.75 (75)` is the minimal production audit correction. After resolving a same-assembly call, Gate A now checks P/Invoke metadata before applying the managed-body traversal filter. Reachable P/Invoke stubs fail closed immediately; any other reachable same-assembly method with no managed IL body also fails closed as an unmeasured execution edge. Gate ordering, target identity, resolver policy, module-constructor barrier, and the no-Harmony/game/native-execution boundary are unchanged.

### Gate A — InitializationPreflight

Before any Step 24 game/Harmony CLR load:

1. require a fresh process;
2. replay the accepted Step 23 Gate A preflight unchanged;
3. reload the persisted zero-blocker runtime plan;
4. require OfflineReady and matching depot/manifest identity;
5. classify initializer-bearing prepared dependencies;
6. require exactly one initializer-bearing dependency;
7. require it to be exactly `0Harmony 2.4.2.0` with exactly one `<Module>..cctor`;
8. export the module initializer plus the bounded same-assembly automatic-initialization closure, including same-assembly type constructors that static calls/fields could implicitly trigger;
9. fail closed if the reachable closure contains P/Invoke, `calli`, function/delegate indirection, direct native-library APIs, explicit runtime-constructor APIs, reflection/dynamic invocation, or an unexpected non-framework execution edge.

Gate A is metadata-only. No Step 24 real game/Harmony assembly is loaded.

### Gate B — ProvenLoadStateReplay

Create the dedicated Step 24 private `AssemblyLoadContext` and reproduce the already-proven Step 23 load-only state in that same context:

- load the real prepared `sts2.dll`;
- resolve every planned host binding from `AssemblyLoadContext.Default`;
- load the maximal initializer-free private prepared closure;
- keep every initializer-bearing dependency deferred;
- require exact private-context membership;
- require zero native loads and zero rejected/unplanned managed requests;
- require `0Harmony` to remain absent.

Gate B crosses no new architectural frontier; it establishes the proven Step 23 state inside the exact context that Gate C will advance.

### Gate C — DeferredModuleInitialization

This is the only new Step 24 execution boundary.

1. re-hash the exact prepared `0Harmony` target immediately before load;
2. permit exactly that initializer-bearing identity in the Step 24 resolver;
3. load `0Harmony` from the receipt-hashed prepared bytes;
4. require exact identity and Step 24 context ownership;
5. call `RuntimeHelpers.RunModuleConstructor(targetAssembly.ManifestModule.ModuleHandle)` as the explicit module-constructor completion barrier;
6. require zero native-library attempts;
7. require zero unplanned managed requests;
8. require no other initializer-bearing assembly to enter the CLR;
9. re-hash the prepared `0Harmony` bytes after initialization.

Gate C still does **not** call a Harmony patch API or any StS2 game member.

### Gate D — PostInitializationAudit

After controlled module initialization:

- re-hash the persisted runtime plan;
- re-hash every prepared and live receipt-backed managed file;
- re-prove OfflineReady;
- require the private context to equal the Step 23 initializer-free closure plus exactly `0Harmony`;
- require zero native-load attempts;
- require zero rejected/unplanned managed requests;
- require no trusted/prepared/live mutation;
- record that explicit Harmony patching, game invocation, Godot startup, and native game loading remain **NO**.

## Acceptance required for Step 24 closure

From a fresh process:

1. confirm `STEP 24 — CONTROLLED 0HARMONY MODULE INITIALIZATION BOUNDARY`, version `0.0.75`;
2. run Step 24 A–D and stop at the first failing gate;
3. Gate A: exact sole target = `0Harmony 2.4.2.0`, one module initializer, bounded automatic-initialization closure fully measured, hazards = 0;
4. Gate B: accepted Step 23 initializer-free state reproduced, `0Harmony` absent;
5. Gate C: `0Harmony` load + `RuntimeHelpers.RunModuleConstructor` completion barrier = PASS, zero native/unplanned requests;
6. Gate D: PASS, summary 4/4;
7. OfflineReady = PASS;
8. Foundation 5/5 = PASS.

After Gate B, the real managed game context remains process-resident; force-quit before rerunning fresh-process Step 21/22/23 regressions.

## Next frontier if Step 24 closes

Do **not** assume Harmony patching is viable merely because its module initializer completes. The next subsystem should use the actual Step 24 evidence to choose the smallest managed API/type initialization boundary. Explicit Harmony construction/patching, broad game reflection, Godot/game initialization, and native integration remain later boundaries until separately gated.
