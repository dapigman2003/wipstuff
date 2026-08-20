# StS2 Launcher iOS — Step 24 Controlled 0Harmony Module Initialization Boundary

This repository is the canonical launcher source after the physical closure of Step 23.4.3.

Step 23 is now proven on a physical iPhone: all four first-real-load gates passed, OfflineReady passed afterward, and Foundation remained 5/5. The real receipt-backed `sts2.dll` plus the maximal initializer-free prepared managed closure can therefore enter the dedicated private CLR load context while initializer-bearing dependencies remain excluded.

## Active Step 24 boundary

Step 24 advances exactly to the sole deferred automatic-execution boundary observed in Step 23:

`0Harmony, Version=2.4.2.0, Culture=neutral, PublicKeyToken=null`

The candidate remains intentionally narrower than “use Harmony”:

- Gate A replays the accepted Step 23 preflight and requires exactly one initializer-bearing dependency, exactly `0Harmony 2.4.2.0` with one `<Module>..cctor`.
- A bounded Cecil same-assembly automatic-initialization audit runs before any Step 24 CLR load. It follows direct same-assembly calls plus implicitly triggerable same-assembly type constructors, and rejects P/Invoke, `calli`, function/delegate indirection, native-library APIs, explicit runtime-constructor APIs, reflection/dynamic invocation, and unexpected non-framework execution edges.
- Gate B recreates the physically proven Step 23 initializer-free private context in the same Step 24 load context.
- Gate C admits exactly `0Harmony`, loads the receipt-hashed prepared bytes, and calls `RuntimeHelpers.RunModuleConstructor` as the explicit completion barrier for the module constructor.
- The strict private resolver still rejects all native-library resolution, unplanned managed loads, and any untargeted initializer-bearing dependency.
- Gate D re-hashes the runtime plan and every prepared/live file, re-proves OfflineReady, and requires the private context to equal the Step 23 closure plus exactly `0Harmony`.

Step 24 does **not** call Harmony patch APIs, inspect or invoke StS2 game types/members, invoke a game entry point, start Godot/game state, or permit native game-library loading.

Step 24.0 / `0.0.73 (73)` was rejected by Codemagic at Core compilation before host tests because the new subsystem referenced the wrong OfflineReady inspection method. The active `0.0.74 (74)` candidate changes only that API call/result check; the Step 24 experiment itself is unchanged.

## Codemagic

Use workflow:

`ios-step-24`

Expected app version: `0.0.74 (74)`.

## Documentation

Start with `docs/MASTER-PLAN.md` for durable architecture/roadmap rules and `docs/CURRENT-STATUS.md` for the active physical boundary. Historical evidence remains under `docs/history/steps/`; `history.zip` is reference-only and never a build dependency.
