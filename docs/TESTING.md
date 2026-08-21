# Testing — Step 25 Controlled Harmony API Resolution + Type Initialization + Instance Construction

## Current principle

Physical iPhone evidence remains authoritative. Static validation and host tests must prove that the candidate cannot silently broaden the boundary before the IPA is allowed to reach the phone.

The validator checks only authoritative current source/docs/tooling. It must **not** depend on `history.zip` or any legacy `StS2Launcher.Step05.iOS` path. It protects the physically closed Step 23 load boundary and Step 24 initialization boundary while separately pinning the active Step 25 candidate.

## Local/static validation

Run:

`bash scripts/validate.sh`

The validator must prove, among other current invariants:

- canonical iOS project/version/release wiring is Step 25 / `0.0.80 (80)`;
- full trimming and `MtouchInterpreter=-all` remain unchanged;
- the exact Step-22 22-root set remains protected;
- `System.Collections.Concurrent` remains exactly one separately classified, physically proven dynamic-IL preservation root;
- Step 23.4.3 and Step 24.0.6 protected implementations remain hash-pinned;
- the active Step 25 implementation is separately candidate-hash-pinned;
- Step 25 has exactly nine ordered fail-fast gates;
- Gate A is metadata-only and requires the exact measured Harmony type-initializer shape, exact constructor shape, and inert `HARMONY_DEBUG`/`DEBUG=false` state;
- Gates B–D replay the physically closed Step 23/24 state before the new boundary;
- Gate E performs only targeted API/type-initializer/constructor/member resolution without reading `DEBUG` or executing the type initializer;
- Gate F explicitly completes only the exact measured Harmony type initializer with `RuntimeHelpers.RunClassConstructor`;
- Gate G re-audits the post-type-initialization state and requires `Harmony.DEBUG == false`;
- Gate H invokes only exact `HarmonyLib.Harmony::.ctor(System.String)` with the fixed probe ID;
- Gate I re-proves byte/plan/OfflineReady/context/object state;
- patch/processor APIs, StS2 game reflection/invocation, Godot/game startup, native game loading, and prepared/live byte mutation remain forbidden.

## Host tests

Run:

`bash scripts/test.sh`

Step 25 host tests use project-owned synthetic IL, unique synthetic assembly identities, and collectible load contexts. They must retain the Step 24 initializer safety regressions and additionally cover:

- ordered A→I success and fail-fast gate sequencing;
- synthetic Step 24 replay followed by exact Harmony API resolution, explicit Harmony type initialization, and one successful inert construction;
- rejection of direct/reachable P/Invoke and implicit type-initializer P/Invoke;
- rejection of generic function-pointer/delegate indirection outside the exact physically measured Step 24 conditional fingerprint;
- no Cecil external-resolution regression for nominally local metadata;
- exact conditional MonoMod initializer fingerprint/state requirements;
- reporting/stopping when a module initializer throws;
- exact `HarmonyLib.Harmony` constructor/API metadata shape used by Step 25.

The host suite cannot prove iOS AOT/linker/reflection behavior; it is a prerequisite, not physical closure.

## Codemagic

Use exactly:

`ios-step-25`

The pipeline runs static validation, host tests, iOS workload setup, Godot build/preflight, iOS publish, and final IPA verification. Build/CI never contains or loads the proprietary game payload; real Step 25 behavior occurs only from the user's receipt-backed on-device prepared runtime.

Expected release:

- version: `0.0.80 (80)`;
- IPA: `artifacts/StS2-Launcher-Step-25.ipa`;
- host TRX: `artifacts/test-results/step25.trx`.

## Physical acceptance for Step 25

Start from a fresh process. Do not run Step 21/22/23/24 fresh-process regressions or start the Godot host first.

Run Step 25 A–I in order and require:

