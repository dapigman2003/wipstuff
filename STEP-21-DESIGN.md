# Step 21 — Prepared Runtime / Framework Binding

**Runtime version:** 0.0.56 (56)  
**Codemagic workflow:** `ios-step-21`

## Objective

Step 21 builds the first execution-oriented managed dependency/binding plan for the real receipt-backed ARM64 StS2 payload while still refusing to CLR-load or execute any game assembly.

The subsystem exists because the downloaded macOS depot contains both application/private managed assemblies and desktop .NET runtime/framework implementation assemblies. Step 19.2 proved that copied desktop `System.*` framework binaries are not the compatibility layer we should mutate or execute on iOS. Step 20 then physically proved that the Release iOS host can execute post-publish managed IL and resolve one verified private dependency through the Mono interpreter.

Step 21 combines those lessons:

1. start from the real ARM64 `sts2.dll` AssemblyRef graph;
2. prefer a compatible assembly supplied by the actual iOS host for framework-shaped contracts;
3. resolve private/game dependencies only from receipt-SHA-1-verified ARM64/shared workspace inputs;
4. never silently guess missing, ambiguous, lower-version, or non-IL-only dependencies;
5. create a byte-identical prepared set containing only reachable IL-only private/game assemblies;
6. persist every host/private edge and blocker into a deterministic runtime binding plan;
7. do not CLR-load StS2 yet.

A key Step 21 semantic is intentional:

> **A 4/4 gate pass proves the binding plan and prepared set are trustworthy. It does not imply the dependency closure is ready for execution.**

The plan separately reports:

```text
Runtime closure ready for first real CLR load: YES/NO
```

If `NO`, Step 22 must address the exact recorded blockers before attempting any real game CLR load.

## Gate A — RuntimePayloadClassification

Gate A:

- re-proves Step 13 `OfflineReady`;
- reads and validates the trusted Step 12 install receipt;
- selects the already-proven Step 17/18 managed scope:
  - macOS ARM64 managed filename candidates;
  - architecture-neutral candidates;
  - excludes macOS x86_64 duplicates;
- recreates:

```text
Documents/StS2Launcher/Step21-PreparedRuntimeBinding/source
```

- byte-copies every selected input;
- SHA-1 verifies every source copy against the trusted receipt;
- probes `.dll` / `.exe` filename candidates with Mono.Cecil using a rejecting dependency resolver;
- catalogs actual assembly identity:
  - Name;
  - Version;
  - Culture;
  - PublicKeyToken;
- records IL-only versus non-IL-only/ReadyToRun-or-mixed-mode image shape;
- records AssemblyRef and ModuleRef metadata without resolving dependencies;
- requires exactly one real ARM64 `sts2.dll` and requires that primary assembly to be IL-only;
- verifies no real StS2 assembly has entered the CLR.

Gate A does not use `AssemblyLoadContext` for game/private assemblies and performs no Cecil write.

## Gate B — HostFrameworkBindingPlan

Gate B performs a breadth-first read-only dependency graph traversal starting at the real copied ARM64 `sts2.dll`.

Every reachable AssemblyRef is classified into one of three broad categories.

### 1. Host framework binding

For framework-shaped names such as:

```text
System
System.*
mscorlib
netstandard
Microsoft.CSharp
Microsoft.VisualBasic*
Microsoft.Win32.*
```

Step 21 first asks the actual iOS default `AssemblyLoadContext` to satisfy the requested identity.

A host binding is accepted only when:

- simple name matches;
- culture matches;
- public-key token matches;
- actual host assembly version is equal to or greater than the requested version.

This version rule follows modern `AssemblyLoadContext` binding semantics: one version per simple name per context, and an already loaded/resolved version may satisfy a request when it is equal or higher than the requested version.

If the host cannot satisfy a framework-shaped request, Step 21 may still attempt the verified workspace as a private fallback. This protects legitimate NuGet/package assemblies whose simple names begin with `System.` but are not actually supplied by the host runtime.

### 2. Verified private/workspace binding

For non-framework dependencies, and for framework-shaped names not supplied by the host, Step 21 matches against the verified workspace metadata catalog.

Order:

