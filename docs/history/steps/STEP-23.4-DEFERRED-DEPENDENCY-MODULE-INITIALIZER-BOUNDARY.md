# Step 23.4 — Deferred Dependency Module-Initializer Boundary

## Physical evidence

Step 23.3 / 0.0.68 reached Gate A on a physical iPhone and stopped before any real CLR load because one prepared private dependency contains a `<Module>..cctor`:

- `0Harmony, Version=2.4.2.0, Culture=neutral, PublicKeyToken=null`
- initializer count: 1

The persisted report stated that Gate A failed at the module-initializer safety boundary. No real StS2 CLR load occurred.

## Architectural correction

The original Step 23 rule was too broad for the stated objective. A module initializer in a dependency does not require us to abandon the first-primary-load experiment; it requires us to avoid loading that dependency in the load-only step.

Step 23.4 therefore separates two boundaries:

1. **Primary-load boundary (Step 23):** `sts2.dll` must itself have zero module initializers. It may be loaded into the dedicated private context.
2. **Automatic dependency-initialization boundary (Step 24):** any prepared dependency with `<Module>..cctor` is not loaded in Step 23. It is statically audited, resolver-blocked, and deferred.

Gate C resolves planned host bindings and loads only private prepared assemblies with zero module initializers. After every load, the context is audited so a deferred assembly cannot appear implicitly. Any CLR request for a deferred private dependency is a hard Step 23 failure.

Gate D proves the primary and maximal initializer-free private closure are resident, while every initializer-bearing dependency remains absent from the CLR. All prepared/live hashes and OfflineReady remain authoritative.

## Diagnostic improvement

Gate A records a compact Cecil instruction audit for each deferred `<Module>..cctor` in `Reports/Step23-FirstRealGameLoad.txt`. This is metadata-only and never invokes the initializer.

## Security/execution boundary

Step 23.4 still forbids entry-point access, game type/member reflection, method/delegate invocation, explicit class-constructor execution, Godot startup, native library loading, and mutation of the prepared/live installation.
