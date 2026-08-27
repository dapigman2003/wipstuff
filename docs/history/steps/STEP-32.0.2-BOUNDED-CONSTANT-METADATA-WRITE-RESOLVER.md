# Step 32.0.2 — Bounded Constant-Metadata Cecil Write Resolver

Version: `0.0.117 (117)`

## Trigger

Physical 0.0.116 reached **Gate A PASS** with the exact receipt-backed `sts2.dll`, OfflineReady 428/428, the physical Step-31 source hash/MVID/token/body fingerprint, all ten PrepareMethod sites, zero Cecil read-time resolution, and no CLR admission. Gate B then failed inside `Mono.Cecil.MetadataBuilder.GetConstantType` while `ModuleDefinition.Write` rebuilt an unrelated field Constant-table row. Cecil requested exactly:

`System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a`

The all-rejecting resolver correctly stopped that request. The failure happened before a transformed image completed writing and therefore says nothing negative about the predeclared 6+4 `PrepareMethod` rewrite itself.

Raw physical evidence is preserved in `docs/history/reports/STEP-32.0.1-PHYSICAL-CECIL-WRITE-RESOLUTION-FAILURE.txt`.

## Why the resolver policy changes only during serialization

Cecil's writer must encode the ECMA-335 Constant table. For a constant whose declared metadata type is an external enum, Cecil resolves that declared type only to determine the enum's primitive storage type. That is different from Step-29/30/31 metadata auditing, where no dependency resolution is required, and different from runtime assembly binding.

Step 32.0.2 therefore does **not** enable `DefaultAssemblyResolver`, directory probing, receipt-tree dependency search, runtime Assembly.Load, or framework-file loading.

Instead Gate B configures a write-only in-memory resolver with these rules:

1. inspect the already verified source module's constant-bearing fields/properties/parameters without resolving anything;
2. derive each external constant's primitive storage type from the constant value already decoded from the source metadata;
3. require every external constant type that needs write-time resolution to be scoped to the exact physical `System.Runtime 9.0.0.0` identity above;
4. synthesize only those enum type definitions in an in-memory `System.Runtime` metadata surrogate;
5. during `module.Write`, satisfy only the exact `System.Runtime 9.0.0.0` assembly request from that surrogate;
6. reject every other assembly-resolution request;
7. open **zero** external framework/game assembly bytes for this resolver.

The surrogate is not serialized into `sts2.dll`; it exists only to let Cecil recover the primitive Constant-table element type while writing the already-loaded metadata graph.

## Rewrite semantics unchanged

Step 32.0.2 does not change the real-game semantic transformation:

- 6 × `RuntimeHelpers.PrepareMethod(RuntimeMethodHandle)` → `Pop`;
- 4 × `RuntimeHelpers.PrepareMethod(RuntimeMethodHandle, RuntimeTypeHandle[])` → `Pop + Pop`;
- exact physical Step-31 method/token/body/site evidence remains mandatory;
- no branch-targeted selected call may be rewritten;
- no other StS2 method/member is changed;
- the receipt-backed Step-12 installation remains immutable;
- the transformed image is still not CLR-loaded in Step 32.

## New verification invariant

Because 0.0.116 exposed a writer path through unrelated Constant-table metadata, Gate B now computes an offset-independent semantic fingerprint of all constant providers before write. Gate C recomputes it on both the private source and reopened transformed image and requires exact equality.

This is in addition to the existing Step-32 checks for:

- source physical hash/MVID/body identity;
- 10 → 0 PrepareMethod references;
- exact 6 + 4 rewrite counts;
- instruction/Pop/exception-handler topology;
- offset-independent transformed method semantic fingerprint;
- post-write transformed body fingerprint evidence;
- no CLR admission;
- final OfflineReady reproof.

## Authority

Local static validation can prove source structure and policy guards. Codemagic remains the compile/full-host/iOS packaging authority. A physical Step-32 pass still requires A–D **4/4** and authorizes only the later, separately gated transformed-real-StS2 CLR admission/execution boundary.
