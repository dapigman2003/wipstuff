# Historical Step Documentation Index

This directory is the readable project history. Historical records stay here even though active source/tooling uses canonical names.

## Retrospective early foundation

- `STEP-01-FOUNDATION-RETROSPECTIVE.md`
- `STEP-02-FOUNDATION-RETROSPECTIVE.md`
- `STEP-03-FOUNDATION-RETROSPECTIVE.md`
- `STEP-04-FOUNDATION-RETROSPECTIVE.md`
- `STEP-05-FINAL-TEST.md`

The Step 01–04 files were written retrospectively during Step 22.4 from the physically closed Foundation evidence because the earliest individual step documents were not retained in the later source tree.

## Later records

Step-specific design/test/fix records for Steps 06 through 22.x are retained in `steps/`. They describe what was known at the time and should be treated as history, not current build instructions.

The current architecture and plan always live one level up in `docs/`.

## Selected raw reports

`reports/` retains diagnostic outputs when they materially explain a later architectural decision.

## Canonical-foundation build fixes

- `STEP-22.4.1-MSTEST-V4-CODEMAGIC-FIX.md`

- `STEP-22.4.2-STEP19-REGRESSION-CONTRACT-CORRECTION.md`

## First real managed-game load

- `STEP-23-FIRST-REAL-CLR-LOAD.md`
- `STEP-23-TEST.md`

- [Step 23.1 — Host-Test Isolation Fix](steps/STEP-23.1-HOST-TEST-ISOLATION-FIX.md)
- [Step 23.2 — Deterministic Host-Test Identity Isolation](steps/STEP-23.2-DETERMINISTIC-HOST-TEST-IDENTITY-ISOLATION.md)
- [Step 23.3 — Synthetic Fixture Binding-Plan Coverage Fix](steps/STEP-23.3-SYNTHETIC-FIXTURE-PLAN-COVERAGE-FIX.md)

- [Step 23.4 — Deferred Dependency Module-Initializer Boundary](steps/STEP-23.4-DEFERRED-DEPENDENCY-MODULE-INITIALIZER-BOUNDARY.md)

- `steps/STEP-23.4.1-CECIL-IL-AUDIT-COMPILE-FIX.md` — compile-only missing Cecil.Cil namespace correction.

- `steps/STEP-23.4.2-SYNTHETIC-CORELIB-FIXTURE-NORMALIZATION.md` — host-test fixture correction removing artificial legacy mscorlib metadata.

- `steps/STEP-23.4.3-CECIL-CORELIB-SCOPE-CONSTRUCTION-FIX.md` — constructs the synthetic module-initializer core-library scope correctly before Cecil creates primitive void metadata.

- [Step 23.4.3 — Physical Closure](steps/STEP-23.4.3-PHYSICAL-CLOSURE.md) — all A–D gates, OfflineReady, and Foundation 5/5 passed on physical iPhone; Step 23 closed.
- [Step 24 — Controlled 0Harmony Module Initialization Boundary](steps/STEP-24-CONTROLLED-MANAGED-INITIALIZATION.md) — active controlled automatic-initialization candidate after physical Step 23 closure.
