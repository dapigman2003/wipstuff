# Step 25 — Controlled Harmony API Resolution + Type Initialization + Instance Construction

## Starting evidence

Physical Step 24.0.6 / `0.0.79 (79)` closed the controlled automatic-initialization boundary 4/4 and passed OfflineReady + Foundation 5/5. Exact `0Harmony 2.4.2.0` can therefore load and complete its module initializer on iPhone with the protected `System.Collections.Concurrent` preservation root, full trimming, `MtouchInterpreter=-all`, strict managed-plan resolution, and native-load refusal.

## Objective

Cross the smallest explicit Harmony managed-API boundary without patching anything and without reflecting over or invoking StS2 game members.

Step 25 resolves exactly `HarmonyLib.Harmony` from the already module-initialized receipt-backed `0Harmony` assembly, verifies its exact type-initializer/constructor/observation surface, explicitly completes only the measured Harmony type initializer, audits that state, then invokes only `HarmonyLib.Harmony::.ctor(System.String)` with a fixed launcher-owned probe ID.

## Gates

### Gate A — InitializationPreflight

Re-run the Step 24 metadata/input preconditions before any Step 25 CLR load. In addition to the accepted Step 24 initializer audit, metadata-only Cecil inspection requires:

- public non-abstract `HarmonyLib.Harmony` in exact `0Harmony 2.4.2.0`;
- exactly one managed `HarmonyLib.Harmony::.cctor` matching the measured three-instruction `ConditionalWeakTable<...>` construction → `AssemblyCachedCategories` store → `ret` shape;
- exactly one public instance constructor and it is `.ctor(System.String)`;
- public `Id : System.String` getter;
- public static `DEBUG : System.Boolean` field;
- constructor contains the expected `HARMONY_DEBUG` environment probe;
- debug-only work remains behind the `Harmony.DEBUG == false` branch;
- `HARMONY_DEBUG` is absent/empty before execution;
- no unbounded `calli`/function-pointer/jump or unexpected non-framework call is reachable in the DEBUG=false constructor path.

No Step 25 real game/Harmony assembly is loaded in Gate A.

### Gates B–D — Proven Step 24 replay

Gate B reproduces the Step 23 initializer-free private context.

Gate C admits exact `0Harmony 2.4.2.0` and completes `RuntimeHelpers.RunModuleConstructor` under the physically proven Step 24 policy.

Gate D re-proves the Step 24 post-initialization byte/plan/context/OfflineReady/native-isolation state.

These are closed prerequisite boundaries replayed in the exact context Step 25 will advance.

### Gate E — HarmonyApiResolution

Use targeted runtime reflection only on exact `0Harmony`:

- resolve exact `HarmonyLib.Harmony`;
- require the exact Gate-A-measured type initializer;
- require exactly one public `.ctor(System.String)`;
- resolve only the `Id` getter and `DEBUG` field needed for later verification;
- require `HARMONY_DEBUG` absent.

Gate E does not read `Harmony.DEBUG`, execute the type initializer, or construct a Harmony object.

### Gate F — HarmonyTypeInitialization

This is the first new managed execution boundary:

- re-hash exact prepared `0Harmony` immediately before execution;
- require `HARMONY_DEBUG` still absent;
- explicitly complete only `HarmonyLib.Harmony::.cctor` using `RuntimeHelpers.RunClassConstructor(HarmonyLib.Harmony.TypeHandle)`;
- require `Harmony.DEBUG == false`;
- require no native request, no rejected/unplanned managed request, and no private-context membership change;
- re-hash exact prepared `0Harmony` afterward.

No Harmony object is constructed yet.

### Gate G — HarmonyTypeInitializationAudit

Re-audit the exact post-type-initialization state before construction: target hash, closed-Step-24 private-context membership, `HARMONY_DEBUG` absence, `Harmony.DEBUG == false`, zero native attempts, and zero rejected/unplanned managed requests.

### Gate H — HarmonyInstanceConstruction

- invoke only exact `HarmonyLib.Harmony::.ctor(System.String)`;
- use fixed ID `com.community.sts2launcher.step25.probe`;
- require returned runtime type = exact `HarmonyLib.Harmony` in the Step 25 context;
- require `Harmony.Id` equals the fixed probe ID;
- require `Harmony.DEBUG` remains false;
- require no native request, no rejected/unplanned managed request, and no private-context membership change;
- re-hash exact prepared `0Harmony` afterward.

No `Patch`, `PatchAll`, `PatchCategory`, `CreateProcessor`, or other patch/processor API is invoked.

### Gate I — PostConstructionAudit

Re-hash the runtime plan and every prepared/live managed file, re-prove OfflineReady, require exact Step 24 private-context membership, re-verify the retained Harmony object/ID/DEBUG state, and require zero native/unplanned resolver events.

## Explicitly still out of scope

- Harmony patch/processor APIs;
- patch discovery or patch-class reflection;
- StS2 entry-point access;
- StS2 type/member reflection or invocation;
- broad `Activator`/`CreateInstance` or general-purpose reflection invocation;
- Godot/game initialization;
- native game-library loading;
- mutation of the trusted live install or prepared runtime.

## Candidate identity

- step: **25.0**
- version: **0.0.80 (80)**
- workflow: **`ios-step-25`**
- IPA: **`artifacts/StS2-Launcher-Step-25.ipa`**
- device report: `Documents/StS2Launcher/Reports/Step25-ControlledHarmonyConstruction.txt`
