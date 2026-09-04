# Documentation map

Current candidate: **Step 35.0.27 / 0.0.150 (150)**. Step 35 remains OPEN pending the exact-authority device result.

Use `CURRENT-STATUS.md` for the latest physical/CI frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical/CI evidence live under `history/`.

Physical **0.0.149** is the latest device evidence. It completed the diagnostic Gate-C path: `MethodInfo.Invoke` returned, the ExecuteVeryEarly Task was `RanToCompletion`, the bounded await returned, post-await resolver/native confinement passed, and Gate C was recorded PASS. The UI later displayed Gate-D terminal 4/4 final-check progress while the durable journal remained at `D_START`; because the terminal progress was UI-only, no formal Gate-D PASS is claimed. Candidate 0.0.150 preserves the proven bridge/resolver contract, adds durable Gate-D result-construction/return boundaries, and introduces `GodotCoreExactClosure` using exact closed transformed sts2 plus exact prepared GodotSharp CLR inputs.

