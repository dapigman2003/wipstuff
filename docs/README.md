# Documentation map

Current candidate: **Step 35.0.21 / 0.0.144 (144)**. Step 35 remains OPEN.

Use `CURRENT-STATUS.md` for the latest physical/CI frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical/CI evidence live under `history/`.

Physical **0.0.143** is the latest device evidence. CORE-HANDOFF accepted the exact 1,800-byte / 225-pointer Step-15 callback table, returned from `NativeFuncs.Initialize` with `initialized=true`, cleared the old dictionary/StringName boundaries, and reached `Godot.OS.GetCmdlineArgs()` → `Godot.OS.get_Singleton()`. Physical **0.0.140** remains the three-control baseline: NATURAL reached GS031 in the Godot dictionary native thunk. OS-RECON reached `Godot.OS::.cctor` → StringName → GS024. FORWARD cleared both command-line boundaries and reached `GodotFileIo.CreateDirectory` → `Godot.DirAccess.DirExistsAbsolute` → StringName → GS024. This establishes a repeated general GodotSharp callback-initialization boundary.

Step 35.0.21 / 0.0.144 changes no runtime semantics. It expands the verified GodotSharp marker/reconnaissance closure around `InteropUtils.EngineGetSingleton`, `UnmanagedGetManaged`, `ConvertStringToNative`, `godotsharp_engine_get_singleton`, and native-to-managed instance-binding callbacks so the new singleton frontier can be localized precisely.