1. **Gate A — InitializationPreflight = PASS**
   - accepted Step 23/24 metadata and initializer preconditions still pass;
   - exact sole initializer-bearing target remains `0Harmony 2.4.2.0`;
   - exact physically proven Step 24 conditional MonoMod initializer policy still passes;
   - exact `HarmonyLib.Harmony` metadata exists with exactly one managed type initializer matching the measured three-instruction `ConditionalWeakTable<...>` → `AssemblyCachedCategories` shape;
   - exactly one public `.ctor(System.String)`, public string `Id` getter, and public static bool `DEBUG` exist;
   - exact constructor metadata contains the expected `HARMONY_DEBUG` environment probe and a `DEBUG=false` guard around debug-only work;
   - `HARMONY_DEBUG` is absent/empty;
   - no blocking indirect/native/reflection/dynamic/unexpected non-framework constructor edge exists outside the measured debug-only branch;
   - no real game/Harmony CLR load has occurred yet.
2. **Gate B — ProvenLoadStateReplay = PASS**
   - exact physically proven Step 23 initializer-free context is reproduced;
   - `0Harmony` remains absent;
   - native attempts and rejected/unplanned managed requests remain zero.
3. **Gate C — DeferredModuleInitialization = PASS**
   - exact `0Harmony` loads and `RuntimeHelpers.RunModuleConstructor` completes under the physically proven Step 24 policy;
   - the proven `System.Collections.Concurrent` preservation root remains active;
   - native attempts and rejected/unplanned managed requests remain zero.
4. **Gate D — ProvenInitializationAudit = PASS**
   - exact physically closed Step 24 post-initialization context/byte/plan/OfflineReady state is reproduced before the new Step 25 action.
5. **Gate E — HarmonyApiResolution = PASS**
   - exact runtime `HarmonyLib.Harmony` is resolved from exact `0Harmony` in the dedicated context;
   - the exact Gate-A-measured type initializer resolves;
   - exact `.ctor(string)`, `Id`, and `DEBUG` observation surface resolves;
   - `HARMONY_DEBUG` remains absent;
   - `Harmony.DEBUG` is not read, the type initializer is not executed, and no Harmony object is constructed.
6. **Gate F — HarmonyTypeInitialization = PASS**
   - exact prepared `0Harmony` hash is rechecked immediately before execution;
   - `RuntimeHelpers.RunClassConstructor(HarmonyLib.Harmony.TypeHandle)` completes;
   - `Harmony.DEBUG == false`;
   - target hash and private-context membership remain unchanged;
   - zero native attempts and zero rejected/unplanned managed requests occur across type initialization;
   - no Harmony object is constructed.
7. **Gate G — HarmonyTypeInitializationAudit = PASS**
   - exact `0Harmony` hash and closed-Step-24 context membership remain unchanged;
   - `HARMONY_DEBUG` remains absent and `Harmony.DEBUG == false`;
   - zero native attempts and zero rejected/unplanned managed requests remain.
8. **Gate H — HarmonyInstanceConstruction = PASS**
   - invoke only the exact `.ctor(System.String)` with probe ID `com.community.sts2launcher.step25.probe`;
   - returned object has exact type/context and exact `Id`;
   - `Harmony.DEBUG == false` afterward;
   - target hash and private-context membership remain unchanged;
   - zero native attempts and zero rejected/unplanned managed requests occur across construction.
9. **Gate I — PostConstructionAudit = PASS**
   - plan/prepared/live hashes remain exact;
   - OfflineReady re-proves;
   - private context remains identical to the closed Step 24 context;
   - retained Harmony object type/ID/DEBUG state remains exact;
   - no patch/processor/game/Godot/native boundary was crossed.
10. Step 25 summary = **9/9 PASS**.
11. OfflineReady regression = **PASS**.
12. Foundation regression = **5/5 PASS**.

Share `Reports/Step25-ControlledHarmonyConstruction.txt` on any failure and stop at the first failed gate. Once Gate B has loaded the real managed context, force-quit before rerunning fresh-process Step 21/22/23/24 regressions or Step 25 itself.
