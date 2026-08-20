# Current Status — Step 23.4.3 First Real StS2 CLR Load Boundary

## Physically closed boundary

**Steps 01–22 are closed on a physical iPhone. Step 22.4.2 / 0.0.64 is the accepted canonical pre-game-load foundation.**

The canonical foundation is fully green: Step 19 A–D, Step 22 A–D, zero runtime-binding blockers, runtime closure ready = YES, OfflineReady = PASS, Foundation 5/5 = PASS, and all other current regressions pass.

## Step 23 physical evidence

Codemagic host-test iterations 23.0–23.3 isolated test-only issues without weakening production safeguards. Step 23.3 then reached the intended physical pre-load safety boundary.

Physical Step 23.3 / 0.0.68 result:

- Gate A: FAIL before any real CLR load;
- stage: module-initializer safety after exact binding-plan metadata coverage;
- offender: `0Harmony, Version=2.4.2.0, Culture=neutral, PublicKeyToken=null`;
- `<Module>..cctor` count: 1;
- no real `sts2.dll` CLR load occurred.

This proves the first automatic-execution boundary is in a dependency, not the primary game assembly.

## Recent Codemagic fixture evidence

Step 23.4 introduced deferred handling for initializer-bearing dependencies while preserving a strict zero-initializer requirement for the primary `sts2.dll`.

Step 23.4.1 fixed a compile-only missing `Mono.Cecil.Cil` import.

Step 23.4.2 / 0.0.71 passed canonical static validation and Core compilation, then reached **153/155 host tests PASS**. Both failures were synthetic module-initializer tests:

- `GateARejectsPrimaryModuleInitializerBeforeAnyRealClrLoad`;
- `DependencyModuleInitializerIsDeferredWhilePrimaryAndSafeClosureLoad`.

Both failed for the same fixture-only reason: Cecil's `MainModule.TypeSystem.Void` had been accessed while the synthetic module had no recognized core-library AssemblyRef. Cecil therefore embedded a legacy `mscorlib, Version=4.0.0.0` scope in the initializer signature. Clearing the module's `AssemblyReferences` collection afterward did not remove that embedded type scope, so the writer recreated `mscorlib` in the final file. Production Step 23 logic did not fail and remains unchanged.

## Active candidate — Step 23.4.3

- Version: **0.0.72 (72)**
- Codemagic workflow: **`ios-step-23-4-3`**
- Live iOS project: `src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`
- Trusted source: Step 12 receipt-backed managed install
- Execution input: Step 21/22 zero-blocker prepared runtime + persisted binding plan
- Entry point, game type/member reflection, game method invocation, Godot startup, native game libraries: **still out of scope**

### Step 23.4.3 fixture correction

The synthetic fixture now constructs metadata in the correct order instead of trying to repair it after construction:

1. obtain the actual .NET 9 `System.Runtime` identity from the host;
2. add every declared AssemblyRef, including `System.Runtime` for initializer-bearing fixtures, **before** touching Cecil `TypeSystem.Void`;
3. only then create `<Module>..cctor` using `MainModule.TypeSystem.Void`;
4. assert Cecil selected the predeclared `System.Runtime` reference as the primitive void scope;
5. write and reopen in `ReadingMode.Immediate`;
6. require the persisted AssemblyRef set to equal the declared set exactly;
7. require no `mscorlib` reference;
8. require the reopened initializer return type to be `MetadataType.Void` with scope `System.Runtime`.

This follows Cecil's own core-library selection behavior: if a recognized core-library reference such as `System.Runtime` already exists, `TypeSystem.Void` uses it; only an otherwise-unscoped synthetic module falls back to legacy `mscorlib`.

**No production core-library alias is added. Production Step 23 resolver/binding behavior is unchanged and remains strict.**

### Gate A — PreparedLoadPreflight

Before any real game CLR load, re-prove OfflineReady, plan/manifest identity, zero blockers, exact prepared/live hashes, IL-only identities, and exact Cecil `AssemblyRef` plan coverage.

Boundary-specific module-initializer policy:

- the **primary `sts2.dll` must have zero `<Module>..cctor` initializers**;
- any initializer-bearing dependency is statically audited and added to a deferred set;
- Gate A records compact Cecil IL for each deferred initializer in `Reports/Step23-FirstRealGameLoad.txt`;
- no deferred assembly is loaded.

### Gate B — PrimaryAssemblyLoad

SHA-1 recheck the real prepared primary, create the dedicated private `AssemblyLoadContext`, and call `LoadFromStream` on `sts2.dll` only. Require exact identity/context ownership and exactly one real game assembly. If the CLR unexpectedly requests a deferred initializer-bearing dependency during primary load, the resolver refuses it and Gate B fails.

### Gate C — PlannedDependencyResolution

Resolve all planned host bindings and load the maximal initializer-free private prepared closure. Planned private targets with module initializers are deliberately skipped and counted as deferred requirements. Any actual CLR resolver request for one of them is a hard failure.

Success requires:

- all planned host bindings resolve from `AssemblyLoadContext.Default`;
- all initializer-free private targets resolve only from the exact receipt-hashed prepared set;
- the private context equals the expected initializer-free prepared set;
- every deferred initializer-bearing private assembly remains outside the CLR;
- zero native-load attempts and zero unplanned managed requests.

### Gate D — LoadIsolationAudit

Rehash the plan, every prepared/live assembly, and re-prove OfflineReady. Require exactly one real `sts2` in the dedicated context, exact initializer-free context membership, zero native attempts, zero rejected/unplanned requests, and zero deferred-initializer assemblies loaded.

A Step 23.4.x 4/4 pass therefore means: **the real `sts2.dll` plus the maximal automatically-inert managed closure can enter the iPhone CLR, while `0Harmony` remains explicitly outside the CLR for the next initialization boundary.**

## Acceptance required for Step 23 closure

From a fresh process:

1. confirm `STEP 23.4.3 — FIRST REAL STS2 CLR LOAD BOUNDARY`, version `0.0.72`;
2. run Step 23 A–D and stop at the first failure;
3. Gate A: primary module initializers = 0; deferred initializer-bearing dependencies may be nonzero and must be reported;
4. Gate B: first real `sts2.dll` CLR load = PASS;
5. Gate C: initializer-free managed closure = PASS, deferred dependencies not loaded, zero native attempts;
6. Gate D: PASS, summary 4/4;
7. OfflineReady = PASS;
8. Foundation 5/5 = PASS.

After Gate B the real game assembly remains process-resident; force-quit before rerunning pre-load regressions.

## Likely next step

If Step 23 closes, Step 24 becomes the **audited automatic-initialization boundary**, starting with the exact `0Harmony <Module>..cctor` IL/call metadata exported by Gate A. Do not invoke Harmony or broaden native/game execution before that initializer is understood.
