# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.19 / `0.0.103 (103)` — net9 surrogate reference-graph assertion fix**

Codemagic 0.0.102 finally reached the intended real-Harmony regression: it downloaded the exact official `Harmony-Fat.2.4.2.0.zip`, selected `net9.0/0Harmony.dll`, compiled production and tests, and executed all **212** host tests. **211 passed / 1 failed**.

The only failure was test-only: the regression assumed a net9 implementation could not retain a `netstandard` assembly reference. The official net9 Harmony binary does retain that compatibility reference, so the negative assertion was invalid. It failed before `CreateIosNormalizedHarmonyRuntimeImage` was invoked.

0.0.103 corrects only that regression contract:

- `scripts/test.sh` still selects exactly the official net9 archive member and now exports that selected member name to the test;
- the test positively proves the member is `net9.0/0Harmony.dll` (root or wrapped), rather than inferring target framework from the absence of `netstandard`;
- it retains the positive `System.Runtime, Version=9.0.0.0` assertion, exact `0Harmony, Version=2.4.2.0` identity, Deferred Cecil read, `EditorBrowsableAttribute` surface check, source-byte immutability, byte-distinct normalized image, and exact 11-instruction cctor audit;
- `ControlledHarmonyPatchExecution.cs` remains byte-for-byte unchanged from 0.0.102.

The complete 0.0.102 Codemagic report is preserved in project history.

## iOS detour decision rule

The stop rule from 0.0.98 remains unchanged:

1. Reach and pass T6 with the normalized cctor.
2. If public `PatchProcessor.Patch()` works, continue the Harmony path.
3. If T6 passes but T7/T8 fails, perform one representative patch/unpatch on a launcher-owned post-publish interpreted fixture.
4. If that interpreted target also cannot be patched, stop iterating Harmony internals and propose deterministic ahead-of-load Cecil transforms on derived runtime copies. That would be a master-plan-level architecture change.

The master plan remains unchanged because 0.0.103 is still inside the existing launcher-owned Harmony characterization boundary.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.103 (103)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Codemagic must acquire and execute the official merged Harmony 2.4.2 net9 structural-surrogate normalizer regression and then pass the complete host suite before publish. Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
