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

- `reports/STEP-24.0.2-PHYSICAL-GATE-A-REPORT.txt` — physical build 75 stopped at Gate A before any Step 24 CLR load because Cecil attempted to resolve `GodotSharp`; this is the direct evidence for Step 24.0.3.
- `reports/STEP-24.0.3-PHYSICAL-GATE-A-REPORT.txt` — physical build 76 repeated the build-75 Gate A `GodotSharp` resolver failure at 0/4, proving the explicit `MethodReference.Resolve()` removal did not eliminate every broad/eager Cecil metadata-resolution path.
- `reports/STEP-24.0.4-PHYSICAL-GATE-A-REPORT.txt` — physical build 77 eliminated the resolver failure and exposed the actual target closure: exactly seven conservative MonoMod logging dispatch findings and four automatic initializers; Gate B never ran.
- `reports/STEP-24.0.5-PHYSICAL-GATE-C-REPORT.txt` — physical build 78 passed Gates A/B and reached the real module initializer; Gate C exposed the trimmed `ConcurrentBag<T>` constructor that motivated the final preservation root.

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
- [Step 24 — Controlled 0Harmony Module Initialization Boundary](steps/STEP-24-CONTROLLED-MANAGED-INITIALIZATION.md) — design lineage for the automatic-initialization boundary after physical Step 23 closure.
- `steps/STEP-24.0.1-OFFLINEREADY-API-COMPILE-FIX.md` — build-73 compile-only correction to the established OfflineReady inspection API; build 74 subsequently reached host tests.
- `steps/STEP-24.0.2-PINVOKE-AUDIT-FIX.md` — build-74 host tests exposed a same-assembly P/Invoke audit blind spot; build 75 corrected it and reached physical Gate A.
- `steps/STEP-24.0.3-CECIL-LOCAL-METADATA-RESOLUTION-FIX.md` — build 75 motivated removal of explicit Cecil method resolution; physical build 76 later showed that correction was incomplete by repeating the same Gate A `GodotSharp` resolver failure.
- `steps/STEP-24.0.4-DEFERRED-TWO-PASS-METADATA-AUDIT-FIX.md` — physical build 76 repeated the opaque `GodotSharp` resolver failure; build 77 narrowed Gate A to deferred shallow whole-plan classification plus target-only closure audit with explicit rejecting Cecil resolvers and stronger diagnostics.
- `steps/STEP-24.0.5-CONDITIONAL-MONOMOD-LOGGING-DISPATCH.md` — physical build 77 exposed seven exact MonoMod logger dispatch findings; build 78 preserved the raw audit, conditionally classified exactly that fingerprint, and physically passed Gates A/B before reaching Gate C.
- `steps/STEP-24.0.6-SYSTEM-COLLECTIONS-CONCURRENT-PRESERVATION.md` — physical build 78 entered the real 0Harmony module initializer and failed on a trimmed `ConcurrentBag<T>` constructor; 0.0.79 added one `System.Collections.Concurrent` trimmer root without changing the interpreter or execution boundary.
- [Step 24.0.6 — Physical Closure](steps/STEP-24.0.6-PHYSICAL-CLOSURE.md) — user-confirmed physical 4/4 plus OfflineReady PASS and Foundation 5/5; Step 24 closed and the concurrent-collections preservation root became protected platform policy.
- [Step 25 — Controlled Harmony API Resolution + Type Initialization + Instance Construction](steps/STEP-25-CONTROLLED-HARMONY-CONSTRUCTION.md) — active nine-gate candidate that replays closed Step 24, resolves only the exact Harmony API/type-initializer surface, explicitly completes the measured Harmony type initializer, constructs one inert Harmony object, and still forbids patching/game/Godot/native progression.
- `steps/STEP-25.0.1-HOST-LOCAL-ASSEMBLY-CLASSIFICATION-FIX.md` — Step 25.0 / 0.0.80 compiled and ran 180 host tests at 177/180; 0.0.81 minimally fixes synthetic local-assembly classification plus one stale test-only fingerprint label.
- `steps/STEP-25.0.2-HARMONY-CONSTRUCTOR-FRAMEWORK-PRESERVATION.md` — physical 0.0.81 advanced 7/9 through Harmony type initialization; Gate H exposed trimmed `Environment.Version`, so 0.0.82 preserves the bounded framework type surface referenced by the exact measured constructor IL.

- `steps/STEP-25.0.2-PHYSICAL-CLOSURE.md` — physical 9/9 Step 25 closure with OfflineReady + Foundation preserved.
- `steps/STEP-26-CONTROLLED-HARMONY-PROCESSOR-CREATION.md` — active empty PatchProcessor creation design.
- `steps/STEP-26.0-PHYSICAL-CLOSURE.md` — physical 14/14 Step 26 closure with OfflineReady PASS and Foundation 5/5; inert PatchProcessor creation became the accepted baseline.
- `steps/STEP-27-CONTROLLED-LAUNCHER-HARMONY-PATCH.md` — active first real Harmony patch/unpatch design, restricted to a deterministic launcher-owned target/prefix pair; StS2 reflection remains deferred to Step 28.
