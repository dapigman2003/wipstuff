# Step 22 — Host Framework Closure Foundation

Step 22 converts the physical Step 21.1 blocker report into a measured iOS-host framework rooting policy.

Physical Step 21/21.1 evidence:

- Step 21 A–D: PASS.
- Explicit binding blockers: 47.
- Unique blocked requested identities: 34.
- Every blocker kind: `NonIlOnlyWorkspaceAssembly`.
- The only fallback for those edges was a copied macOS non-IL-only/ReadyToRun framework image.
- The prepared set also contained 12 framework-shaped `System.*`/`netstandard` IL assemblies that should be supplied by the iOS host rather than treated as private game payload.

Step 22 therefore adds 22 measured `TrimmerRootAssembly` seed roots. They are the union of:

1. every framework-shaped assembly that Step 21 selected as a private IL fallback; and
2. every framework assembly directly blocked from a non-framework consumer (`sts2`, `GodotSharp`, `Sentry`, or `0Harmony`).

ILLink preserves rooted assemblies and their statically understood dependencies. Gate A does not assume that this is sufficient: it requires the complete 44-name observed framework frontier (32 blocked simple names + 12 former private framework names) to load from `AssemblyLoadContext.Default` on the physical iPhone.

## Gates

### Gate A — RootedHostAvailability

- No StS2 CLR load.
- Loads all 44 measured host-framework identities from the default iOS/.NET host.
- Requires matching simple name and public-key token.
- Requires actual version >= the highest requested version observed by Step 21.1.

### Gate B — BindingClosureRecompute

Reuses the physically proven Step 21 planner unchanged:

- re-proves OfflineReady and rebuilds the receipt-backed ARM64/shared workspace;
- recomputes the real `sts2.dll` dependency graph;
- reports the recomputed blocker count/readiness but does not fail solely on residual blockers, so Gate C can persist a full diagnostic plan for Files export.

### Gate C — HostOnlyFrameworkPreparedSet

Reuses Step 21 Gate C unchanged to persist the recomputed plan, then additionally requires:

- persisted plan has zero blockers and `Runtime closure ready for first real CLR load: YES`;
- prepared private/game set contains no `System.*`, `netstandard`, `mscorlib`, Microsoft.CSharp/VisualBasic, or Microsoft.Win32 framework assemblies;
- zero Cecil writes;
- prepared bytes remain receipt-identical.

### Gate D — IsolationAudit

Reuses physically proven Step 21 Gate D unchanged and then requalifies the persisted plan:

- source/prepared/live hashes and plan integrity pass;
- zero blockers remain;
- no private framework implementation remains;
- no StS2 assembly has entered the CLR.

Step 22 does **not** execute StS2. A 4/4 result only proves that dependency closure is eligible for a later, controlled first real CLR-load probe.
