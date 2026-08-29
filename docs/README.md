# Documentation Map

Start here when resuming the project. The active candidate is **Step 35.0.2 / 0.0.125**. Physical 0.0.120 closed the exact real-StS2 ahead-of-load rewrite, 0.0.121 closed transformed-primary CLR admission, and 0.0.122 closed exact transformed `PrewarmJit` execution. Physical 0.0.124 then proved Step-35 Gate B passes and localized the hard termination inside the synchronous execution initiated by exact transformed `ExecuteVeryEarly()` `MethodInfo.Invoke`, after planned dependency/framework resolution but before Invoke returns.

1. `MASTER-PLAN.md` — long-lived architecture, safety rules, authority model, and roadmap.
2. `CURRENT-STATUS.md` — exact physical baseline, active candidate, and next evidence boundary.
3. `ARCHITECTURE.md` — canonical transform-before-load/execution architecture.
4. `TESTING.md` — host/static/device acceptance workflow and Step-35 diagnostic interpretation.
5. `REGRESSION-CONTRACTS.md` — protected capability-level semantics and physical closure evidence.
6. `REPORTS.md` — output-only diagnostics, including crash checkpoint and static IL/callsite map.
7. `RELEASE-CHECKLIST.md` — pre-release and physical acceptance checklist.
8. `history/INDEX.md` — chronological records, including 0.0.124 invoke-crash localization and the 0.0.125 static-map diagnostic design.
