# Step 28.0.2 — Deferred Cecil Metadata Read Fix

Candidate: `0.0.111 (111)`

## Trigger evidence

Codemagic `0.0.110 (110)` passed canonical static validation **850/850**, compiled the project, built every external managed fixture, and executed the complete host suite. The result was **216/217 PASS**. The sole failure was `VerifiedSourceIsRewrittenBeforeLoadAndOnlyTransformedBehaviorExecutes` at Step-28 Gate A before any rewrite or CLR admission.

The exact failure was:

- `Mono.Cecil.AssemblyResolutionException`
- requested assembly: `System.Runtime, Version=9.0.0.0`
- source: `ReadFixtureModule(...)` using `ReadingMode.Immediate` with the deliberately rejecting `RejectingAssemblyResolver`
- Cecil path: eager custom-attribute constructor-argument decoding during immediate module read

Raw Codemagic output is preserved at `docs/history/reports/STEP-28.0.1-CODEMAGIC-HOST-TEST-FAILURE.txt`.

## Classification

This is a deterministic Gate-A host implementation defect, not a failure of the ahead-of-load transformation architecture. No transformed image was written, no fixture bytes entered the CLR, and no iOS publish/device evidence exists for 0.0.110.

The rejecting resolver behaved correctly: Step 28 must not broaden Cecil dependency search paths merely to satisfy metadata that the experiment does not need to resolve. The defect is that `ReadingMode.Immediate` asked Cecil to eagerly decode unrelated custom-attribute argument types.

## Correction

`AheadOfLoadManagedTransformation.ReadFixtureModule(...)` now uses:

- `ReadingMode.Deferred`
- the same `RejectingAssemblyResolver.Instance`

Deferred mode leaves unrelated custom-attribute blobs opaque unless explicitly inspected. Step 28 still reads and validates the exact assembly identity, type/method surface, method bodies, integer constants, direct-call topology, and P/Invoke metadata needed by Gates A–C. Any accidental assembly resolution remains fail-closed because the resolver is still rejecting.

No fixture semantics, Gate A–E ordering, rewrite target, transformed behavior, `AssemblyLoadContext` policy, OfflineReady rule, trusted-install rule, or Step-27 architecture decision changes.

## Protected regression

Static validation pins all of the following:

1. Step-28 fixture reads use `ReadingMode.Deferred`.
2. Step-28 production code retains the rejecting Cecil resolver.
3. `ReadingMode.Immediate` is absent from the Step-28 fixture reader.
4. The 0.0.110 host failure report remains preserved as evidence.

`MASTER-PLAN.md` remains unchanged because this correction does not alter architecture, methodology, roadmap, or end-state assumptions.

Local canonical static validation: **859/859 PASS**. Local host tests are not runnable in the current environment because `dotnet` is unavailable; this is not a host-test verdict.

## Next authority

Run Codemagic workflow `ios-step-28`. The required sequence remains:

1. canonical static validation;
2. complete host suite — expected **217/217 PASS** before publish;
3. iOS publish;
4. IPA verification;
5. physical Step-28 Gates A–E from a fresh process.

Physical closure remains **5/5 PASS**, with Gate D requiring **1000 / 1041 / 1041** and Gate E re-proving OfflineReady/isolation.
