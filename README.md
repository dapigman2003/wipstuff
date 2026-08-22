# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Physical Step 27 builds progressively localized the iOS/AOT compatibility frontier: `0.0.89 (89)` hard-terminated inside `PatchProcessor.AddPrefix(MethodInfo)`, and `0.0.90 (90)` successfully bypassed only that annotation-import convenience path, reached **Gate T / T1**, then hard-terminated inside the first exact public `PatchProcessor.Patch()` invocation before the launcher target was invoked.

## Active candidate

**Step 27.0.7 / `0.0.91 (91)`** keeps the same 26-gate launcher-only patch objective and preserves A–S from 0.0.90. It decomposes the newly measured patch-engine frontier instead of bypassing it.

- Gate O now audits the exact receipt-backed `HarmonySharedState -> MethodCreator -> MonoMod detour -> UpdatePatchInfo` chain in addition to the public patch surface and physically traversed AccessTools initializer.
- Gate O also preflights only the bounded host `Reflection.Emit`/`RuntimeMethodHandle` members used by that audited post-publish code.
- Gate T1/T2 explicitly initializes and validates `HarmonySharedState` (`internalVersion/actualVersion == 102`) before public patching, admitting only the exact Harmony/MonoMod runtime-generated singleton/proxy assembly names and rejecting any other context mutation.
- Gate T3/T4 still invokes **exactly one** public `PatchProcessor.Patch()`; T5 validates the replacement and snapshots the exact resulting context state for U–Z. The launcher target is not invoked until Gate V.
- `TrimMode=full` and `MtouchInterpreter=-all` remain unchanged; broad `UseInterpreter=true` and NativeAOT remain prohibited.
- Crash checkpoints continue at every gate plus sensitive O/R/S/T substages.
- The fresh-process rule remains mandatory after any attempt reaches Gate B.

StS2 reflection/patching/invocation, broad Harmony discovery, Godot/game startup, and native game-library loading remain forbidden. The master document is unchanged; current/candidate evidence is recorded in Step-27 status/history documents.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.91 (91)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
