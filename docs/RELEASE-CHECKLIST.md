# Release Checklist — Step 27.0.7

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve physical Step-27 refinement evidence through the 0.0.89 Gate-S/S1 `AddPrefix(MethodInfo)` hard crash.
- Preserve physical 0.0.90 Gate-T/T1 evidence: bounded prefix descriptor completed far enough to enter the first exact public `PatchProcessor.Patch()` invocation; no T2 survived; launcher target remained uninvoked.
- Step 27.0.7 keeps the 26-gate launcher-only patch boundary and the 0.0.90 Gate-S descriptor path intact.
- Gate O additionally metadata-audits exact `HarmonySharedState`, replacement-generation, detour, and shared-state-update internals and preflights only the bounded Reflection.Emit/MethodHandle framework surface used by that receipt-backed chain.
- Gate T1/T2 explicitly initializes/validates `HarmonySharedState` and requires version 102 before public patching; T3/T4 invoke exactly one public `PatchProcessor.Patch()`; T5 validates replacement/isolation.
- Public `PatchProcessor.Patch()` is not bypassed; no internal patch-engine operation is called as a substitute for acceptance.
- Synchronous crash checkpoints cover every gate transition and sensitive O/R/S/T substages; candidate Gate T has T1–T5.
- No protected Step 23/24/25/26 behavior file is weakened.
- `TrimMode=full`, `MtouchInterpreter=-all`, established roots/preservation policies remain active; `UseInterpreter=true` and NativeAOT remain prohibited.
- New patch-engine preservation is bounded `DynamicDependency` member preservation, not a broad Reflection.Emit assembly root.
- StS2 reflection/patching/invocation, broad Harmony discovery, Godot startup, native game libraries remain absent.
- The master document is unchanged; routine candidate/physical evidence is recorded in current status/history.

## Build identity / visible app identity

- version: `0.0.91 (91)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.7**, bundle-derived **Version 0.0.91**, current short description/status.
- validator rejects stale prior Step-27 candidate identity.

## Device-run discipline

- Force-quit/relaunch before the run.
- Once Gate B starts, force-quit before any retry, regardless of failure gate.
- If the app terminates without a managed report, preserve `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` before another attempt.
- Interpret the final checkpoint causally:
  - last T1 => inside explicit `HarmonySharedState::.cctor`;
  - T2 then last T3 => inside public `PatchProcessor.Patch()` after shared state was proven initialized;
  - T4 => Patch returned and replacement validation is running;
  - T5 => Gate T fully crossed.
- If Gate T or later runs, additionally assume launcher probe/shared patch state may remain process-resident.

## Authority

Require static validation, host tests, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
