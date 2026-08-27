# Documentation Map

Start here when resuming the project. The active candidate is **Step 32.0.5 / 0.0.120**. Physical 0.0.119 advanced Step 32 to 2/4: Gate A re-proved the exact source and Gate B successfully wrote the bounded 6+4 private rewrite using only the three audited System.Runtime/Sentry metadata requirements. Gate C then failed before semantic verification because it reused the source MethodDef token as a post-Cecil-write locator. 0.0.120 changes only that transformed-image locator to exact declaring type + full signature; all semantic, metadata, resolver, isolation, and no-CLR-load contracts remain unchanged.

1. `MASTER-PLAN.md` — long-lived architecture, safety rules, authority model, and roadmap. Step 32 does not change that architecture.
2. `CURRENT-STATUS.md` — exact physical baseline/candidate and acceptance state.
3. `ARCHITECTURE.md` — canonical live source/tooling structure and active transform-before-load path.
4. `TESTING.md` — build/test/device acceptance workflow.
5. `REGRESSION-CONTRACTS.md` — protected capability-level semantics and physical closure evidence.
6. `REPORTS.md` — shareable diagnostics locations and secret-exclusion rules.
7. `RELEASE-CHECKLIST.md` — pre-release and physical acceptance checklist.
8. `history/INDEX.md` — chronological step records.
