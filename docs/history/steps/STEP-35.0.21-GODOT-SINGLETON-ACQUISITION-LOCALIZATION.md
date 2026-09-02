# Step 35.0.21 — Godot singleton acquisition localization

Candidate: 0.0.144 (144)

Physical 0.0.143 CORE-HANDOFF proved the primary callback-table handoff. The Step-15 engine returned the exact 1,800-byte / 225-pointer Godot 4.5.1 runtime interop table, private GodotSharp `NativeFuncs.Initialize(IntPtr,int)` returned with `initialized=true`, and the natural path survived the previously fatal Godot dictionary, `StringName`, and OS method-bind callback paths.

The new last durable marker is `INMETHOD_GS039 — Godot.OS::get_Singleton()` after `Godot.OS::GetCmdlineArgs()` entered. The static GodotSharp map shows `get_Singleton()` reads its cached field and, when empty, calls `Godot.NativeInterop.InteropUtils::EngineGetSingleton("OS")` before casting/storing the resulting `OSInstance`.

0.0.144 changes no game behavior and does not add another compatibility bypass. It expands the separately verified GodotSharp diagnostic derivative and read-only reconnaissance to include the singleton acquisition/wrapping closure:

- `Godot.NativeInterop.InteropUtils::EngineGetSingleton`
- `Godot.NativeInterop.InteropUtils::UnmanagedGetManaged`
- `Godot.NativeInterop.Marshaling::ConvertStringToNative`
- `Godot.NativeInterop.NativeFuncs::godotsharp_engine_get_singleton`
- `godotsharp_internal_unmanaged_get_script_instance_managed`
- `godotsharp_internal_unmanaged_get_instance_binding_managed`
- `godotsharp_internal_unmanaged_instance_binding_create_managed`
- relevant `OSInstance`/GodotObject construction methods when present

CORE-HANDOFF still requires Step 15 Gates A-C in the same process, still rejects competing Godot-managed-runtime state, still initializes only the verified private GodotSharp derivative, and still runs the natural sts2 callsites. NATURAL, OS-RECON, and FORWARD remain controls. A diagnostic 4/4 is not exact Step-35 closure.
