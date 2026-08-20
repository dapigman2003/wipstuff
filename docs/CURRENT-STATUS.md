# Current Status — Step 23.4.2 First Real StS2 CLR Load Boundary

## Physically closed boundary

**Steps 01–22 are closed on a physical iPhone. Step 22.4.2 / 0.0.64 is the accepted canonical pre-game-load foundation.**

The canonical foundation is fully green: Step 19 A–D, Step 22 A–D, zero runtime-binding blockers, runtime closure ready = YES, OfflineReady = PASS, Foundation 5/5 = PASS, and all other current regressions pass.

## Step 23 evidence so far

Codemagic host-test iterations 23.0–23.3 isolated and corrected test-only issues without weakening production safeguards. Step 23.3 produced a physical iPhone report and reached the intended pre-load safety boundary.

Physical Step 23.3 / 0.0.68 result:

- Gate A: FAIL before any real CLR load;
- stage: module-initializer safety after exact binding-plan metadata coverage;
- offender: `0Harmony, Version=2.4.2.0, Culture=neutral, PublicKeyToken=null`;
- `<Module>..cctor` count: 1;
- no real `sts2.dll` CLR load occurred.

This proves the Step 23 load-only policy found an automatic-execution boundary in a dependency, not in the primary game assembly.

## Active candidate — Step 23.4.2

- Version: **0.0.71 (71)**
- Codemagic workflow: **`ios-step-23-4-2`**
- Live iOS project: `src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`
- Trusted source: Step 12 receipt-backed managed install
- Execution input: Step 21/22 zero-blocker prepared runtime + persisted binding plan
- Entry point, game type/member reflection, game method invocation, Godot startup, native game libraries: **still out of scope**


### Step 23.4.1 Codemagic host-test result

The Step 23.4.1 Codemagic run passed canonical static validation and Core compilation. Host tests reached **154/155 PASS**. The sole failure was `DependencyModuleInitializerIsDeferredWhilePrimaryAndSafeClosureLoad`: the Cecil-built synthetic initializer fixture carried an artificial legacy `mscorlib, Version=4.0.0.0` AssemblyRef. The synthetic plan faithfully recorded it, then Gate C correctly failed when .NET 9 refused to bind that legacy identity.

Step 23.4.2 changes only the host-test fixture generator. It removes Cecil's temporary legacy core-library AssemblyRef after constructing the primitive-void module initializer and verifies the written fixture contains no `mscorlib` reference. Production Step 23 binding/load code is unchanged and remains intentionally strict.

### Gate A — PreparedLoadPreflight

Before any real game CLR load, re-prove OfflineReady, plan/manifest identity, zero blockers, exact prepared/live hashes, IL-only identities, and exact Cecil `AssemblyRef` plan coverage.

The module-initializer policy is now boundary-specific:

- the **primary `sts2.dll` must have zero `<Module>..cctor` initializers**;
- any initializer-bearing *dependency* is statically audited and added to a deferred set;
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

A Step 23.4 4/4 pass therefore means: **the real `sts2.dll` plus the maximal automatically-inert managed closure can enter the iPhone CLR, while `0Harmony` remains explicitly outside the CLR for the next initialization boundary.**

## Acceptance required for Step 23 closure

From a fresh process:

1. confirm `STEP 23.4.2 — FIRST REAL STS2 CLR LOAD BOUNDARY`, version `0.0.71`;
2. run Step 23 A–D and stop at the first failure;
3. Gate A: primary module initializers = 0; deferred initializer-bearing dependencies may be nonzero and must be reported;
4. Gate B: first real `sts2.dll` CLR load = PASS;
5. Gate C: initializer-free managed closure = PASS, deferred dependencies not loaded, zero native attempts;
6. Gate D: PASS, summary 4/4;
7. OfflineReady = PASS;
8. Foundation 5/5 = PASS.

After Gate B the real game assembly remains process-resident; force-quit before rerunning pre-load regressions.

## Likely next step

If 23.4 passes, Step 24 becomes the **audited automatic-initialization boundary**, starting with the exact `0Harmony <Module>..cctor` IL/call metadata exported by Gate A. Do not invoke Harmony or broaden native/game execution before that initializer is understood.
