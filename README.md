# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27.0 / `0.0.84 (84)` remains the furthest clean Step-27 execution result at **17/25 (A–Q PASS)** before implicit `HarmonyLib.AccessTools` initialization failed. Builds `0.0.85`–`0.0.87` then safely refined the exact AccessTools metadata fingerprint.

Physical `0.0.88 (88)` exposed repeated abrupt termination around N–Q plus one expected stale-process Gate-A rejection. Step 27.0.5 / `0.0.89 (89)` then added durable crash telemetry and localized the hard termination precisely to **Gate S / S1**, inside the exact `PatchProcessor.AddPrefix(MethodInfo)` reflection invocation and before the first `Patch()` call.

## Active candidate

**Step 27.0.6 / `0.0.90 (90)`** keeps the same 26-gate launcher-only patch objective but replaces the physically crashing `AddPrefix(MethodInfo)` convenience path with an exact bounded descriptor-registration equivalent for the deliberately Harmony-annotation-free launcher prefix.

- `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` is synchronously flushed at run start, every gate START/PASS/FAIL, ordinary gate progress, and sensitive O/R/S/T substages.
- Gate O retains the exact 57-instruction AccessTools metadata fingerprint and bounded runtime reflection, but it no longer invokes `RuntimeInformation.FrameworkDescription` through `PropertyInfo.GetValue`.
- Gate R now owns that first reflected getter invocation immediately before the explicit `RuntimeHelpers.RunClassConstructor(HarmonyLib.AccessTools.TypeHandle)` barrier.
- Gate S does **not** invoke `AddPrefix(MethodInfo)` or `HarmonyMethod(MethodInfo)`. It constructs exact `HarmonyMethod()`, verifies `priority=-1`/`method=null`, assigns only the exact launcher Prefix `MethodInfo`, then assigns only `PatchProcessor.prefix`; S1–S5 are crash-checkpointed.
- Gate T remains the first actual `PatchProcessor.Patch()` call.
- The fresh-process rule is explicit: once Gate B starts, force-quit before every Step-27 retry, even if the run stops before patching.
- The top launcher banner remains release-synchronized through `CurrentReleasePresentation` plus bundle-derived version text.

StS2 reflection/patching/invocation, broad Harmony discovery, Godot/game startup, and native game-library loading remain forbidden.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.90 (90)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
