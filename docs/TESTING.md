# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `bash scripts/validate.sh`.

For Step 27.0.22 / `0.0.106 (106)`, validation must retain every 0.0.105 raw-body normalization invariant and additionally prove the measured post-publish `System.Linq` framework-member preservation contract. `System.Linq` must be an exact `TrimmerRootAssembly` root, build telemetry must report it, and Gate T must perform T6a/T6b exact `Enumerable.Select` / two-sequence `Union` / three-selector `ToDictionary` signature checks after normalized shared-state validation and before the single public `PatchProcessor.Patch()` call.

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

Codemagic must pass static validation, the complete host suite, iOS publish, and IPA verification. Then install `0.0.106 (106)` from a fresh process. T6 is already physically crossed by 0.0.105; the new immediate proof is T6a/T6b confirming the linked host retains the exact Harmony LINQ surface. If that passes, T7/T8 may finally reach replacement generation and, potentially, the MonoMod detour boundary. Do not trigger the Step-28 pivot solely from a framework-member trimming failure.
