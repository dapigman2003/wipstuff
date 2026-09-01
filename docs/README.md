# Documentation map

Current candidate: **Step 35.0.15 / 0.0.138 (138)**. Step 35 remains OPEN.

Use `CURRENT-STATUS.md` for the latest physical/CI frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical/CI evidence live under `history/`.

Physical 0.0.136 is the latest device evidence: `CommandLineHelper..cctor` reached `INMETHOD_CL_CRITICAL_001_PRE` and hard-terminated during `Godot.Collections.Dictionary<string,string>` construction before `_args` assignment.

0.0.137 did **not** reach a physical run. Codemagic stopped at 208/209 host tests because the new GodotSharp derivative verifier checked its entry marker against the sts2 diagnostic bridge type. Step 35.0.15 / 0.0.138 fixes that verifier-only mismatch, keeps live-stack CL/CLTV runtime sweeps retired, and otherwise preserves the same NATURAL/COMPAT comprehensive GodotSharp/native reconnaissance design.
