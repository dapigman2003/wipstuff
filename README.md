# StS2 Launcher iOS — Step 21 Prepared Runtime / Framework Binding

**Version:** `0.0.56 (56)`  
**Codemagic workflow:** `ios-step-21`

Steps **01–20 are physically complete and closed** on the iPhone. Step 20 proved that the Release iOS host can execute post-publish managed IL through the Mono interpreter and resolve one exact verified private dependency while keeping build-time launcher assemblies on their AOT path.

Step 21 returns to the real user-owned StS2 managed payload but remains a **no-game-CLR-load** subsystem. It builds an execution-oriented dependency/binding plan beginning at the real receipt-backed ARM64 `sts2.dll`.

The key rule is:

> Prefer a compatible framework contract supplied by the actual iOS host; use the verified ARM64/shared workspace for private/game assemblies; preserve every missing, ambiguous, lower-version or non-IL-only edge as an explicit blocker instead of hiding it with broad fallback.

## Ordered gates

A. **RuntimePayloadClassification** — re-prove OfflineReady; clone and SHA-1 verify the ARM64/shared managed filename scope; catalog real assembly identities, AssemblyRefs/ModuleRefs, and IL-only versus ReadyToRun/mixed-mode shape; exclude x86_64 duplicates; require the primary ARM64 `sts2.dll` to be IL-only.

B. **HostFrameworkBindingPlan** — walk the real `sts2.dll` AssemblyRef graph. Framework-shaped references are first tested against the default iOS host runtime. Private dependencies resolve only through exact or one controlled higher/equal version workspace identity. Every unresolved/ambiguous/non-IL-only edge is recorded as a structured blocker.

C. **PreparedRuntimeAssemblySet** — perform zero Cecil writes; byte-copy only reachable IL-only private/game assemblies into `Step21-PreparedRuntimeBinding/prepared`; persist the deterministic `runtime-binding-plan.json` containing host bindings, private bindings, dependency edges and blockers.

D. **ClosureAudit** — independently re-hash source/prepared/live trees, re-open prepared metadata, verify plan integrity, reject host/private simple-name duplication, re-prove OfflineReady, and assert that no real StS2 assembly entered the CLR.

Target device result:

```text
PREPARED RUNTIME / FRAMEWORK BINDING PASS — 4/4
```

Step 21 deliberately separates **gate success** from **execution readiness**. The important plan signal is:

```text
Runtime closure ready for first real CLR load: YES/NO
```

A `NO` can still accompany a valid 4/4 Step 21 result; it means Step 22 has precise binding blockers to solve before any real game CLR load.

After 4/4, run **Verify Offline-Ready Install (Local Only)** and **Foundation 5/5 Regression** before closing Step 21.
