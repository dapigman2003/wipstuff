# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `bash scripts/validate.sh`.

The validator protects Steps 23–26 and all Step-27 evidence through physical 0.0.97. For Step 27.0.18 / `0.0.102 (102)`, it must prove that the production normalizer uses `ReadingMode.Deferred` for source and post-write audit, contains no `ReadingMode.Immediate`, retains the fail-closed metadata resolver, preserves the exact eleven Cecil-opcode rewrite, keeps canonical-vs-synthetic scope separation, requires the explicit Cecil custom-attribute-provider alias in the real-Harmony regression, requires the official fat-release net9.0 structural-surrogate fixture outside MSBuild and labels it non-authoritative for production, and preserves the later single public `PatchProcessor.Patch()` boundary.

The physical 0.0.97 report must remain archived and fingerprinted as the evidence for the `System.ComponentModel.EditorBrowsableState` eager-resolution failure.

## Host tests

Run `bash scripts/test.sh`.

In addition to the synthetic gate tests, the canonical host-test script downloads the exact tagged official `Harmony-Fat.2.4.2.0.zip` release asset to `artifacts/host-step27-fixtures`. Codemagic 0.0.101 proved that archive has no netstandard2.0 implementation, so the script now requires exactly one `net9.0/0Harmony.dll` member at archive root or under a release wrapper and treats it only as a **host structural surrogate**. It retains the original archive member name for extraction, prints all `0Harmony.dll` members on mismatch, records archive/DLL SHA-256 values, exports its absolute path as `STS2_STEP27_REAL_HARMONY_FIXTURE`, and calls the unchanged private production `CreateIosNormalizedHarmonyRuntimeImage` helper. The test project contains no Harmony PackageReference/PackageDownload and the surrogate cannot replace the exact on-device production metadata fingerprint.

That regression requires:

- exact `0Harmony` / `2.4.2.0` identity;
- net9 implementation profile: no `netstandard` reference and `System.Runtime, Version=9.0.0.0`;
- an `EditorBrowsableAttribute` somewhere on the real metadata surface, detected without reading constructor arguments;
- successful normalization under the rejecting metadata resolver;
- original fixture bytes unchanged;
- source SHA and runtime-image SHA different;
- normalized cctor audit contains `instructions=11`.

This test is specifically intended to catch the class of failure that 0.0.95–0.0.97 allowed to reach Codemagic/device because synthetic fixtures did not reproduce the real Harmony metadata shape.

Expected TRX: `artifacts/test-results/step27.trx`.

## Codemagic / physical acceptance

Workflow: `ios-step-27`. Expected version: `0.0.102 (102)`. Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`.

Codemagic must pass compilation and the complete host suite before publish. Start the physical run from a force-quit/relaunch and require A–Z **26/26 PASS**, OfflineReady PASS, Foundation 5/5 PASS. Once Gate B starts, force-quit before any retry. If the process terminates without a managed report, preserve `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` before another run.

The key physical substages are T5a (runtime image reverified), T5b (normalized cctor entered), T6 (normalized cctor returned and state validation started), then T7/T8/T9 for the public Patch boundary.

If T6 passes but T7/T8 fails, the next test target is a project-owned post-publish interpreted assembly so runtime patching is evaluated against the same managed execution class as eventual dynamically loaded game IL. If that representative target also fails, Harmony runtime detouring is no longer the default direction; propose ahead-of-load Cecil transformation and update the master plan before implementation.
