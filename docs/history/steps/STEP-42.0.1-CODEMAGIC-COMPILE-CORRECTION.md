# Step 42.0.1 — 0.0.175 Codemagic compile correction

0.0.174 did not reach iOS runtime. Codemagic Core compilation failed in `TransformedRealStS2GameStartupInitPools.cs` with CS1061 for three non-existent shorthand properties on `Step35ExecutionLoadContext` and CS0103 for a non-existent diagnostic helper.

The correction is intentionally compile-only:

- `context.InitializerLoads` → `context.InitializerBearingRequests`
- `context.RejectedLoads` → `context.RejectedManagedRequests`
- `context.NativeLoads` → `context.NativeLoadAttempts`
- `BuildFailureDiagnostic(ex)` → `FormatExceptionDiagnostic(ex)`

These are the existing members already used by prior physically proven Step-39/40 confinement code. No Step-42 gate semantics, `InitPools()` reflection binding, Cecil closure audit, one-shot execution policy, rendering policy, trusted-install policy, or forbidden startup/platform/Steam/native boundaries changed.

Codemagic remains the first actual compile authority for 0.0.175.
