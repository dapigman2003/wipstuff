# Step 35.0.22 — Godot reverse-binding readiness probe

Candidate: **0.0.145 (145)**. Step 35 remains OPEN.

## Motivation
Physical 0.0.144 proved that CORE-HANDOFF successfully obtains the native OS singleton and enters `InteropUtils.UnmanagedGetManaged`, then stops inside `NativeFuncs.godotsharp_internal_unmanaged_get_instance_binding_managed(IntPtr)` before the create-binding callback is reached. The remaining boundary is therefore native->managed object association, not the previously solved managed->native runtime-interoperability table.

## Design
After the existing successful 225-pointer `NativeFuncs.Initialize(IntPtr,int)` handoff and before Gate C, the project-owned Step-15 native bridge performs read-only queries of Godot 4.5.1 state:

- `CSharpLanguage::get_singleton() != nullptr`;
- `GDMonoCache::godot_api_cache_updated`;
- `GDMonoCache::managed_callbacks.ScriptManagerBridge_CreateManagedForGodotObjectBinding != nullptr`;
- an aggregate reverse-binding-ready conjunction.

The iOS layer writes `CB_REVERSE_BINDING_STATE`. If the aggregate is false, it writes `CB_REVERSE_BINDING_NOT_READY_STOP` and returns before Gate C. If true, it writes `CB_REVERSE_BINDING_READY_PASS` and retains the natural CORE-HANDOFF Gate-C path.

## Non-goals / prohibitions
This candidate does not call `GDMono::initialize`, does not call `GDMonoCache::update_godot_api_cache`, does not invoke `ScriptManagerBridge_CreateManagedForGodotObjectBinding`, does not initialize a second CLR, does not fabricate a GCHandle or `OSInstance`, and does not load the game's native executable.

## Expected physical result
The leading hypothesis is a safe pre-Gate-C stop with C# language/native scaffolding present but Godot managed API/cache/create-binding callback state absent. A surprising READY result is also useful: Gate C remains natural and the existing singleton marker set will localize the next boundary.
