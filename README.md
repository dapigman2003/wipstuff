# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.17 / `0.0.101 (101)` — Harmony-Fat archive member discovery hardening**

Codemagic 0.0.100 successfully downloaded the exact official `Harmony-Fat.2.4.2.0.zip`, but the host script stopped before any build or MSTest execution because it required the archive member to equal `netstandard2.0/0Harmony.dll`. Official fat distributions wrap framework folders under a release root (for example `Harmony-Fat.2.4.2.0/net48/0Harmony.dll`), so the root-exact check found zero netstandard candidates. No production code was compiled or executed in that run, no IPA was published, and there is no new device evidence.

0.0.101 fixes only that CI fixture selector:

- downloads the same exact tagged official `Harmony-Fat.2.4.2.0.zip`;
- lists the archive once and requires exactly one member whose normalized path ends in `/netstandard2.0/0Harmony.dll`;
- retains the archive's original member name for extraction rather than inventing a path;
- prints all discovered `0Harmony.dll` members if the strict selector ever finds zero or multiple candidates;
- extracts only the unique netstandard2.0 DLL into `artifacts/host-step27-fixtures`;
- preserves the SHA-256 and `STS2_STEP27_REAL_HARMONY_FIXTURE` handoff;
- leaves `ControlledHarmonyPatchExecution.cs` byte-for-byte unchanged from 0.0.100 and retains the real-binary identity/metadata/normalizer assertions.

The 0.0.100 Codemagic host report is preserved in `docs/history/reports/STEP-27.0.16-CODEMAGIC-HARMONY-FAT-ARCHIVE-MEMBER-FAILURE.txt`.

## iOS detour decision rule

The stop rule from 0.0.98 remains unchanged:

1. Reach and pass T6 with the normalized cctor.
2. If public `PatchProcessor.Patch()` works, continue the Harmony path.
3. If T6 passes but T7/T8 fails, perform one representative patch/unpatch on a launcher-owned post-publish interpreted fixture.
4. If that interpreted target also cannot be patched, stop iterating Harmony internals and propose deterministic ahead-of-load Cecil transforms on derived runtime copies. That would be a master-plan-level architecture change.

The master plan remains unchanged because 0.0.101 is still inside the existing launcher-owned Harmony characterization boundary.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.101 (101)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Codemagic must acquire and execute the real Harmony 2.4.2 normalizer regression and then pass the complete host suite before publish. Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
