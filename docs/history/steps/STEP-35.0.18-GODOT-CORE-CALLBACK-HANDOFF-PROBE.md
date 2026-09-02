# Step 35.0.18 — Godot core callback-handoff probe

Candidate release: **0.0.141 (141)**

## Trigger

Physical 0.0.140 FORWARD passed the entire CommandLineHelper interval and then reproduced the same GodotSharp callback boundary at `GodotFileIo.CreateDirectory -> Godot.DirAccess.DirExistsAbsolute -> StringName -> NativeFuncs.godotsharp_string_name_new_from_string`. Continuing to replace individual Godot wrappers would increasingly replace required game behavior rather than test the missing prerequisite.

## Native evidence

The owner-uploaded main game executable is 179,706,736 bytes with SHA-256 `7fadae8d46f0074ba745bc3beebe31a13df5fafed2f2ac69cd68b3c5dd8508e6`, exactly matching the main executable inventoried by the 0.0.140 reconnaissance. Its Godot 4.5.1 native side exposes the standard C# runtime interop machinery, including `godotsharp::get_runtime_interop_funcs(int&)` and Godot C#/Mono module symbols.

GodotSharp `NativeFuncs.Initialize(IntPtr,int)` requires the corresponding fixed runtime-interoperability callback layout and copies it into `_unmanagedCallbacks`. The active design therefore uses the same pinned source-built Godot 4.5.1 engine as the native-state owner and callback producer instead of fabricating pointers or loading the game native executable.

## Design

The pinned Step-15 iOS Godot archive is rebuilt with `module_mono_enabled=yes`. The existing project-owned smoke project remains non-C# and has no `dotnet` feature.

The Step-15 native bridge adds four diagnostic exports:

- `sts2_step15_is_runtime_interop_ready`
- `sts2_step15_has_dotnet_feature`
- `sts2_step15_is_dotnet_runtime_initialized`
- `sts2_step15_get_runtime_interop_funcs`

Readiness requires the live Step-15 engine plus Engine, ProjectSettings, CSharpLanguage and GDMono native scaffolding. Callback export obtains only `godotsharp::get_runtime_interop_funcs(size)` and rejects null/empty/non-pointer-aligned tables or null callback entries. The native bridge does not execute a callback or initialize managed GodotSharp.

A fourth Step-35 diagnostic mode, `GodotCoreCallbackHandoff`, requires Step-15 setup-complete state in the same process and refuses to run if the smoke project advertises the `dotnet` feature or if GDMono says its own runtime is initialized.

After Step-35 Gate B admits the verified sts2 diagnostic clone, the mode loads the separately verified private GodotSharp diagnostic derivative through the strict Step-35 ALC, binds exact `Godot.NativeInterop.NativeFuncs.Initialize(IntPtr,int)`, requires its `initialized` field to be false, invokes Initialize exactly once with the native pointer/size, requires `initialized=true`, freezes the resolver/native counters, and only then executes Gate C with the natural sts2/GodotSharp callsites.

NATURAL, OS-RECON and FORWARD remain unchanged fresh-process/no-Godot controls.

## Non-authority

CORE-HANDOFF is diagnostic. It does not load the game native executable, does not invoke the game entry point, does not authorize ExecuteEssential/ExecuteDeferred, does not broaden arbitrary resolver fallback, and does not permit Harmony/MonoMod runtime patching. A diagnostic 4/4 result cannot close exact Step 35.

## Intended physical sequence

Fresh launch -> Step 15 Gates A-C -> keep the smoke engine live -> Step 35 CORE-HANDOFF exactly once.

High-value checkpoints are the native readiness/table return, GS025 NativeFuncs.Initialize, managed handoff PASS, then natural advancement beyond the old GS031 dictionary frontier and GS024 StringName frontier.
