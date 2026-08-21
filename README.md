# StS2 Launcher iOS — Step 24 Controlled 0Harmony Module Initialization Boundary

This repository is the canonical launcher source after the physical closure of Step 23.4.3.

Step 23 is now proven on a physical iPhone: all four first-real-load gates passed, OfflineReady passed afterward, and Foundation remained 5/5. The real receipt-backed `sts2.dll` plus the maximal initializer-free prepared managed closure can therefore enter the dedicated private CLR load context while initializer-bearing dependencies remain excluded.

## Active Step 24 boundary

Step 24 advances exactly to the sole deferred automatic-execution boundary observed in Step 23:

`0Harmony, Version=2.4.2.0, Culture=neutral, PublicKeyToken=null`

The candidate remains intentionally narrower than “use Harmony”:

- Gate A replays the accepted Step 23 preflight and requires exactly one initializer-bearing dependency, exactly `0Harmony 2.4.2.0` with one `<Module>..cctor`.
- A bounded Cecil same-assembly automatic-initialization audit runs before any Step 24 CLR load. It follows direct same-assembly calls plus implicitly triggerable same-assembly type constructors and resolves local calls only from metadata already present in the audited module. The raw audit remains conservative for P/Invoke, `calli`, function/delegate indirection, bodyless dispatch, native-library APIs, explicit runtime-constructor APIs, reflection/dynamic invocation, unresolved local calls, and unexpected non-framework execution edges. Step 24.0.5 may downgrade only the exact seven physically measured MonoMod logging dispatch findings to conditionally dormant when the exact measured initializer shape is unchanged and no debugger, `MONOMOD_*` environment override, or relevant MonoMod logging AppContext override is present; every other finding remains blocking.
- Gate B recreates the physically proven Step 23 initializer-free private context in the same Step 24 load context.
- Gate C admits exactly `0Harmony`, loads the receipt-hashed prepared bytes, and calls `RuntimeHelpers.RunModuleConstructor` as the explicit completion barrier for the module constructor.
- Step 24.0.6 additionally roots only `System.Collections.Concurrent` in the iOS host because physical build 78 proved that the dynamically loaded MonoMod initializer reached a framework constructor removed from the fully trimmed host. The exact Step 22 22-root set remains intact; this is a separate unproven Step 24 preservation root.
- The strict private resolver still rejects all native-library resolution, unplanned managed loads, and any untargeted initializer-bearing dependency.
- Gate D re-hashes the runtime plan and every prepared/live file, re-proves OfflineReady, and requires the private context to equal the Step 23 closure plus exactly `0Harmony`.

Step 24 does **not** call Harmony patch APIs, inspect or invoke StS2 game types/members, invoke a game entry point, start Godot/game state, or permit native game-library loading.

Step 24.0 / `0.0.73 (73)` was rejected by Codemagic at Core compilation before host tests because the new subsystem referenced the wrong OfflineReady inspection method. Step 24.0.1 / `0.0.74 (74)` compiled and ran the full host suite, where two Gate A safety tests exposed that reachable same-assembly P/Invoke stubs were skipped because they have no managed method body. Step 24.0.2 / `0.0.75 (75)` corrected that issue and reached a physical iPhone, where Gate A failed safely during prepared-target classification because Cecil attempted to resolve `GodotSharp`. Step 24.0.3 / `0.0.76 (76)` removed the explicit `MethodReference.Resolve()` path but physically repeated the same Gate A `GodotSharp` resolver failure. Step 24.0.4 / `0.0.77 (77)` eliminated that resolver problem and physically exposed the real target closure: exactly seven conservative MonoMod logging dispatch findings and four automatic initializers; Gate A correctly stopped before Gate B. Step 24.0.5 / `0.0.78 (78)` then passed Gates A and B on-device and reached the real Gate C module-constructor boundary. `0Harmony` loaded and `<Module>..cctor` began, but `MonoMod.Logs.DebugLog::.cctor` failed with a missing `System.Collections.Concurrent.ConcurrentBag<T>` parameterless constructor in the fully trimmed iOS host. The active Step 24.0.6 / `0.0.79 (79)` retains the exact runtime gates and adds only one candidate-only build root, `System.Collections.Concurrent`, so the post-publish MonoMod IL can reach that framework surface without disabling full trimming or changing `MtouchInterpreter=-all`.

## Codemagic

Use workflow:

`ios-step-24`

Expected app version: `0.0.79 (79)`.

## Documentation

Start with `docs/MASTER-PLAN.md` for durable architecture/roadmap rules and `docs/CURRENT-STATUS.md` for the active physical boundary. Historical evidence remains under `docs/history/steps/`; `history.zip` is reference-only and never a build dependency.
