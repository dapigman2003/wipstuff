# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.16 / `0.0.100 (100)` — real-Harmony fat release fixture hardening**

Codemagic 0.0.99 advanced beyond the prior namespace error: `StS2Launcher.Core` and `StS2Launcher.Core.Tests` both compiled. It then failed before test execution in the test-project MSBuild fixture-copy target because the downloaded `Lib.Harmony 2.4.2` NuGet package did not contain the implementation DLL at the assumed `lib/netstandard2.0/0Harmony.dll` path. No host tests, IPA publish, or device runtime occurred.

0.0.100 fixes the acquisition boundary without changing production runtime code:

- removes `PackageDownload` and the brittle `$(NuGetPackageRoot)` MSBuild copy target;
- pins the official tagged `Harmony-Fat.2.4.2.0.zip` release URL in `scripts/test.sh`;
- requires exactly one `netstandard2.0/0Harmony.dll` member and extracts only that file into `artifacts/host-step27-fixtures`;
- passes the absolute fixture path through `STS2_STEP27_REAL_HARMONY_FIXTURE`;
- records archive and extracted-DLL SHA-256 values in the host-test report;
- retains the real-binary assertions for `0Harmony` 2.4.2.0 identity, netstandard 2.0 reference profile, `EditorBrowsableAttribute` surface, Deferred-Cecil normalization, source-byte immutability, and the exact byte-distinct 11-instruction runtime cctor;
- leaves `ControlledHarmonyPatchExecution.cs` byte-for-byte unchanged from 0.0.99.

The 0.0.99 Codemagic report is preserved in `docs/history/reports/STEP-27.0.15-CODEMAGIC-REAL-HARMONY-FIXTURE-ACQUISITION-FAILURE.txt`.

## iOS detour decision rule

The stop rule from 0.0.98 remains unchanged:

1. Reach and pass T6 with the normalized cctor.
2. If public `PatchProcessor.Patch()` works, continue the Harmony path.
3. If T6 passes but T7/T8 fails, perform one representative patch/unpatch on a launcher-owned post-publish interpreted fixture.
4. If that interpreted target also cannot be patched, stop iterating Harmony internals and propose deterministic ahead-of-load Cecil transforms on derived runtime copies. That would be a master-plan-level architecture change.

The master plan remains unchanged because 0.0.100 is still inside the existing launcher-owned Harmony characterization boundary.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.100 (100)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Codemagic must acquire and execute the real Harmony 2.4.2 normalizer regression and then pass the complete host suite before publish. Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
