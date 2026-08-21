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

- Version: **0.0.78 (78)**
- Codemagic workflow: **`ios-step-24`**
- IPA: **`artifacts/StS2-Launcher-Step-24.ipa`**
- Live iOS project: `src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`
- Trusted source: Step 12 receipt-backed managed install
- Execution input: physically proven Step 21/22 zero-blocker prepared runtime and persisted binding plan
- Closed prerequisite: physical Step 23.4.3 4/4 + OfflineReady + Foundation 5/5

Step 24.0 / `0.0.73 (73)` did **not** reach host tests or iOS build. Codemagic static validation passed 281/281, then Core compilation failed because the new Step 24 subsystem called a nonexistent `SteamOfflineInstallInspection.InspectAsync` API at its pre/post OfflineReady checks.

Step 24.0.1 / `0.0.74 (74)` corrected that compile issue and reached the full host suite. Canonical static validation passed **287/287** and Core/test compilation succeeded, but host tests finished **160/162**. The two failures were both intentional Gate A safety assertions: `GateARejectsReachablePInvokeBeforeAnyStep24ClrLoad` and `GateARejectsImplicitTypeInitializerPInvokeBeforeAnyStep24ClrLoad`. The fixtures correctly encoded P/Invoke; the production audit resolved the same-assembly native stub but then skipped it because P/Invoke methods have no managed `MethodBody`. No IPA was produced and no physical Step 24 evidence exists for build 74.

Step 24.0.2 / `0.0.75 (75)` corrected the P/Invoke audit and reached a physical iPhone. Gate A stopped safely at `prepared target classification` before any Step 24 CLR load with `AssemblyResolutionException: Failed to resolve assembly: 'GodotSharp, Version=4.5.1.0, Culture=neutral, PublicKeyToken=null'`. The failure came from using Cecil `MethodReference.Resolve()` while recursively traversing a nominally same-assembly initializer call: Cecil resolution can walk external type/base/member metadata, which violates Gate A's intended self-contained metadata-only audit. Step 23 remained the latest physically closed boundary; no Gate B/C/D evidence was generated.

Step 24.0.3 / `0.0.76 (76)` reached a physical iPhone and again failed safely **0/4 at Gate A**, at the same broad `prepared target classification` stage with the same `AssemblyResolutionException` for `GodotSharp 4.5.1.0`. Because build 76 had already removed the explicit `MethodReference.Resolve()` call, this result proved the build-75 diagnosis was incomplete: some other eager/broad Cecil metadata path could still request external metadata before the target-specific audit. Gate B never ran and no Step 24 CLR state was created.

Step 24.0.4 / `0.0.77 (77)` reached a physical iPhone and failed safely **0/4 at Gate A**, but it resolved the metadata ambiguity from builds 75–76. Gate A reached the exact `0Harmony.dll` target closure and reported seven conservative execution findings, all in merged MonoMod logging dispatch: one `IDebugFormattable` bodyless interface dispatch, two logging-delegate `Invoke` targets, two logging-delegate constructors, and two corresponding `ldftn`/delegate-indirection findings. The report also measured exactly four automatic initializers: `<Module>..cctor`, `MonoMod.Switches::.cctor`, `MonoMod.Logs.DebugLog::.cctor`, and `MonoMod.Logs.DebugLog/LevelSubscriptions::.cctor`; the module initializer itself is exactly two instructions, `MMDbgLog::LogVersion()` then `ret`. Gate B never ran, so no Step 24 CLR load occurred.

Step 24.0.5 / `0.0.78 (78)` is the active candidate. The conservative metadata audit is unchanged and still records all raw findings. A separate fail-closed conditional policy may downgrade **only the exact seven physically measured logging findings** to dormant when the target remains exactly `0Harmony 2.4.2.0`, the exact four-method automatic-initializer set and measured structural markers are unchanged, no managed debugger is attached, no `MONOMOD_*` environment-variable name is present, and no relevant MonoMod logging AppContext key is overridden. Any missing/additional/changed finding, P/Invoke, `calli`, generic indirect target outside the measured set, native/reflection/dynamic/unresolved/non-framework edge, initializer-shape drift, or non-inert logger state remains blocking before Gate B. Gate ordering, runtime resolver policy, module-constructor barrier, and the no-Harmony/game/native-execution boundary are unchanged.

### Gate A — InitializationPreflight

Before any Step 24 game/Harmony CLR load:

1. require a fresh process;
2. replay the accepted Step 23 Gate A preflight unchanged;
3. reload the persisted zero-blocker runtime plan;
4. require OfflineReady and matching depot/manifest identity;
5. shallow-scan every exact prepared plan member for module-initializer presence using deferred, resolution-rejecting Cecil metadata;
6. require exactly one initializer-bearing dependency;
7. require it to be exactly `0Harmony 2.4.2.0` with exactly one `<Module>..cctor`;
8. after selecting the sole exact target, audit only that file's module initializer plus bounded same-assembly automatic-initialization closure, including same-assembly type constructors that static calls/fields could implicitly trigger, resolving same-assembly calls only from definitions already present in the audited module;
9. preserve the complete conservative hazard set; reject P/Invoke, `calli`, native-library APIs, explicit runtime-constructor APIs, reflection/dynamic invocation, unresolved local calls, unexpected non-framework execution edges, and generic function/delegate/bodyless dispatch; only the exact seven physically measured MonoMod logging dispatch findings may be conditionally downgraded when the exact measured initializer shape and inert-logging preconditions all match;
10. report raw conservative findings separately from conditionally dormant findings and effective blocking hazards.

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

1. confirm `STEP 24 — CONTROLLED 0HARMONY MODULE INITIALIZATION BOUNDARY`, version `0.0.78`;
2. run Step 24 A–D and stop at the first failing gate;
3. Gate A: exact sole target = `0Harmony 2.4.2.0`, one module initializer, bounded automatic-initialization closure fully measured; for the current physical target expect raw conservative findings = 7, conditionally dormant findings = 7, effective `Initializer hazards` = 0, and conditional policy = PASS; any fingerprint/state drift must fail before Gate B;
4. Gate B: accepted Step 23 initializer-free state reproduced, `0Harmony` absent;
5. Gate C: `0Harmony` load + `RuntimeHelpers.RunModuleConstructor` completion barrier = PASS, zero native/unplanned requests;
6. Gate D: PASS, summary 4/4;
7. OfflineReady = PASS;
8. Foundation 5/5 = PASS.

After Gate B, the real managed game context remains process-resident; force-quit before rerunning fresh-process Step 21/22/23 regressions.

## Next frontier if Step 24 closes

Do **not** assume Harmony patching is viable merely because its module initializer completes. The next subsystem should use the actual Step 24 evidence to choose the smallest managed API/type initialization boundary. Explicit Harmony construction/patching, broad game reflection, Godot/game initialization, and native integration remain later boundaries until separately gated.
