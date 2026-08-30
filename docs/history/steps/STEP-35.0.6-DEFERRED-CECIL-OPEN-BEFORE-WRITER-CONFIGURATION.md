# Step 35.0.6 — Deferred Cecil Open Before Writer Configuration

Candidate: **0.0.129 (129)**

## Evidence
Physical 0.0.128 repeated the normal Gate-A failure seen in 0.0.127: `AssemblyResolutionException` for exact `System.Runtime 9.0.0.0` before CLR admission. It produced no Gate-B/Gate-C/in-method evidence, so physical 0.0.126 remains the runtime authority.

The exact `sts2.dll` supplied after the run hashes to the closed trusted source SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`.

## Root cause
0.0.128 created `DiagnosticConstantMetadataWriteResolver`, then called `ModuleDefinition.ReadModule(... ReadingMode.Immediate ...)`, and only after the read attempted `resolver.Configure(module)`. The resolver rejects all requests until configured. Cecil immediate reading can materialize constant-bearing metadata and resolve the external enum type while still inside `ReadModule`, so `System.Runtime` is rejected before configuration.

The physically closed Step-32 writer avoids this ordering: it opens the module deferred, then audits/configures the bounded resolver, and only then writes.

## 0.0.129 correction
`CreateInstrumentedDiagnosticClone` must:
1. open file-backed with `ReadingMode.Deferred`;
2. require zero resolver requests from the initial open;
3. collect and validate the exact audited constant metadata requirements;
4. configure only in-memory surrogate assemblies for the approved `System.Runtime` and `Sentry` scopes;
5. inject the unchanged diagnostic markers and write the clone;
6. validate that all write-time resolver requests were approved;
7. reopen with a rejecting resolver and reverify constant metadata, identity, MVID, target signature, bridge and markers;
8. re-hash the exact transformed source unchanged.

`ReadingMode.Immediate` in the diagnostic clone source-open path is now a regression-contract violation.

## Authority
This is a launcher diagnostic-tooling correction only. It does not authorize additional dependencies, native loading, later initialization methods, Harmony/MonoMod patching, Godot startup, or exact Step-35 closure. A derivative 4/4 result remains localization evidence only.
