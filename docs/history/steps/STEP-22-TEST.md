# Step 22 physical-device test

Build workflow:

`ios-step-22`

Confirm the app header:

- `STEP 22 — HOST FRAMEWORK CLOSURE FOUNDATION`
- `Version 0.0.58`

Start from a fresh process if the Step 15 Godot host has run in the current process.

Tap:

`Run Step 22 A–D — Root Host BCL → Recompute Closure → Prepare Host-Bound Set → Audit`

Stop at the first failing gate.

Expected decisive evidence:

- Gate A: complete measured host framework frontier `44/44` loadable from the iOS host.
- Gate B: inspect the recomputed blocker/readiness counts; residual blockers are allowed to continue so the plan can be persisted.
- Gate C: requires `Explicit binding blockers: 0`, `Runtime closure ready for first real CLR load: YES`, and `Prepared System.*/netstandard framework assemblies: 0`.
- Gate D: independent isolation/plan audit passes and closure remains YES.

If Gate B or C fails, use `Export Current Runtime Binding Diagnostics to Files` and send `Step21.1-RuntimeBindingDiagnostics.txt`; the exporter reads the newly recomputed persisted plan.

After 4/4, run:

1. `Verify Offline-Ready Install (Local Only)`
2. `Run Foundation 5/5 Regression`

Only after both pass should Step 22 close.
