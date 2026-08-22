# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27.0 / `0.0.84 (84)` reached **17/25** (A–Q PASS) before implicit `HarmonyLib.AccessTools` initialization failed. Step 27.0.1 / `0.0.85 (85)` then failed safely at Gate O **14/26**, revealing that the real AccessTools initializer is a 56-instruction runtime-detection/cache initializer rather than the assumed BindingFlags-only shape.

## Active candidate

**Step 27.0.2 / `0.0.86 (86)`** keeps the launcher-only patch objective unchanged.

- Gate O now pins the exact physically measured AccessTools initializer fingerprint and proves its string-reflected `RuntimeInformation.FrameworkDescription` plus cache/lock framework surface survived trimming.
- A candidate-only `DynamicDependency` anchor preserves only that bounded AccessTools framework surface.
- Gate R explicitly runs the measured AccessTools initializer and audits its runtime/cache state.
- Gate S still only registers the launcher prefix.
- Gate T remains the first actual `PatchProcessor.Patch()` call.
- Gates U–Z audit, execute the patched launcher probe, unpatch, prove restoration, and finish with integrity/isolation.

StS2 reflection/patching/invocation, broad Harmony discovery, Godot/game startup, and native game-library loading remain forbidden.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.86 (86)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
