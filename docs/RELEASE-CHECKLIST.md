# Release Checklist — Step 27.0.9

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve physical Step-27 refinement evidence through the 0.0.89 Gate-S/S1 `AddPrefix(MethodInfo)` hard crash.
- Preserve physical 0.0.90 Gate-T/T1 evidence: bounded prefix descriptor completed far enough to enter the first exact public `PatchProcessor.Patch()` invocation; no T2 survived; launcher target remained uninvoked.
- Preserve physical 0.0.91 Gate-O evidence: A–N PASS, Gate O 14/26 managed FAIL because the newly added HarmonySharedState runtime reflection changed resolver/load counters; Gate T was not reached.
- Step 27.0.9 keeps the complete 0.0.92 26-gate launcher-only runtime path intact; no Gate O/S/T patch-engine behavior changes are admitted in this candidate.
- Treat the newly supplied fresh-timestamp AddPrefix S1 checkpoint as provenance-inconsistent with executable 0.0.92 source, not as a new physical runtime frontier.
- Step-27 execution must fail closed before Gate A if the installed bundle identity differs from the source-pinned `0.0.93 (93)`.
- Every Step-27 crash checkpoint must self-identify installed version/build, expected source version/build, active candidate, and the bounded Gate-S implementation marker.
- Gate O additionally metadata-audits exact `HarmonySharedState`, replacement-generation, detour, and shared-state-update internals, but its runtime reflection is restored to the physically passing 0.0.90 PatchProcessor/HarmonyMethod/AccessTools surface.
- The bounded Reflection.Emit/MethodHandle runtime preflight and HarmonySharedState runtime Type/field reflection are deferred to measured Gate-T substages rather than admitted silently in Gate O.
- Gate T1/T2 measure the host dynamic-code preservation preflight; T3/T4 measure exact HarmonySharedState runtime reflection; T5/T6 explicitly initialize/validate `HarmonySharedState` and require version 102; T7/T8 invoke exactly one public `PatchProcessor.Patch()`; T9 validates replacement/isolation.
- Public `PatchProcessor.Patch()` is not bypassed; no internal patch-engine operation is called as a substitute for acceptance.
- Synchronous crash checkpoints cover every gate transition and sensitive O/R/S/T substages; candidate Gate T has T1–T9.
- No protected Step 23/24/25/26 behavior file is weakened.
- `TrimMode=full`, `MtouchInterpreter=-all`, established roots/preservation policies remain active; `UseInterpreter=true` and NativeAOT remain prohibited.
- New patch-engine preservation is bounded `DynamicDependency` member preservation, not a broad Reflection.Emit assembly root.
- StS2 reflection/patching/invocation, broad Harmony discovery, Godot startup, native game libraries remain absent.
- The master document is unchanged; routine candidate/physical evidence is recorded in current status/history.

## Build identity / visible app identity

- version: `0.0.93 (93)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.9**, bundle-derived **Version 0.0.93**, current provenance-hardening description/status.
- validator rejects stale prior Step-27 candidate identity.

## Device-run discipline

- Force-quit/relaunch before the run.
- Once Gate B starts, force-quit before any retry, regardless of failure gate.
- If the app terminates without a managed report, preserve `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` before another attempt.
- Interpret the final checkpoint causally:
  - last T1 => inside bounded host Reflection.Emit/RuntimeMethodHandle preservation preflight;
  - T2 then last T3 => inside exact HarmonySharedState runtime reflection;
  - T4 then last T5 => inside explicit `HarmonySharedState::.cctor`;
  - T6 then last T7 => inside public `PatchProcessor.Patch()` after shared state was proven initialized;
  - T8 => Patch returned and replacement validation is running;
  - T9 => Gate T fully crossed.
- If Gate T or later runs, additionally assume launcher probe/shared patch state may remain process-resident.

## Authority

Require static validation, host tests, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
