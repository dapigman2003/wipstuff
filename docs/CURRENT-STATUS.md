# Current Status — Step 25 Controlled Harmony API Resolution + Type Initialization + Instance Construction

## Physically closed boundary

**Steps 01–24 are closed on a physical iPhone.**

The latest closed runtime boundary is **Step 24.0.6 / 0.0.79 (79)**.

Physical Step 24.0.6 evidence reported by the user:

- Gate A — InitializationPreflight: **PASS**;
- Gate B — ProvenLoadStateReplay: **PASS**;
- Gate C — DeferredModuleInitialization: **PASS**;
- Gate D — PostInitializationAudit: **PASS**;
- Step 24 summary: **4/4 PASS**;
- OfflineReady after Step 24: **PASS**;
- Foundation after Step 24: **5/5 PASS**.

This closes the controlled automatic-initialization boundary. Exact receipt-backed `0Harmony 2.4.2.0` can enter the dedicated private iPhone load context and complete its module constructor while strict managed-plan resolution/native refusal remains intact.

The Step 24 `System.Collections.Concurrent` preservation root is now **physically proven platform policy**, separately classified from the exact 22 Step-22 direct framework roots.

## Active candidate — Step 25

Step 25.0.1 / `0.0.81 (81)` passed Codemagic compilation/host testing and reached a physical iPhone. The physical run advanced **7/9**: Gates A–G passed, Gate H failed at the exact `HarmonyLib.Harmony::.ctor(System.String)` invocation with `MissingMethodException: System.Environment.get_Version()`, and Gate I did not run. This physically establishes targeted Harmony API resolution and the explicit `HarmonyLib.Harmony` type-initialization boundary under the current strict context. Step 25 itself remains open because instance construction did not complete.

The measured constructor places `Environment.Version` inside the `Harmony.DEBUG` branch, while Gates F/G physically established `Harmony.DEBUG == false` and no `HARMONY_DEBUG` activation. The conservative diagnosis is therefore trim survival of framework tokens referenced by the exact post-publish constructor IL, not intentional execution of the debug branch. Step 25.0.2 / `0.0.82 (82)` adds a bounded candidate-only `DynamicDependency` preservation anchor for the exact framework types referenced by that measured constructor while leaving the A–I runtime code unchanged.

- Step: **25.0.2**
- Version: **0.0.82 (82)**
- Codemagic workflow: **`ios-step-25`**
- IPA: **`artifacts/StS2-Launcher-Step-25.ipa`**
- Device report: `Documents/StS2Launcher/Reports/Step25-ControlledHarmonyConstruction.txt`
- Trusted source: Step 12 receipt-backed managed install
- Execution input: physically proven Step 21/22 prepared runtime + binding plan
- Closed prerequisites: Step 23.4.3 + Step 24.0.6 + OfflineReady + Foundation 5/5
- Physically established within Step 25: **Gates A–G on 0.0.81**
- Open Step 25 frontier: **Gate H constructor completion → Gate I post-construction audit**
- Candidate-only build change: bounded `DynamicDependency` preservation for the exact measured `Harmony(string)` framework surface

Step 25 still does not attempt patching. It advances only through exact Harmony API resolution, explicit execution of the exact measured `HarmonyLib.Harmony` type initializer, and construction of one inert `HarmonyLib.Harmony` object.

### Gate A — InitializationPreflight

Metadata-only, before any Step 25 real game/Harmony CLR load:

1. require a fresh process;
2. replay the accepted Step 23/24 input and initializer preconditions;
3. require exact sole initializer-bearing target = `0Harmony 2.4.2.0`;
4. retain the exact physically measured Step 24 initializer policy;
5. inspect exact `HarmonyLib.Harmony` metadata with a rejecting Cecil resolver;
6. require exactly one managed `HarmonyLib.Harmony::.cctor` matching the measured three-instruction `ConditionalWeakTable<...>` → `AssemblyCachedCategories` cache setup;
7. require exactly one public `.ctor(System.String)`, public string `Id` getter, and public static bool `DEBUG`;
8. require the expected `HARMONY_DEBUG` probe and `DEBUG=false` guard around debug-only constructor work;
9. require `HARMONY_DEBUG` absent/empty;
10. reject unbounded indirect execution and unexpected non-framework calls in the DEBUG=false constructor path.

