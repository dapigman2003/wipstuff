# Documentation map

Current candidate: **Step 35.0.20 / 0.0.143 (143)**. Step 35 remains OPEN.

Use `CURRENT-STATUS.md` for the latest physical/CI frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical/CI evidence live under `history/`.

Physical **0.0.140** is the latest device evidence. NATURAL reached GS031 in the Godot dictionary native thunk. OS-RECON reached `Godot.OS::.cctor` → StringName → GS024. FORWARD cleared both command-line boundaries and reached `GodotFileIo.CreateDirectory` → `Godot.DirAccess.DirExistsAbsolute` → StringName → GS024. This establishes a repeated general GodotSharp callback-initialization boundary.

0.0.142 proved the corrected callback telemetry contract with 855/855 static checks and 211/211 host tests, then stopped at iOS compile because the new Step-35 partial omitted `using StS2Launcher.iOS.Platform;`. Step 35.0.20 / 0.0.143 adds only that compile-integration correction. NATURAL / OS-RECON / FORWARD / CORE-HANDOFF runtime behavior is unchanged.
