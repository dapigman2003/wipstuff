# Step 24 — Controlled 0Harmony Module Initialization Boundary

## Starting evidence

Physical Step 23.4.3 closed the load-only boundary 4/4 and passed OfflineReady + Foundation 5/5. Step 23 identified exactly one deferred initializer-bearing dependency:

`0Harmony, Version=2.4.2.0, Culture=neutral, PublicKeyToken=null`

with one `<Module>..cctor`.

## Objective

Cross the automatic module-initialization boundary without yet invoking Harmony patch APIs or StS2 game members.

## Gates

### Gate A — InitializationPreflight

Replay accepted Step 23 Gate A, require exactly the known `0Harmony 2.4.2.0` module-initializer target, and perform a bounded Cecil same-assembly automatic-initialization audit. Follow direct calls plus same-assembly type constructors that static calls/fields could implicitly trigger. Reject P/Invoke, `calli`, function/delegate indirection, native-loader APIs, explicit runtime-constructor APIs, reflection/dynamic invocation, or unexpected non-framework execution edges before any Step 24 load.

### Gate B — ProvenLoadStateReplay

Recreate the physically proven Step 23 initializer-free private context in the same Step 24 `AssemblyLoadContext`. `0Harmony` must remain absent.

### Gate C — DeferredModuleInitialization

Permit exactly `0Harmony`, load its exact prepared bytes, and use `RuntimeHelpers.RunModuleConstructor` as an explicit completion barrier. Native and unplanned managed loads remain fail-closed. No Harmony patch method is called.

### Gate D — PostInitializationAudit

Re-hash the plan and every prepared/live file, re-prove OfflineReady, and require exact context membership: Step 23 initializer-free closure plus exactly `0Harmony`. Native attempts and rejected/unplanned managed requests must remain zero.

## Candidate lineage

- Step 24.0 / `0.0.73 (73)`: Codemagic static validation passed 281/281, but Core compilation failed before host tests because `ControlledManagedInitialization` referenced nonexistent `SteamOfflineInstallInspection.InspectAsync` / `OfflineReady` members. No IPA or physical Step 24 evidence exists for build 73.
- Step 24.0.1 / `0.0.74 (74)`: compile-only correction to the established OfflineReady `RunAsync` result contract. Codemagic then compiled successfully and ran 162 host tests; 160 passed and two Gate A safety tests failed because reachable P/Invoke stubs with no managed IL body were not inspected after same-assembly resolution. No IPA or physical Step 24 evidence exists for build 74.
- Step 24.0.2 / `0.0.75 (75)`: production metadata-audit correction. Gate A inspects a resolved same-assembly target before the managed-body traversal filter, rejects reachable P/Invoke stubs explicitly, and fails closed on any other reachable same-assembly target without managed IL.
- Step 24.0.3 / `0.0.76 (76)`: removed the explicit `MethodReference.Resolve()` path, but physical testing repeated the same 0/4 Gate A `GodotSharp` `AssemblyResolutionException` at broad prepared-target classification. This proved the previous diagnosis was incomplete; no Step 24 CLR load occurred.
- Step 24.0.4 / `0.0.77 (77)`: active metadata-containment correction. Whole-plan initializer classification is shallow + deferred; only the exact `0Harmony` target receives detailed call-closure traversal; Cecil assembly/metadata resolution is explicitly rejected; method-reference `LookupToken` is removed; and failures preserve exact file/stage plus full exception diagnostics.

## Candidate identity

- active corrected version: `0.0.77 (77)`
- workflow: `ios-step-24`
- IPA: `artifacts/StS2-Launcher-Step-24.ipa`
- device report: `Documents/StS2Launcher/Reports/Step24-ControlledManagedInitialization.txt`

## Explicitly still out of scope

- `Harmony` construction or patch APIs;
- StS2 entry-point access;
- StS2 type/member reflection or invocation;
- `Activator`/broad dynamic reflection as a game execution mechanism;
- Godot/game initialization;
- native game-library loading;
- mutation of the trusted live install or prepared runtime.
