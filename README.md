# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Physical Step 27 builds progressively localized the iOS/AOT compatibility frontier: `0.0.89 (89)` hard-terminated inside `PatchProcessor.AddPrefix(MethodInfo)`, `0.0.90 (90)` advanced into the first exact public `PatchProcessor.Patch()` invocation and hard-terminated there, and `0.0.91 (91)` then failed cleanly at Gate O because its newly expanded `HarmonySharedState` runtime reflection changed resolver/load counters before Gate T could run.

## Active candidate

**Step 27.0.8 / `0.0.92 (92)`** keeps the same 26-gate launcher-only patch objective and corrects the 0.0.91 Gate-O regression without weakening its purity checks.

- Gate O retains the broader receipt-backed `HarmonySharedState -> MethodCreator -> MonoMod detour -> UpdatePatchInfo` **Cecil metadata audit**, but restores runtime reflection to the physically passing 0.0.90 PatchProcessor/HarmonyMethod/AccessTools surface.
- Gate T1/T2 measures the bounded host `Reflection.Emit`/`RuntimeMethodHandle` preservation preflight while requiring unchanged private-context membership, bytes, probe counters, and native/rejected-request state.
- Gate T3/T4 performs and measures the exact `HarmonySharedState` runtime Type/.cctor/version-field reflection that 0.0.91 proved has loader effects; those exact resolver/load deltas are recorded rather than globally ignored.
- Gate T5/T6 explicitly initializes and validates `HarmonySharedState` (`internalVersion/actualVersion == 102`) with the bounded generated-assembly policy.
- Gate T7/T8 still invokes **exactly one** public `PatchProcessor.Patch()`; T9 validates the replacement and snapshots the exact resulting context state for U–Z. The launcher target is not invoked until Gate V.
- `TrimMode=full` and `MtouchInterpreter=-all` remain unchanged; broad `UseInterpreter=true` and NativeAOT remain prohibited.
- Crash checkpoints continue at every gate plus sensitive O/R/S/T substages.
- The fresh-process rule remains mandatory after any attempt reaches Gate B.

StS2 reflection/patching/invocation, broad Harmony discovery, Godot/game startup, and native game-library loading remain forbidden. The master document is unchanged; current/candidate evidence is recorded in Step-27 status/history documents.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.92 (92)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Require **26/26**, then OfflineReady PASS and Foundation 5/5.