1. exact Name + Version + Culture + PublicKeyToken;
2. if no exact match exists, allow one unambiguous version-unified candidate only when:
   - Name matches;
   - Culture matches;
   - PublicKeyToken matches;
   - only one workspace version exists for that identity;
   - actual version is equal to or greater than the requested version;
3. byte-identical duplicate paths are deterministic;
4. byte-distinct ambiguity is never guessed.

Only IL-only workspace assemblies are eligible for the eventual prepared execution set.

### 3. Explicit blocker

Examples:

```text
MissingWorkspaceAssembly
WorkspaceIdentityMismatch
WorkspaceVersionAmbiguity
WorkspaceVersionTooLow
WorkspaceByteAmbiguity
HostFrameworkUnavailable
NonIlOnlyWorkspaceAssembly
HostPrivateSimpleNameConflict
```

A blocker is preserved as structured plan data rather than converted into a broad fallback.

Critically, blocker presence does **not** make Gate B itself fail. Gate B fails only if the graph cannot be safely/authoritatively classified. This keeps Step 21 an analysis/preparation subsystem rather than conflating “we understand the closure” with “the closure has no remaining work.”

## Gate C — PreparedRuntimeAssemblySet

Gate C creates:

```text
Documents/StS2Launcher/Step21-PreparedRuntimeBinding/prepared
Documents/StS2Launcher/Step21-PreparedRuntimeBinding/plan/runtime-binding-plan.json
```

The prepared set contains only:

- the real primary ARM64 `sts2.dll`;
- reachable IL-only private/game assemblies selected by Gate B.

The prepared set explicitly does not include a copied desktop framework implementation merely because a `System.*` DLL exists in the depot when the iOS host has already satisfied that contract.

For every prepared assembly Gate C:

1. rechecks source SHA-1 immediately before copy;
2. byte-copies the assembly;
3. verifies prepared SHA-1 still equals the receipt SHA-1;
4. reopens metadata read-only;
5. requires the same assembly full identity;
6. requires IL-only image shape.

Gate C performs:

```text
Cecil assembly writes: 0
Strong-name/public-key edits: 0
Game CLR loads: 0
```

It serializes a source-generated JSON plan containing:

- trusted install identity;
- primary assembly identity;
- prepared assemblies;
- host framework bindings;
- explicit blockers;
- every classified dependency edge;
- `RuntimeClosureReady`.

## Gate D — ClosureAudit

Gate D independently proves:

- every selected Step 21 source copy still matches its trusted receipt SHA-1;
- every corresponding live Step 12 install file still matches its trusted receipt SHA-1;
- the prepared directory contains exactly the assemblies declared by the plan;
- every prepared assembly remains receipt-identical, IL-only and identity-stable;
- persisted plan SHA-256 is unchanged since Gate C;
- persisted plan summary matches the in-memory Gate B plan;
- host-bound assembly simple names are not duplicated in the private prepared set;
- OfflineReady still passes after preparation;
- no real StS2 assembly entered the CLR.

Final target:

```text
PREPARED RUNTIME / FRAMEWORK BINDING PASS — 4/4
```

After 4/4, run:

```text
Verify Offline-Ready Install (Local Only)
Run Foundation 5/5 Regression
```

Only then close Step 21.

## What Step 21 does not do

Step 21 does not:

- `AssemblyLoadContext`-load `sts2.dll`;
- load `GodotSharp` into the CLR;
- invoke a game method;
- trigger game static/module initialization;
- mutate managed assembly IL;
- force copied desktop ReadyToRun/framework images to become IL-only;
- integrate native game libraries;
- apply Harmony/MonoMod detours;
- implement FMOD/Spine runtime binding;
- add Cloud/Workshop.

## Step 22 decision

If Step 21 closes with:

```text
Runtime closure ready for first real CLR load: YES
```

then Step 22 may be the first tightly controlled real-assembly CLR-load probe, still stopping before broad game execution.

If it closes with:

```text
Runtime closure ready for first real CLR load: NO
```

Step 22 should instead address the exact blockers in `runtime-binding-plan.json` (for example missing trimmed host framework contracts, unresolved private assembly identities, or non-IL-only desktop-only dependencies) before any real StS2 CLR load.
