# Documentation map

Current candidate: **Step 35.0.11 / 0.0.134 (134)**. Step 35 remains OPEN.

Use `CURRENT-STATUS.md` for the latest physical frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical evidence live under `history/`.

Physical 0.0.132 remains the game-frontier localization: `NullPlatformUtilStrategy..ctor` triggered `CommandLineHelper.TryGetValue` and hard-terminated before return. Physical 0.0.133 corrected the ordinal to NP002 but its new CommandLine cctor sweep was rejected with managed `InvalidProgramException` before any cctor marker and the launcher reached normal `RUN_END`; that is a diagnostic MaxStack defect. 0.0.134 reserves/verifies MaxStack and adds stack-neutral critical boundaries around dictionary setup and `Godot.OS.GetCmdlineArgs`.
