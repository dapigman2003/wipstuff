# Current status

Steps **01–19 are physically complete and closed on the iPhone**.

Current candidate: **Step 20 — Dynamic Managed Execution Foundation**.

- App version: `0.0.55 (55)`
- Workflow: `ios-step-20`
- Protected baseline: Step 19.2 / `0.0.54`, which passed A–D plus OfflineReady and Foundation 5/5.
- Step 20 physical status: **not yet tested**.

## Why Step 20 comes before runtime/framework binding

The launcher is intended to acquire the legitimate StS2 payload after installation. Before spending a subsystem on binding the game's dependency graph to the iOS host, the project must prove that the Release iOS process can execute managed IL that was not available to the AOT compiler when the IPA was built.

Step 20 keeps all build-time launcher assemblies on the AOT path with `MtouchInterpreter=-all`, while retaining the Mono interpreter for runtime/dynamic managed code. Its fixture DLLs are copied into the `.app` only after `dotnet publish`, so Gate B cannot be satisfied by a normal build-time project/AOT reference.

Target completion:

```text
A — FixtureIntegrityAndOfflineReady
B — DynamicFixtureExecution
C — PrivateDependencyResolution
D — IsolationAudit

DYNAMIC MANAGED EXECUTION FOUNDATION PASS — 4/4
```

Closure still requires OfflineReady + Foundation 5/5 after the 4/4 result. No real StS2 CLR load occurs in Step 20.
