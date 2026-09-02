# Documentation map

Current candidate: **Step 35.0.17 / 0.0.140 (140)**. Step 35 remains OPEN.

Use `CURRENT-STATUS.md` for the latest physical/CI frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical/CI evidence live under `history/`.

Physical **0.0.138** remains the latest device evidence. NATURAL entered the GodotSharp dictionary native thunk and stopped after `INMETHOD_GS014`; COMPAT physically proved the four-reference BCL dictionary substitution by emitting `CL_CRITICAL_001_POST`, then entered `Godot.OS::.cctor()` as `INMETHOD_GS033` before `GetCmdlineArgs` entry.

0.0.139 did **not** reach IPA/device testing. Static validation passed and Codemagic executed 210 host tests; 209 passed and one stale release-summary assertion still expected Step 35.0.15 while production emitted Step 35.0.16. Step 35.0.17 / 0.0.140 fixes only that test/release consistency boundary, preserves the three-mode NATURAL / OS-RECON / FORWARD runtime experiment, and adds a static guard against candidate-summary drift.
