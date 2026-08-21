# StS2 Launcher iOS — Step 25 Controlled Harmony API Resolution + Type Initialization + Instance Construction

This repository is the canonical launcher source after the physical closure of **Steps 01–24**.

Step 24.0.6 / `0.0.79 (79)` is physically closed on an iPhone: Gates A–D passed, OfflineReady passed afterward, and Foundation remained 5/5. Exact receipt-backed `0Harmony 2.4.2.0` can therefore enter the dedicated private CLR context and complete its module constructor under the strict managed-plan/native-refusal policy. The `System.Collections.Concurrent` preservation root that allowed the dynamically loaded MonoMod initialization path to reach the framework surface is now protected platform policy, separately classified from the exact 22 Step-22 direct framework roots.

## Active Step 25 boundary

Step 25 remains inside controlled managed initialization. It advances only from the closed Step 24 state through **targeted Harmony API resolution, explicit initialization of the exact measured `HarmonyLib.Harmony` type initializer, and construction of one inert `HarmonyLib.Harmony` object**.

The candidate is intentionally narrower than patching:

- **Gate A — InitializationPreflight:** replay the closed Step 23/24 metadata and initializer conditions, then inspect exact `HarmonyLib.Harmony` metadata with Cecil before any Step 25 real game/Harmony CLR load. Require the exact measured three-instruction `HarmonyLib.Harmony::.cctor` static-cache shape, exactly one public `.ctor(System.String)`, public `Id` getter, public static bool `DEBUG`, the expected `HARMONY_DEBUG` environment probe, and a `DEBUG=false` branch that excludes debug-only constructor work.
- **Gate B — ProvenLoadStateReplay:** recreate the physically proven Step 23 initializer-free private context; `0Harmony` remains absent.
- **Gate C — DeferredModuleInitialization:** replay the physically proven Step 24 `0Harmony` module-initialization boundary with `RuntimeHelpers.RunModuleConstructor`.
- **Gate D — ProvenInitializationAudit:** re-prove the complete closed Step 24 post-initialization state before any new Step 25 action.
- **Gate E — HarmonyApiResolution:** resolve only exact `HarmonyLib.Harmony`, its exact measured type initializer, public `.ctor(string)`, `Id`, and `DEBUG` metadata. Do not read `DEBUG`, execute the type initializer, or construct an object.
- **Gate F — HarmonyTypeInitialization:** explicitly execute only the exact Gate-A-measured `HarmonyLib.Harmony` type initializer with `RuntimeHelpers.RunClassConstructor`; require `HARMONY_DEBUG` absent, `Harmony.DEBUG == false`, unchanged bytes/context membership, and zero native/unplanned resolution.
- **Gate G — HarmonyTypeInitializationAudit:** re-audit exact hash/context/resolver state after type initialization and require `Harmony.DEBUG == false` before construction.
- **Gate H — HarmonyInstanceConstruction:** invoke only the exact public string constructor using fixed probe ID `com.community.sts2launcher.step25.probe`; verify object type/context/ID/DEBUG and require zero new native or unplanned managed resolution.
- **Gate I — PostConstructionAudit:** re-hash the plan and prepared/live bytes, re-prove OfflineReady, require unchanged private-context membership, and re-verify the retained Harmony object.

Step 25 does **not** call `Patch`, `PatchAll`, `PatchCategory`, `PatchAllUncategorized`, `CreateProcessor`, or other patch/processor APIs. It does not discover patch classes, reflect over or invoke StS2 game members, invoke the game entry point, start Godot/game state, or permit native game-library loading.

## Codemagic

Use workflow:

`ios-step-25`

Expected app version: `0.0.81 (81)`.

Expected IPA: `artifacts/StS2-Launcher-Step-25.ipa`.

## Documentation

Start with `docs/MASTER-PLAN.md` for durable architecture/roadmap rules and `docs/CURRENT-STATUS.md` for the active physical boundary. Historical evidence remains under `docs/history/steps/`; `history.zip` is reference-only and never a build dependency.
