# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27.0 / `0.0.84 (84)` reached **17/25** (A–Q PASS) before implicit `HarmonyLib.AccessTools` initialization failed. Step 27.0.1 / `0.0.85 (85)` then failed safely at Gate O **14/26**, exposing the broader AccessTools runtime-detection/cache initializer. Step 27.0.2 / `0.0.86 (86)` also failed safely at Gate O **14/26** and corrected the exact receipt-backed fingerprint to **57 instructions**, including one required `ldc.i4.1` for the first `RuntimeInformation` `Type.GetType(string,bool)` probe.

## Active candidate

**Step 27.0.3 / `0.0.87 (87)`** keeps the launcher-only patch objective unchanged.

- Gate O pins the corrected 57-instruction physical AccessTools fingerprint, including exact first-`true` / second-`false` `Type.GetType(string,bool)` operands, and proves its bounded string-reflected `RuntimeInformation.FrameworkDescription` plus cache/lock framework surface survived trimming.
- The existing candidate-only `DynamicDependency` anchor remains bounded to that AccessTools framework surface.
- Gate R explicitly runs the measured AccessTools initializer and audits its runtime/cache state.
- Gate S still only registers the launcher prefix.
- Gate T remains the first actual `PatchProcessor.Patch()` call.
- Gates U–Z audit, execute the patched launcher probe, unpatch, prove restoration, and finish with integrity/isolation.
- The top launcher banner now comes from one current-release presentation definition; its version is read from the built bundle, and static validation rejects stale candidate text.

StS2 reflection/patching/invocation, broad Harmony discovery, Godot/game startup, and native game-library loading remain forbidden.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.87 (87)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
