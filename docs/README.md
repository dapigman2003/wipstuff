# Documentation map

Current candidate: **Step 35.0.28 / 0.0.151 (151)**. Step 35 remains OPEN pending the exact-authority device result.

Use `CURRENT-STATUS.md` for the latest physical/CI frontier, `REGRESSION-CONTRACTS.md` for non-negotiable implementation constraints, `TESTING.md` for host/device procedure, `RELEASE-CHECKLIST.md` for packaging, and `MASTER-PLAN.md` for the durable roadmap. Historical immutable design and physical/CI evidence live under `history/`.

Physical **0.0.149** is the latest device evidence. It completed the diagnostic Gate-C path: `MethodInfo.Invoke` returned, the ExecuteVeryEarly Task was `RanToCompletion`, the bounded await returned, post-await resolver/native confinement passed, and Gate C was recorded PASS. The UI later displayed Gate-D terminal 4/4 final-check progress while the durable journal remained at `D_START`; because the terminal progress was UI-only, no formal Gate-D PASS is claimed. Candidate 0.0.150 introduced durable Gate-D result-construction/return boundaries plus `GodotCoreExactClosure`, and Codemagic then passed 895/895 static, 214/214 host tests, and the Step-15 native-link preflight before iOS compile stopped on a single missing Core.Runtime namespace import in the managed-plugin bootstrap partial. 0.0.151 corrects only that compile integration defect and preserves the exact-closure runtime design.

