# Documentation Map

Start here when resuming the project. The active frontier is Step 32.0.2 / 0.0.117 after physical 0.0.116 passed Gate A and exposed a bounded Cecil serialization dependency: `module.Write` needed exact `System.Runtime 9.0.0.0` enum metadata to encode unrelated Constant-table rows.

1. `MASTER-PLAN.md` — long-lived architecture, safety rules, authority model, and roadmap. Step 32 does not change that architecture.
2. `CURRENT-STATUS.md` — exact physical baseline/candidate and acceptance state.
3. `ARCHITECTURE.md` — canonical live source/tooling structure and active transform-before-load path.
4. `TESTING.md` — build/test/device acceptance workflow.
5. `REGRESSION-CONTRACTS.md` — protected capability-level semantics and physical closure evidence.
6. `REPORTS.md` — shareable diagnostics locations and secret-exclusion rules.
7. `RELEASE-CHECKLIST.md` — pre-release and physical acceptance checklist.
8. `history/INDEX.md` — chronological step records.
