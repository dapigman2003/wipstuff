# Current Status — Step 23.1 First Real StS2 CLR Load Boundary

## Physically closed boundary

**Steps 01–22 are closed on a physical iPhone, and Step 22.4.2 is the accepted canonical foundation baseline.**

The final Step 22.4.2 acceptance was completely green:

- canonical Codemagic build/test/package path succeeded;
- Step 19 A–D: PASS after the post-Step-20 regression-contract correction;
- Step 22 A–D: PASS;
- explicit runtime-binding blockers: 0;
- runtime closure ready for first real CLR load: YES;
- OfflineReady regression: PASS;
- Foundation 5/5 regression: PASS;
- all other current user-run tests passed;
- current long diagnostics are available as Files-visible text reports.

This establishes version **0.0.64 (64)** as the protected pre-game-load foundation.

## Active candidate — Step 23.1

Step 23 crosses exactly one new runtime boundary: the first real CLR load of the receipt-backed prepared `sts2.dll`.

- Version: **0.0.66 (66)**
- Codemagic workflow: **`ios-step-23-1`**
- Live iOS project: **`src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`**
- Trusted game source: existing Step 12 receipt-backed managed install
- Execution input: existing Step 21/22 zero-blocker prepared runtime + persisted binding plan
- Game entry point/method invocation: **out of scope**
- Godot/game initialization: **out of scope**
- Native game-library resolution: **explicitly refused/out of scope**

### Gate A — PreparedLoadPreflight

Before any real game CLR load:

1. re-prove exact OfflineReady;
2. require the persisted Step 21/22 plan to match the current depot/manifest/branch;
3. require `RuntimeClosureReady=true`, zero blockers, and no blocker edges;
4. require exactly one primary `sts2` assembly;
5. require the complete prepared file set to match the plan exactly;
6. SHA-1/length reverify every prepared assembly and corresponding trusted live-install file;
7. require every prepared private assembly to be IL-only and identity-identical to the plan;
8. require the persisted plan edges to exactly cover every prepared assembly's Cecil `AssemblyRef` metadata;
9. require no framework-shaped `System.*`/`netstandard` assembly in the private set;
10. inspect module/PInvoke metadata with Cecil;
11. **reject the load if any prepared private assembly has `<Module>..cctor`**, because the load-only step must not silently cross a module-initialization boundary;
12. require no prepared private/game assembly already loaded in the process;
13. preserve the canonical iOS `RuntimeFeature.IsDynamicCodeCompiled=false` contract.

### Gate B — PrimaryAssemblyLoad

- SHA-1 recheck the prepared primary immediately before load;
- create one dedicated private `AssemblyLoadContext`;
- call `LoadFromStream` on the real prepared `sts2.dll`;
- require exact assembly identity;
- require the assembly to belong to the dedicated Step 23 context;
- do not inspect game types/members, entry point, custom attributes, or invoke any game method;
- do not intentionally resolve native libraries.

### Gate C — PlannedDependencyResolution

For every unique dependency identity in the persisted zero-blocker plan:

- `HostFramework` must resolve from `AssemblyLoadContext.Default` to the exact planned actual host identity;
- `WorkspaceExact` / `WorkspaceVersionUnified` must resolve from the exact receipt-hashed prepared set in the Step 23 private context;
- unplanned non-framework fallback is rejected;
- no downloaded desktop framework implementation fallback is permitted;
- the final private context assembly set must equal the prepared plan exactly;
- any unmanaged-library resolution attempt fails the gate.

### Gate D — LoadIsolationAudit

After the real managed load:

- re-hash the persisted plan;
- re-hash every prepared and live-install assembly;
- re-prove OfflineReady;
- require exactly one real `sts2` assembly and require it to remain in the dedicated context;
- require exact private-context membership;
- require zero native load attempts and zero rejected/unplanned managed requests;
- record explicitly that no game entry point, game member reflection, method invocation, Godot initialization, or native game load was requested.

The loaded Step 23 game context is intentionally process-resident in production. Force-quit before rerunning Step 21/22 gates that require no real game assembly in the CLR.

## Acceptance required for Step 23 closure

Codemagic must pass static validation, host unit tests, Godot/native build/preflight, iOS publish, and IPA verification.

On device, from a fresh process:

1. confirm `STEP 23.1 — FIRST REAL STS2 CLR LOAD BOUNDARY`, version `0.0.66`;
2. run Step 23 A–D and stop at the first failure;
3. require Gate A module initializers = 0;
4. require Gate B first real `sts2.dll` CLR load = PASS;
5. require Gate C complete planned managed closure resolution = PASS with zero native attempts;
6. require Gate D = PASS and Step 23 summary 4/4;
7. run OfflineReady and require PASS;
8. run Foundation 5/5 and require PASS;
9. share `Reports/Step23-FirstRealGameLoad.txt` on any failure or unexpected diagnostic.

Only after this is green should controlled type/member access or broader managed initialization begin.

### Step 23.1 Codemagic host-test isolation correction

The first Step 23 Codemagic run reached the host test suite: static validation passed 187/187, Core compiled, and 153/154 tests passed. The sole failure was test-only collectible `AssemblyLoadContext` GC timing: the positive synthetic `sts2` load could remain visible in `AppDomain.GetAssemblies()` long enough for the following module-initializer preflight test to hit the fresh-process guard first. Step 23.1 clears the async helper's strong reference explicitly in `finally` and waits for the collectible synthetic `sts2` context to disappear, making host tests independent of test order and CI GC timing. Production Step 23 remains non-collectible and unchanged.
