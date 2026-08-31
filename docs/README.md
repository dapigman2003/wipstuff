# Documentation map

Current candidate: **Step 35.0.10 / 0.0.133 (133)**. Step 35 remains OPEN.

Use `CURRENT-STATUS.md` for the latest physical frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical evidence live under `history/`.

The latest physical result is 0.0.132: `NullPlatformUtilStrategy..ctor` entered, emitted `INMETHOD_NP003_PRE` for `CommandLineHelper.TryGetValue`, then hard-terminated. The exact-source map labels the same call `CALLSITE#002`; 0.0.133 repairs that mapping and localizes `CommandLineHelper..cctor`, including the `Godot.OS.GetCmdlineArgs` boundary.
