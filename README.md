# StS2 Launcher iOS — Step 21.1 Binding Diagnostic Export

**Version:** `0.0.57 (57)`  
**Codemagic workflow:** `ios-step-21-1`

Steps **01–20 are physically complete and closed** on the iPhone. Step 21 then physically passed all four Prepared Runtime / Framework Binding gates and produced an authoritative plan with:

```text
Explicit binding blockers: 47
Runtime closure ready for first real CLR load: NO
```

Step 21.1 is a **reporting/export-only hotfix**. The physically passed Step 21 binding/preparation implementation and host tests are hash-protected and unchanged.

## What Step 21.1 adds

- Reads the already persisted Step 21 `runtime-binding-plan.json`.
- Writes a complete share-safe text report:
  `Documents/StS2Launcher/Step21.1-RuntimeBindingDiagnostics.txt`.
- Includes grouped blocker counts, unique requested identities, every blocker row, host framework identities, prepared assembly identities, and the plan SHA-256.
- Excludes Steam credentials/tokens, Steam Guard material, Apple signing secrets, and host absolute assembly paths.
- Enables `UIFileSharingEnabled` and `LSSupportsOpeningDocumentsInPlace` so the report can be retrieved through iOS Files.
- Adds an **Export Complete Step 21 Binding Diagnostics to Files** button that can use the existing Step 21 plan immediately after app update; A–D do not need to be rerun merely to export if the plan persisted.

Files path:

```text
Files
→ On My iPhone
→ StS2 Launcher
→ StS2Launcher
→ Step21.1-RuntimeBindingDiagnostics.txt
```

The exported `.txt` is output only and is never consumed by the launcher as trusted input.

## Preserved Step 21 boundary

Step 21 A–D remain available unchanged:

```text
A — RuntimePayloadClassification
B — HostFrameworkBindingPlan
C — PreparedRuntimeAssemblySet
D — ClosureAudit
```

No real StS2 CLR load should be attempted while the authoritative plan still reports `Runtime closure ready: NO`.

See `docs/STEP-21.1-DIAGNOSTIC-EXPORT.md` and `docs/STEP-21.1-TEST.md`.
