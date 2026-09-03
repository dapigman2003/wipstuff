# Step 35.0.23 — Godot managed-plugin bridge bootstrap

Release: 0.0.146 (146)
Status: diagnostic candidate; exact Step 35 remains OPEN.

## Trigger

Physical 0.0.145 converted the 0.0.144 GS035 hard termination into a deterministic lifecycle verdict: CSharpLanguage exists, but GDMonoCache::godot_api_cache_updated is false, ScriptManagerBridge_CreateManagedForGodotObjectBinding is absent, reverseBindingReady is false, and Godot's own runtime_initialized state is false. Gate C was intentionally not invoked.

The shipped sts2 assembly contains the Godot-generated `GodotPlugins.Game.Main.InitializeFromGameProject` unmanaged entry-point contract (`godotsharp_game_main_init`). Static inspection shows that generated bootstrap initializes GodotSharp NativeFuncs, creates the full `Godot.Bridge.ManagedCallbacks` table, and registers scripts from the game assembly. Godot 4.5.1 then copies that complete struct through `GDMonoCache::update_godot_api_cache` and calls `GD_OnCoreApiAssemblyLoaded`.

## Experiment

CORE-HANDOFF keeps the already proven Step-15 live-engine prerequisite and the exact 1,800-byte / 225-pointer source-built runtime interop table.

After NativeFuncs.Initialize succeeds, the launcher:
1. verifies the admitted diagnostic sts2 clone still exposes the generated `GodotPlugins.Game.Main.InitializeFromGameProject` + `UnmanagedCallersOnly(EntryPoint="godotsharp_game_main_init")` contract;
2. verifies private GodotSharp `ManagedCallbacks` contains exactly 37 unmanaged function-pointer fields and the required create-binding/core-API callbacks;
3. calls GodotSharp `ManagedCallbacks.Create(IntPtr)` into temporary launcher-owned native memory and requires all 37 pointers to be non-null;
4. calls `ScriptManagerBridge.LookupScriptsInAssembly` on the already admitted diagnostic sts2 assembly;
5. copies the complete produced callback struct into source-built Godot's `GDMonoCache` through a project-owned native export that performs the same `update_godot_api_cache` operation as normal Godot initialization;
6. requires reverse-binding readiness to become true while `GDMono::is_runtime_initialized()` remains false;
7. invokes the standard `GD_OnCoreApiAssemblyLoaded` callback as its own durable physical boundary;
8. proceeds to the unchanged NATURAL diagnostic ExecuteVeryEarly path only if every bootstrap boundary returns.

## Non-authority / prohibitions

- No second CLR / hostfxr / CoreCLR instance is started by the launcher.
- No game native executable or game NativeAOT image is loaded.
- No individual managed callback pointer is fabricated or substituted.
- The launcher does not write or fake `GDMono::runtime_initialized` / `initialized` ownership flags.
- The native cache-adoption export accepts only the complete exact-size `GDMonoCache::ManagedCallbacks` struct and is single-shot.
- Initializer-bearing 0Harmony remains forbidden.
- ExecuteEssential, ExecuteDeferred, game entry point, arbitrary resolver fallback, and broader game startup remain forbidden.
- A diagnostic 4/4 still cannot close exact Step 35.

## Expected high-value physical checkpoints

`CB_REVERSE_BINDING_STATE_BEFORE`
→ `CB_GAME_PLUGIN_ENTRY_CONTRACT_PASS`
→ `CB_MANAGED_CALLBACKS_BIND_PASS`
→ `CB_MANAGED_CALLBACKS_CREATE_START`
→ `CB_MANAGED_CALLBACKS_CREATE_RETURNED`
→ `CB_MANAGED_CALLBACKS_CREATE_PASS`
→ `CB_SCRIPT_LOOKUP_START`
→ `CB_SCRIPT_LOOKUP_RETURNED`
→ `CB_REVERSE_PREP_PASS`
→ `CB_NATIVE_REVERSE_INSTALL_START`
→ `CB_NATIVE_REVERSE_INSTALL_RETURNED`
→ `CB_REVERSE_BINDING_STATE_AFTER_INSTALL`
→ `CB_REVERSE_CACHE_ADOPTION_PASS`
→ `CB_CORE_API_SIGNAL_START`
→ `CB_CORE_API_SIGNAL_RETURNED`
→ `CB_MANAGED_PLUGIN_BOOTSTRAP_PASS`
→ natural Gate C markers.

If the run ends at `CB_CORE_API_SIGNAL_START`, the remaining problem is reverse unmanaged-callability of the dynamically loaded private GodotSharp callbacks on iOS. If that callback returns, the next natural Gate-C frontier is meaningful game/Godot behavior rather than missing bootstrap state.
