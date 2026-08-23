# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.20 / `0.0.104 (104)` — hash-pinned real-Harmony normalizer execution**

Codemagic 0.0.103 compiled production and tests and executed all **212** host tests. **211 passed / 1 failed**. The sole failure was test-only and occurred before `CreateIosNormalizedHarmonyRuntimeImage`: the merged official Harmony-Fat net9.0 binary contains more than one `System.Runtime` AssemblyRef, so `SingleOrDefault` was an invalid metadata assumption.

0.0.104 removes that entire class of surrogate inference instead of replacing it with another AssemblyRef guess:

- `scripts/test.sh` downloads the exact tagged `Harmony-Fat.2.4.2.0.zip`;
- it requires the exact selected `net9.0/0Harmony.dll` archive member;
- it now pins the Codemagic-observed release archive SHA-256 `a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774`;
- it pins the extracted official net9.0 DLL SHA-256 `a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab`;
- the C# regression independently re-hashes the DLL, verifies exact `0Harmony, Version=2.4.2.0` identity and the `EditorBrowsableAttribute` surface, and then immediately invokes the actual production Deferred-Cecil normalizer;
- it deliberately makes no uniqueness/version assertions about `System.Runtime` or `netstandard` AssemblyRef rows in the merged binary.

`ControlledHarmonyPatchExecution.cs` is unchanged from 0.0.103. The production on-device admission still uses the exact prepared StS2 Harmony 2.4.2 patch-engine fingerprint; the upstream net9 binary remains a host-only structural surrogate.

The full 0.0.103 Codemagic 211/212 report is preserved in project history.

## iOS detour decision rule

The stop rule remains unchanged: reach T6 with the normalized cctor; if public `PatchProcessor.Patch()` works, continue Harmony. If T6 passes but T7/T8 fails, perform one representative patch/unpatch on a launcher-owned post-publish interpreted fixture. If that also fails, stop iterating Harmony internals and propose deterministic ahead-of-load Cecil transforms on derived runtime copies; that would be a master-plan-level architecture change.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.104 (104)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Codemagic must pass the hash-pinned official Harmony 2.4.2 normalizer regression and the complete host suite before publish. Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
