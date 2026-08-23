# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `bash scripts/validate.sh`.

For Step 27.0.20 / `0.0.104 (104)`, validation must prove the production normalizer still uses Deferred Cecil reads, retains the fail-closed resolver and exact 11-instruction cctor rewrite, keeps canonical-vs-synthetic scope separation, preserves the single public `PatchProcessor.Patch()` boundary, and keeps the production source byte-for-byte unchanged from 0.0.103.

The host surrogate contract is now content-addressed instead of inferred from merged AssemblyRef topology. Validation requires the exact Harmony-Fat 2.4.2 release URL, exact root-or-wrapped `net9.0/0Harmony.dll` selection, archive SHA-256 `a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774`, DLL SHA-256 `a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab`, independent C# DLL re-hash, `EditorBrowsableAttribute` surface detection without `ConstructorArguments`, and direct invocation of `CreateIosNormalizedHarmonyRuntimeImage`. It must forbid the prior `SingleOrDefault(reference => reference.Name == "System.Runtime")` and other target-framework inference from `System.Runtime`/`netstandard` AssemblyRefs.

## Host tests

Run `bash scripts/test.sh`.

The script downloads the exact official `Harmony-Fat.2.4.2.0.zip`, verifies the pinned archive hash before inspecting it, selects exactly one `net9.0/0Harmony.dll`, extracts only that member, verifies the pinned DLL hash, then exports its absolute path and exact archive member to the test process. The C# regression independently re-hashes the DLL and then exercises the production normalizer. The surrogate remains host-only; physical StS2 Harmony metadata is the production admission authority.

The complete 0.0.103 Codemagic report is archived as evidence that 212 tests executed at 211/212 and the sole failure was duplicate `System.Runtime` AssemblyRef handling before normalization.

## Codemagic / physical run

Codemagic must pass static validation, the complete host suite, iOS publish, and IPA verification. Then install `0.0.104 (104)` from a fresh process. The first meaningful device proof is T6. If T6 passes, the unresolved platform question becomes T7/T8 `PatchProcessor.Patch()` viability. The existing interpreted-fixture stop rule remains in force.
