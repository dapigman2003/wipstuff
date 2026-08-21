# Testing — Step 24 Controlled Managed Initialization Boundary

## Current principle

Text files are the primary handoff format for long diagnostics. Screenshots are useful for short UI state only. Ordered gates intentionally let one physical candidate test several adjacent questions without losing the first failing boundary.

## Static validation

```sh
bash scripts/validate.sh
```

Output:

`artifacts/reports/static-validation.txt`

The validator checks only authoritative current source/docs/tooling. It must **not** depend on `history.zip` or any legacy `StS2Launcher.Step05.iOS` path. It protects the closed Step 23 boundary while separately pinning the active Step 24 candidate.

## Host unit tests

```sh
bash scripts/test.sh
```

Output:

`artifacts/reports/host-unit-tests.txt`

Detailed test results remain under `artifacts/test-results/`.

Step 24 host tests use project-owned synthetic IL, unique synthetic assembly identities, and collectible load contexts. They cover ordered-gate behavior, a successful inert module initializer, Gate-A rejection of direct P/Invoke, implicitly type-initializer-reachable P/Invoke, and function-pointer/delegate indirection, plus Gate-C reporting when an initializer throws. Production Step 24 remains process-resident on device.

## Godot native preflight

On macOS/Xcode:

```sh
bash scripts/build-godot.sh
bash scripts/preflight-godot-link.sh
```

Shareable preflight output:

`artifacts/reports/godot-native-preflight.txt`

## iOS build

On macOS/Xcode with the pinned .NET SDK/workload available:

```sh
bash scripts/build-ios.sh
```

Expected IPA:

`artifacts/StS2-Launcher-Step-24.ipa`

## IPA verification

```sh
bash scripts/verify-ipa.sh artifacts/StS2-Launcher-Step-24.ipa
```

Output:

`artifacts/reports/ipa-verification.txt`

## Codemagic

Workflow:

`ios-step-24`

Authoritative entry point:

```sh
bash scripts/codemagic.sh
```

The pipeline runs static validation, host tests, iOS workload setup, Godot build/preflight, iOS publish, and final IPA verification. Build/CI never contains or loads the proprietary game payload; real Step 24 initialization occurs only from the user's receipt-backed on-device prepared runtime.

## Physical acceptance for Step 24

Build `0.0.73 (73)` is not a physical-test candidate: Codemagic rejected it at Core compilation before host tests. Build `0.0.74 (74)` is also not a physical-test candidate: it compiled and reached the full host suite, but two Step 24 Gate A P/Invoke safety tests failed (160/162), so no IPA was produced.

Build `0.0.75 (75)` reached the physical iPhone but failed safely in Gate A during metadata classification because Cecil attempted to resolve `GodotSharp`; no Step 24 CLR load occurred.

Install version `0.0.77 (77)` only after Codemagic host tests and IPA verification are fully green. Build 77 must retain Gate A's no-external-resolution rule while also proving the revised two-pass metadata behavior: shallow deferred whole-plan initializer classification first, then detailed closure traversal only for the exact `0Harmony` target. An actually reachable `GodotSharp` call must fail as an explicit prohibited edge with audited IL; any Cecil resolver attempt must fail with the exact prepared path/stage rather than widening the metadata environment. Then start from a fresh process. Do not run the Step 23 load regression or start the Step 15 Godot host first.

Run Step 24 A–D in order and require:

1. **Gate A — InitializationPreflight = PASS**
   - accepted Step 23 Gate A replay still passes;
   - OfflineReady and runtime plan depot/manifest match;
   - primary remains initializer-free;
   - exactly one initializer-bearing dependency exists;
   - exact target is `0Harmony, Version=2.4.2.0, Culture=neutral, PublicKeyToken=null`;
   - exactly one `<Module>..cctor`;
   - bounded same-assembly automatic-initialization closure (including implicitly triggerable type constructors) is fully measured and hazards = 0;
   - no Step 24 real game/Harmony CLR load yet.
2. **Gate B — ProvenLoadStateReplay = PASS**
   - one dedicated Step 24 private context is created;
   - the accepted Step 23 initializer-free private closure is reproduced exactly;
   - planned host bindings resolve exactly;
   - `0Harmony` remains outside the CLR;
   - native attempts = 0 and rejected/unplanned managed requests = 0.
3. **Gate C — DeferredModuleInitialization = PASS**
   - exact prepared `0Harmony` hash matches immediately before load;
   - only that initializer-bearing identity is admitted;
   - exact target loads in the Step 24 context;
   - `RuntimeHelpers.RunModuleConstructor(targetAssembly.ManifestModule.ModuleHandle)` completes;
   - native attempts = 0;
   - rejected/unplanned managed requests = 0;
   - no other initializer-bearing private assembly enters;
   - prepared target hash remains unchanged after initialization.
4. **Gate D — PostInitializationAudit = PASS**
   - persisted plan hash is unchanged;
   - every prepared and receipt-backed live managed file re-hashes correctly;
   - OfflineReady re-proves;
   - private context equals the accepted Step 23 inert closure plus exactly `0Harmony`;
   - native attempts = 0;
   - rejected/unplanned managed requests = 0;
   - explicit Harmony patching/game invocation/Godot startup/native game loading remain NO.
5. Step 24 summary = **4/4**.
6. OfflineReady regression = **PASS**.
7. Foundation regression = **5/5 PASS**.

Share `Reports/Step24-ControlledManagedInitialization.txt` on any failure. Stop at the first failing gate. Once Gate B has loaded the real game context, force-quit before rerunning fresh-process Step 21/22/23 regressions or Step 24 itself.
