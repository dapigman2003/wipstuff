# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27.0 / `0.0.84 (84)` remains the furthest clean Step-27 execution result at **17/25 (A–Q PASS)** before implicit `HarmonyLib.AccessTools` initialization failed. Builds `0.0.85`–`0.0.87` then safely refined the exact AccessTools metadata fingerprint.

Physical `0.0.88 (88)` exposed a different problem: repeated abrupt app termination was observed around the N–Q region, while one later retry correctly failed Gate A because the previous Step-27 `sts2`/Harmony load context was still resident in that process. That Gate-A result is expected once any prior attempt reached Gate B.

## Active candidate

**Step 27.0.5 / `0.0.89 (89)`** keeps the same 26-gate launcher-only patch objective but improves crash attribution and restores a cleaner execution boundary.

- `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` is synchronously flushed at run start, every gate START/PASS/FAIL, ordinary gate progress, and sensitive O/R/S/T substages.
- Gate O retains the exact 57-instruction AccessTools metadata fingerprint and bounded runtime reflection, but it no longer invokes `RuntimeInformation.FrameworkDescription` through `PropertyInfo.GetValue`.
- Gate R now owns that first reflected getter invocation immediately before the explicit `RuntimeHelpers.RunClassConstructor(HarmonyLib.AccessTools.TypeHandle)` barrier.
- Gate S remains exact prefix registration only.
- Gate T remains the first actual `PatchProcessor.Patch()` call.
- The fresh-process rule is explicit: once Gate B starts, force-quit before every Step-27 retry, even if the run stops before patching.
- The top launcher banner remains release-synchronized through `CurrentReleasePresentation` plus bundle-derived version text.

StS2 reflection/patching/invocation, broad Harmony discovery, Godot/game startup, and native game-library loading remain forbidden.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.89 (89)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
