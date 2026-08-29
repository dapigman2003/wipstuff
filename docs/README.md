# Documentation Map

Start here when resuming the project. The active candidate is **Step 35.0.3 / 0.0.126**. Physical 0.0.120 closed the exact real-StS2 ahead-of-load rewrite, 0.0.121 closed transformed-primary CLR admission, and 0.0.122 closed exact transformed `PrewarmJit` execution. Physical 0.0.124 localized the current hard-kill frontier inside synchronous `ExecuteVeryEarly()` invocation. Physical 0.0.125 repeated the same native failure family but exposed a cross-run telemetry-correlation gap, so 0.0.126 first makes same-run durable evidence mandatory.

1. `MASTER-PLAN.md` — long-lived architecture, safety rules, authority model, and roadmap.
2. `CURRENT-STATUS.md` — exact physical baseline, active candidate, and next evidence boundary.
3. `ARCHITECTURE.md` — canonical transform-before-load/execution architecture.
4. `TESTING.md` — host/static/device acceptance workflow and Step-35 diagnostic interpretation.
5. `REGRESSION-CONTRACTS.md` — protected capability-level semantics and physical closure evidence.
6. `REPORTS.md` — output-only diagnostics, including current-run manifest, last checkpoint, run-specific crash journal, and static IL/callsite map.
7. `RELEASE-CHECKLIST.md` — pre-release and physical acceptance checklist.
8. `history/INDEX.md` — chronological records, including the 0.0.125 repeated hard kill and the 0.0.126 telemetry correction.