### Gate B — ProvenLoadStateReplay

Reproduce the physically proven Step 23 initializer-free state in the Step 25 private context. `0Harmony` remains absent.

### Gate C — DeferredModuleInitialization

Replay the physically proven Step 24 module-initialization boundary: load exact `0Harmony` and complete `RuntimeHelpers.RunModuleConstructor`. The proven `System.Collections.Concurrent` root remains active; native/unplanned resolution remains fail-closed.

### Gate D — ProvenInitializationAudit

Re-prove the closed Step 24 post-initialization state: exact context membership, unchanged plan/prepared/live hashes, OfflineReady, zero native attempts, zero rejected/unplanned managed requests.

### Gate E — HarmonyApiResolution

Targeted runtime reflection only:

- exact assembly = `0Harmony 2.4.2.0` in the Step 25 context;
- exact type = `HarmonyLib.Harmony`;
- exact type initializer = the Gate-A-measured `.cctor`;
- exact public `.ctor(System.String)`;
- exact observation members = `Id` getter + `DEBUG` field;
- `HARMONY_DEBUG` absent.

Gate E does **not** read `Harmony.DEBUG`, execute the type initializer, or construct a Harmony object.

### Gate F — HarmonyTypeInitialization

Cross only the Harmony type-initialization boundary:

- re-hash exact prepared `0Harmony` immediately before execution;
- require `HARMONY_DEBUG` still absent;
- execute `RuntimeHelpers.RunClassConstructor(HarmonyLib.Harmony.TypeHandle)`;
- require `Harmony.DEBUG == false`;
- require unchanged context membership and exact target hash;
- require zero native attempts and zero rejected/unplanned managed requests.

No Harmony object is constructed in Gate F.

### Gate G — HarmonyTypeInitializationAudit

Re-audit the post-type-initialization state before construction: exact `0Harmony` hash, exact closed-Step-24 context membership, `HARMONY_DEBUG` absent, `Harmony.DEBUG == false`, zero native attempts, and zero rejected/unplanned managed requests.

### Gate H — HarmonyInstanceConstruction

Invoke only exact `HarmonyLib.Harmony::.ctor(System.String)` using fixed probe ID:

`com.community.sts2launcher.step25.probe`

Require exact returned type/context, exact `Harmony.Id`, `Harmony.DEBUG == false`, unchanged context membership, unchanged target hash, zero native attempts, and zero rejected/unplanned managed requests.

### Gate I — PostConstructionAudit

Re-hash the runtime plan and every prepared/live file, re-prove OfflineReady, require unchanged Step 24 context membership, and re-verify the retained Harmony object identity/ID/DEBUG state.

## Still forbidden

- `Harmony.Patch`, `PatchAll`, `PatchCategory`, `PatchAllUncategorized`, `CreateProcessor`, or other patch/processor APIs;
- patch-class discovery/reflection;
- StS2 entry-point/type/member reflection or invocation;
- broad `Activator`/`CreateInstance` or general reflection invocation;
- Godot/game startup;
- native game-library loading;
- mutation of the trusted live install/prepared runtime.

## Acceptance required for Step 25 closure

From a fresh process:

1. Codemagic static validation + host tests + iOS publish + IPA verification = PASS;
2. install `0.0.82 (82)`;
3. run Step 25 A–I and stop at the first failure;
4. require summary **9/9 PASS**;
5. run OfflineReady = **PASS**;
6. run Foundation = **5/5 PASS**.

After Gate B, the real managed game/Harmony context remains process-resident. Force-quit before rerunning fresh-process Step 21/22/23/24 regressions.

## Next frontier if Step 25 closes

Do not jump directly to broad `PatchAll`. Use the physical Step 25 result plus exact Harmony metadata to choose the smallest patch-engine/API boundary. Candidate next work should separate patch-object/processor creation, target-method reflection, and actual method replacement into distinct gates wherever possible.
