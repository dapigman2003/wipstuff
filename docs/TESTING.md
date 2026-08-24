# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `bash scripts/validate.sh`.

For Step 27.0.21 / `0.0.105 (105)`, validation must prove that production normalization no longer calls `Mono.Cecil.ModuleDefinition.Write`, keeps both Cecil reads Deferred, retains the fail-closed resolver, and changes only the existing `HarmonySharedState::.cctor` PE method-body slot in the cloned runtime image. The source must be IL-only; a `StrongNameSigned` source must be rejected because in-place IL substitution would invalidate its signature. The admitted cctor storage must contain no other managed-method RVA.

The raw-body contract must require:

- `PEReader` RVA-to-section mapping;
- a fat ECMA-335 method header;
- no exception handlers / `MoreSects`;
- exact existing MemberRef tokens for the three required dictionary constructors;
- exact FieldDef tokens for the five written HarmonySharedState fields;
- a 12-byte replacement fat header with `MaxStack=1`, `CodeSize=47`, and `LocalVarSigTok=0`;
- the same exact 11-instruction post-write Cecil audit;
- a byte-for-byte invariant that all bytes outside the admitted original cctor slot remain unchanged.

The host surrogate remains content-addressed instead of inferred from merged AssemblyRef topology. Validation requires the exact Harmony-Fat 2.4.2 release URL, exact root-or-wrapped `net9.0/0Harmony.dll` selection, archive SHA-256 `a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774`, DLL SHA-256 `a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab`, independent C# DLL re-hash, `EditorBrowsableAttribute` surface detection without `ConstructorArguments`, and direct invocation of `CreateIosNormalizedHarmonyRuntimeImage`.

## Host tests

Run `bash scripts/test.sh`.

The script downloads the exact official `Harmony-Fat.2.4.2.0.zip`, verifies the pinned archive hash, selects exactly one `net9.0/0Harmony.dll`, verifies the pinned DLL hash, and exports its path/member to the test process. The C# regression now has to pass through the real production raw-body normalizer, require the runtime image length to equal the source image length, require the runtime bytes to differ, preserve the source fixture byte-for-byte, and retain the exact 11-instruction normalized cctor audit.

The complete 0.0.104 Codemagic report is archived as evidence that all 212 tests executed at 211/212 and that the sole failure occurred in Cecil's writer at `MetadataBuilder.GetConstantType` while resolving `System.Reflection.BindingFlags`.

## Codemagic / physical run

Codemagic must pass static validation, the complete host suite, iOS publish, and IPA verification. Then install `0.0.105 (105)` from a fresh process. The first meaningful device proof is T6. If T6 passes, the unresolved platform question becomes T7/T8 `PatchProcessor.Patch()` viability. The existing interpreted-fixture stop rule remains in force.
