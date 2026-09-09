# Step 46.1–50.0 — execution-aware direct main-menu ladder

## Physical authority entering this candidate

Physical 0.0.178 proved Step 45 complete because Step 46 Gate A accepted same-process Step-45 4/4 authority with retained real NGame/state 2, rendering frozen, and zero post-Step-45 resolver/host/private/initializer/rejected/native drift. Step 46 Gate B then located exact `NGame.LaunchMainMenu(bool)` token `0x06001BDF`, async state machine `NGame/<LaunchMainMenu>d__131`, MoveNext token `0x06001C3D`, and 336 MoveNext IL instructions with zero external Cecil resolution.

The old Step-46 Gate-C audit failed safely before invocation. Its transitive graph followed every method-reference operand, including function-pointer/delegate targets that are registered for later menu/combat/multiplayer events. That mechanically pulled dormant Steam, multiplayer, SaveManager fallback, and Spine paths into the closure alongside the genuinely immediate `LoadDeferredStartupAssetsAsync -> ExecuteDeferred/PrewarmJit` and Steam command-line join frontiers. `LaunchMainMenu` was not invoked and rendering was not restarted.

## Design correction

Do not approve the polluted closure and do not weaken Steam/Spine/native policy. Instead:

1. **Step 46.1 — immediate invocation frontier map.** Traverse only IL opcodes that execute a call/constructor immediately (`call`, `callvirt`, `newobj`, `jmp`). Record `ldftn`, `ldvirtftn`, `ldtoken`, and other non-invoking method references as deferred frontiers without recursively treating them as execution. Recursively expand compiler async/iterator state machines only when their wrapper methods are reached through an immediate edge. The map is evidence only; original `LaunchMainMenu` remains forbidden and no admissibility flag unlocks it.
2. **Step 47.0 — exact main-menu resource preparation.** Extract exact receipt-backed `res://scenes/screens/main_menu.tscn` and `res://scenes/backgrounds/main_menu_bg.tscn` from the closed PCK. Require the sealed byte counts/hashes. Build a deterministic private Spine-neutral derivative of the background only; never mutate the trusted PCK/install. Load the derivative privately and take over the background resource path in Godot's resource cache, then load the exact original main-menu PackedScene. The durable static map is written before a UI/Core one-shot arm; once cache takeover/resource loading is armed, the rung cannot be retried in the same process even after failure. No instantiation or rendering.
3. **Step 48.0 — off-tree real NMainMenu instantiation.** Instantiate the retained exact main-menu PackedScene once while rendering remains frozen. Require exact `NMainMenu`, private selected load-context authority, `IsInsideTree=false`, and build an execution-qualified actual hierarchy/lifecycle frontier map before any SceneTree insertion.
4. **Step 49.0 — frozen SceneTree admission.** Resolve exact real `NGame.RootSceneContainer`, durably record exact `AddChild` binding/child count, then add the already-audited NMainMenu once with rendering still stopped. Require exact parent, child-count +1, retained NGame/state authority, and zero initializer/rejected/native escape.
5. **Step 50.0 — controlled real-menu render pulse.** Enumerate actual in-tree main-menu frame/input callbacks and audit their immediate execution frontiers. After the map is durable, start rendering exactly once, request the proven 100 ms pulse, synchronously stop at the first managed continuation before telemetry/file I/O, and require frozen post-pulse confinement. Always leave rendering stopped.

## Still forbidden

Whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, external Steamworks/native Steam, FMOD/Spine native game extensions, explicit `_ExitTree`, `RemoveChild`/`Free`, state reset, and mutation of the trusted Step-12 install remain forbidden unless a later candidate separately authorizes an exact boundary.
