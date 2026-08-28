# Documentation Map

Start here when resuming the project. The active candidate is **Step 35.0.1 / 0.0.124**. Physical 0.0.120 closed the exact real-StS2 ahead-of-load rewrite, 0.0.121 closed transformed-primary CLR admission, and 0.0.122 closed the first exact transformed-site execution (`PrewarmJit`) at 4/4. Physical Step 35.0 / 0.0.123 then hard-terminated around the visible Gate-B/B→C region with a matching iOS `.ips` showing `EXC_BAD_ACCESS / SIGKILL` and main-thread PC=`0x0`; no managed Step-35 report survived. Step 35.0.1 keeps the experiment unchanged and adds durable crash-localization checkpoints.

1. `MASTER-PLAN.md` — long-lived architecture, safety rules, authority model, and roadmap.
2. `CURRENT-STATUS.md` — exact physical baseline/candidate and acceptance state.
3. `ARCHITECTURE.md` — canonical live source/tooling structure and transform-before-load/execution path.
4. `TESTING.md` — build/test/device acceptance workflow, including Step-35 crash-checkpoint interpretation.
5. `REGRESSION-CONTRACTS.md` — protected capability-level semantics and physical closure evidence.
6. `REPORTS.md` — shareable diagnostics locations and secret-exclusion rules.
7. `RELEASE-CHECKLIST.md` — pre-release and physical acceptance checklist.
8. `history/INDEX.md` — chronological step records, including Step-34 closure, Step-35.0 hard-termination evidence, and Step-35.0.1 diagnostic design.
