# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27.0 / `0.0.84 (84)` reached **17/25** (A–Q PASS) before implicit `HarmonyLib.AccessTools` initialization failed. Builds `0.0.85`–`0.0.87` then stopped safely at Gate O while measuring the exact AccessTools initializer. Physical `0.0.87 (87)` confirmed the **57-instruction** fingerprint but disproved the previous operand attribution: both `RuntimeInformation` `Type.GetType(string,bool)` probes use `throwOnError=false`; the single required `ldc.i4.1` instead supplies `LockRecursionPolicy.SupportsRecursion` to `ReaderWriterLockSlim`.

## Active candidate

**Step 27.0.4 / `0.0.88 (88)`** keeps the launcher-only patch objective unchanged.

- Gate O pins the corrected 57-instruction AccessTools fingerprint and exact operand semantics: both `RuntimeInformation` probes use `false`; the lock constructor uses `SupportsRecursion (1)`.
- The bounded `DynamicDependency` preservation anchor remains unchanged.
- Gate R explicitly runs the measured AccessTools initializer and audits runtime/cache state.
- Gate S only registers the launcher prefix.
- Gate T remains the first actual `PatchProcessor.Patch()` call.
- Gates U–Z audit, execute the patched launcher probe, unpatch, prove restoration, and finish with integrity/isolation.
- The top launcher banner is release-synchronized: step/summary come from `CurrentReleasePresentation`, while version comes from the built bundle.

StS2 reflection/patching/invocation, broad Harmony discovery, Godot/game startup, and native game-library loading remain forbidden.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.88 (88)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
