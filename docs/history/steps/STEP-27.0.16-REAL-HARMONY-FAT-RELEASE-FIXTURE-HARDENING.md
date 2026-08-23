# Step 27.0.16 — Real-Harmony fat release fixture hardening

Candidate: `0.0.100 (100)`

## Evidence from 0.0.99

Codemagic 0.0.99 proves the prior Cecil namespace correction worked: both `StS2Launcher.Core` and `StS2Launcher.Core.Tests` compiled and the test assembly was emitted. The run then stopped before MSTest execution in the test project's `CopyStep27RealHarmonyNormalizerFixture` target. That target assumed `PackageDownload Include="Lib.Harmony" Version="[2.4.2]"` would expose a fat implementation at `$(NuGetPackageRoot)lib.harmony/2.4.2/lib/netstandard2.0/0Harmony.dll`. The file was absent at that location, so MSBuild failed deliberately. No normalizer test, IPA publish, or device runtime followed.

## Correction

The package-layout assumption is removed rather than broadened. `scripts/test.sh` now owns the quarantined external fixture acquisition and pins the official Harmony tagged release asset:

`https://github.com/pardeike/Harmony/releases/download/v2.4.2.0/Harmony-Fat.2.4.2.0.zip`

The script requires exactly one archive member named `netstandard2.0/0Harmony.dll`, extracts only that member to `artifacts/host-step27-fixtures/0Harmony.dll`, records SHA-256 for both archive and extracted DLL, and exports its absolute path through `STS2_STEP27_REAL_HARMONY_FIXTURE`. The test project has no Harmony PackageReference or PackageDownload and therefore cannot accidentally add Harmony assets/dependencies to the host graph or depend on NuGet's internal package layout.

The existing real-binary regression still proves assembly name/version, the netstandard 2.0 reference observed on-device, the `EditorBrowsableAttribute` metadata surface without reading constructor arguments, Deferred-Cecil normalization under the rejecting resolver, source immutability, a byte-distinct runtime image, and the exact eleven-instruction normalized cctor audit.

## Runtime scope

`src/StS2Launcher.Core/Runtime/ControlledHarmonyPatchExecution.cs` is unchanged from 0.0.99. Gate A normalization, Gate S registration, Gate T T5/T6/T7 boundaries, the detour stop rule, and all StS2 prohibitions are unchanged. This candidate exists to make the real-Harmony CI proof execute reliably before another IPA is installed.

The master plan remains unchanged.
