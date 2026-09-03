# Documentation map

Current candidate: **Step 35.0.24 / 0.0.147 (147)**. Step 35 remains OPEN.

Use `CURRENT-STATUS.md` for the latest physical/CI frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical/CI evidence live under `history/`.

Physical **0.0.146** is the latest device evidence. It physically completed the generated Godot managed-plugin bridge: 37 non-null reverse callbacks were created, script lookup returned, the complete callback struct was adopted by source-built Godot, reverse binding became ready, and `GD_OnCoreApiAssemblyLoaded` returned. The run then stopped cleanly before ExecuteVeryEarly target binding because Gate C still compared resolver counters with the pre-bootstrap snapshot. Candidate 0.0.147 preserves that successful bootstrap and seals an exact post-bootstrap resolver baseline before natural Gate C.

