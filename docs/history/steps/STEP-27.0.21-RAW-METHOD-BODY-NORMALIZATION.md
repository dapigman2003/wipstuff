# Step 27.0.21 — raw HarmonySharedState method-body normalization

Candidate: `0.0.105 (105)`

## Trigger

Codemagic 0.0.104 finally executed the exact official, hash-pinned Harmony-Fat 2.4.2 net9.0 structural surrogate through `CreateIosNormalizedHarmonyRuntimeImage`. Production and tests compiled; all 212 tests ran; 211 passed. The sole failure was a real normalizer failure during `Mono.Cecil.ModuleDefinition.Write`.

The Deferred read solved the earlier eager custom-attribute problem, but Cecil 0.11.6's metadata writer still calls `MetadataBuilder.GetConstantType` when rebuilding the Constant table. For enum-typed constants that requires `TypeReference.Resolve()`. The fail-closed Step-27 metadata resolver therefore rejected `System.Reflection.BindingFlags`.

This is a known class of Cecil round-trip limitation: unresolved enum constants cannot be serialized without resolving the enum definition.

## Design correction

The Step-27 normalization never needed to rebuild Harmony metadata. It needs only to substitute one already-audited static constructor with a smaller body.

0.0.105 therefore removes Cecil's writer from the production normalizer entirely.

Cecil remains **read-only** and Deferred for:

- exact 0Harmony 2.4.2 identity;
- the existing full patch-engine fingerprint;
- exact HarmonySharedState field/cctor shape;
- discovery of the already-existing parameterless `Dictionary<...>` constructor MemberRef tokens;
- discovery of the exact HarmonySharedState FieldDef tokens;
- post-patch reopening and exact 11-instruction audit.

The runtime byte image itself is made from an exact clone of the prepared source bytes.

## In-place PE method-body substitution

The source `HarmonySharedState::.cctor` is required to:

- come from an IL-only module that is not `StrongNameSigned`, because in-place IL substitution would invalidate a strong-name signature;
- have a physical RVA;
- use a fat ECMA-335 method header;
- have no exception handlers / extra method sections;
- contain exactly one distinct existing MemberRef token for each required parameterless dictionary constructor;
- expose exact FieldDef tokens for `state`, `originals`, `originalsMono`, `methodAddressRef`, and `actualVersion`;
- have no other managed-method RVA inside the original cctor storage span;
- have enough existing method-body storage for a 12-byte fat header plus the 47-byte replacement IL.

`PEReader.PEHeaders` maps the cctor RVA to its existing file offset. The normalizer then clears **only the original cctor header+IL slot** in the cloned runtime image and writes:

- fat header, 3 DWORDs;
- `MaxStack = 1`;
- `CodeSize = 47`;
- `LocalVarSigTok = 0`;
- the same logical 11 instructions already admitted in 0.0.95:
  - three `newobj`;
  - five `stsfld`;
  - `ldnull`;
  - `ldc.i4 102`;
  - `ret`.

All metadata operands use tokens that already exist in the original Harmony image. No new metadata row is created.

The normalizer then verifies byte-for-byte that no location outside the admitted original cctor slot changed, reopens the resulting image with Deferred Cecil, and requires the exact 11-instruction fingerprint.

## Why this is stronger than an enum resolver whitelist

A whitelist for `BindingFlags`, `EditorBrowsableState`, or later enum types would make progress dependent on whichever unrelated metadata row Cecil happens to rebuild next. A general framework resolver would also broaden Gate A beyond its deliberate resolution-free model.

Raw method-body substitution removes that dependency completely:

- no Constant table rewrite;
- no CustomAttribute rewrite;
- no AssemblyRef/MemberRef/TypeSpec rebuild;
- no framework enum resolution;
- no metadata token renumbering;
- no PE section relocation;
- no source/prepared-file mutation.

## Gate model

Gate ordering remains unchanged. The runtime image is still private and in-memory only.

T5a re-hashes it; T5b executes the normalized cctor; T6 requires the three local dictionaries, null `methodAddressRef`, version 102, unchanged prepared bytes, and no generated Harmony shared-state/proxy assemblies. Only then may T7/T8 invoke the single public `PatchProcessor.Patch()` acceptance call.

The detour stop rule remains unchanged: if T6 passes but T7/T8 cannot patch a launcher-owned target, perform one bounded post-publish interpreted patch/unpatch experiment. If that also fails, stop iterating Harmony internals and propose the ahead-of-load StS2 transformation architecture as a master-plan change.

The master plan is unchanged for 0.0.105.
