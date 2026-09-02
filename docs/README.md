# Documentation map

Current candidate: **Step 35.0.18 / 0.0.141 (141)**. Step 35 remains OPEN.

Use `CURRENT-STATUS.md` for the latest physical/CI frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical/CI evidence live under `history/`.

Physical **0.0.140** is the latest device evidence. NATURAL reached GS031 in the Godot dictionary native thunk. OS-RECON reached `Godot.OS::.cctor` → StringName → GS024. FORWARD cleared both command-line boundaries and reached `GodotFileIo.CreateDirectory` → `Godot.DirAccess.DirExistsAbsolute` → StringName → GS024. This establishes a repeated general GodotSharp callback-initialization boundary.

Step 35.0.18 / 0.0.141 preserves NATURAL / OS-RECON / FORWARD and adds a separately gated CORE-HANDOFF mode. CORE-HANDOFF requires the already-live Step-15 smoke engine, rejects `dotnet` feature / competing Godot-managed-runtime state, obtains the exact source-built Godot 4.5.1 runtime callback table, initializes only the verified private GodotSharp derivative with it, and then measures the natural ExecuteVeryEarly path.
