# Documentation map

Current candidate: **Step 35.0.26 / 0.0.149 (149)**. Step 35 remains OPEN.

Use `CURRENT-STATUS.md` for the latest physical/CI frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical/CI evidence live under `history/`.

Physical **0.0.146** is the latest device evidence. It physically completed the generated Godot managed-plugin bridge: 37 non-null reverse callbacks were created, script lookup returned, the complete callback struct was adopted by source-built Godot, reverse binding became ready, and `GD_OnCoreApiAssemblyLoaded` returned. The run then stopped cleanly before ExecuteVeryEarly target binding because Gate C still compared resolver counters with the pre-bootstrap snapshot. 0.0.147 preserved that successful bootstrap and added the exact post-bootstrap resolver seal, but Codemagic stopped at 212/213 host tests on a stale negative-test message assertion. Candidate 0.0.148 corrected that host-test contract and has now reached Gate D on-device. Candidate 0.0.149 preserves the bridge/Gate-C contract while exposing Gate-D receipt verification progress plus a live elapsed-time heartbeat during long single-file hashes and warming the pinned Codemagic .NET+iOS workload toolchain cache.

