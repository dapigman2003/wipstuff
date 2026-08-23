# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.18 / `0.0.102 (102)` — official net9 Harmony-Fat normalizer structural surrogate**

Codemagic 0.0.101 passed static validation and downloaded the exact official `Harmony-Fat.2.4.2.0.zip`. The improved archive diagnostic then proved the release contains concrete `0Harmony.dll` implementations for `netcoreapp3.x`, `net5.0` through `net10.0`, and .NET Framework targets, but **no `netstandard2.0` implementation**. The host script therefore stopped before any build, MSTest execution, IPA publish, or device runtime.

0.0.102 corrects only the host regression fixture model:

- downloads the same exact tagged official `Harmony-Fat.2.4.2.0.zip`;
- selects exactly one `net9.0/0Harmony.dll` member at archive root or under a release wrapper as a **host-only structural surrogate**;
- keeps the official merged Harmony 2.4.2 implementation quarantined outside the test dependency graph;
- verifies exact `0Harmony, Version=2.4.2.0`, net9 `System.Runtime` profile, and the `EditorBrowsableAttribute` surface without resolving custom-attribute arguments;
- invokes the unchanged private production `CreateIosNormalizedHarmonyRuntimeImage` helper against that real merged binary;
- requires byte-immutable source bytes and a byte-distinct exact 11-instruction normalized cctor image;
- leaves `ControlledHarmonyPatchExecution.cs` byte-for-byte unchanged from 0.0.101.

The net9 binary is deliberately **not** treated as a substitute for StS2's on-device Harmony image. Production admission remains pinned to the exact 0Harmony 2.4.2 metadata fingerprint measured from the prepared StS2 runtime.

The 0.0.101 Codemagic host report is preserved in `docs/history/reports/STEP-27.0.17-CODEMAGIC-HARMONY-FAT-NETSTANDARD-ABSENCE.txt`.

## iOS detour decision rule

The stop rule from 0.0.98 remains unchanged:

1. Reach and pass T6 with the normalized cctor.
2. If public `PatchProcessor.Patch()` works, continue the Harmony path.
3. If T6 passes but T7/T8 fails, perform one representative patch/unpatch on a launcher-owned post-publish interpreted fixture.
4. If that interpreted target also cannot be patched, stop iterating Harmony internals and propose deterministic ahead-of-load Cecil transforms on derived runtime copies. That would be a master-plan-level architecture change.

The master plan remains unchanged because 0.0.102 is still inside the existing launcher-owned Harmony characterization boundary.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.102 (102)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Codemagic must acquire and execute the official merged Harmony 2.4.2 net9 structural-surrogate normalizer regression and then pass the complete host suite before publish. Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
