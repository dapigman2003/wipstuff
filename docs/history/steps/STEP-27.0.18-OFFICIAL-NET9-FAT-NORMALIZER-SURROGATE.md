# Step 27.0.18 — official net9 Harmony-Fat normalizer structural surrogate

Candidate: `0.0.102 (102)`

## Evidence

Codemagic 0.0.101 passed static validation (741/741) and downloaded the exact official `Harmony-Fat.2.4.2.0.zip`. The improved archive diagnostic then proved the release ZIP contains `0Harmony.dll` implementations for `netcoreapp3.0`, `netcoreapp3.1`, `net5.0` through `net10.0`, and .NET Framework targets, but **no netstandard2.0 implementation**. The host script therefore stopped before any `dotnet build`, MSTest execution, IPA publish, or device runtime. The raw report is preserved in `docs/history/reports/STEP-27.0.17-CODEMAGIC-HARMONY-FAT-NETSTANDARD-ABSENCE.txt`.

This corrects the remaining fixture-model assumption rather than production code. NuGet's `Lib.Harmony 2.4.2` framework metadata exposes .NET Standard 2.0 through `Lib.Harmony.Ref`, while the official fat release provides concrete merged implementations such as `net9.0/0Harmony.dll`. A reference assembly is not a valid runtime-normalizer fixture because it does not represent the merged executable patch-engine body we need to rewrite.

## Candidate change

`scripts/test.sh` still downloads the exact tagged official fat release over HTTPS, but now requires exactly one archive member ending in `/net9.0/0Harmony.dll`. That DLL is a **host-only structural surrogate**: it is not claimed to be byte-identical to StS2's on-device netstandard-flavored Harmony image. It is suitable for CI because Codemagic runs .NET 9, the assembly is the official merged Harmony 2.4.2 implementation, it contains the same upstream `HarmonySharedState` source and merged MonoMod patch-engine surface, and the regression explicitly requires the `EditorBrowsableAttribute` metadata surface that exposed the 0.0.97 Immediate-reader bug.

The host regression now proves exact `0Harmony, Version=2.4.2.0`, requires the net9 `System.Runtime, Version=9.0.0.0` reference profile and absence of a `netstandard` reference, verifies the `EditorBrowsableAttribute` surface without decoding constructor arguments, invokes the unchanged private production `CreateIosNormalizedHarmonyRuntimeImage` helper, requires a byte-distinct 11-instruction normalized image, and re-verifies source-byte immutability.

## Production boundary

`src/StS2Launcher.Core/Runtime/ControlledHarmonyPatchExecution.cs` is intentionally unchanged from 0.0.101. Production admission remains pinned to the exact on-device 0Harmony 2.4.2 metadata fingerprint. The CI net9 DLL is only a regression surrogate for Cecil/rewriter behavior and cannot weaken or substitute for the physical StS2 Harmony admission checks.

The master plan remains unchanged. If physical normalization reaches T6, the next evidence boundary remains the single public `PatchProcessor.Patch()` call at T7/T8. If that detour boundary fails after T6, the existing one-experiment interpreted-target stop rule still applies before any architecture pivot.
