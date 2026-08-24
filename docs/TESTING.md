# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `bash scripts/validate.sh`.

For Step 27.0.23 / `0.0.107 (107)`, validation must retain every 0.0.106 raw-body and LINQ-closure invariant while proving the new dynamic-payload host policy: `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all`. Static validation must reject any return to `TrimMode=full` for this candidate and must keep the single public `PatchProcessor.Patch()` call in the same location after T6/T6a/T6b. The production Harmony normalizer is unchanged except for diagnostic wording.

Physical 0.0.106 must be archived as evidence that the System.Linq root worked and that the next failure was `System.Diagnostics.DebuggableAttribute` during `MonoMod.Utils.DynamicMethodDefinition` type initialization, still before `PatchTools.DetourMethod`.

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

The complete 0.0.105 physical report is archived as evidence that raw-body HarmonySharedState normalization succeeded on-device and the first public `PatchProcessor.Patch()` call then failed in `HarmonyLib.MethodCreator..ctor` on trimmed `Enumerable.Union<T>` before `PatchTools.DetourMethod` was reached.

## Codemagic / physical run

Codemagic must pass static validation, the complete host suite, iOS publish, and IPA verification. Then install `0.0.107 (107)` from a fresh process. T6 and the LINQ preservation lesson are already physically established. The new proof is that the same `PatchProcessor.Patch()` path can initialize `DynamicMethodDefinition` and continue without another linker-induced missing BCL member. Only a failure after the copy/no-link policy has removed trimming ambiguity should influence the Harmony-versus-Step-28 decision.
