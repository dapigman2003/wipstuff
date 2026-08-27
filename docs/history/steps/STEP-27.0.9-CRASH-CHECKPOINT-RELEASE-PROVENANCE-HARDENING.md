# Step 27.0.9 — Crash-checkpoint release-provenance hardening

Candidate: `0.0.93 (93)`

## Evidence entering this candidate

A newly supplied Step-27 crash checkpoint was generated on 2026-08-22 and reports:

`Gate S progress: S1 — entering exact PatchProcessor.AddPrefix(MethodInfo) reflection invocation.`

That S1 text is the exact archived 0.0.89 Gate-S implementation marker preserved in `STEP-27.0.5-PHYSICAL-GATE-S-CRASH-CHECKPOINT.txt`. It is not emitted by candidate 0.0.92 executable source. In 0.0.92, Gate S enters exact parameterless `HarmonyMethod()` reflection construction and explicitly states that `AddPrefix(MethodInfo)` and `ImportMethod` are not invoked.

The legacy crash-checkpoint schema records timestamp, process ID, phase, gate, and detail, but not app version/build or candidate identity. A fresh timestamp therefore proves only that some installed Step-27 binary wrote the file; it does not bind that file to the uploaded 0.0.92 source. The supplied observation is classified as a release-provenance conflict, not a new Step-27 runtime frontier.

## Correction principle

Do not change the Harmony patch path in response to evidence that cannot be attributed to the current candidate. Instead, make future physical crash evidence self-identifying and fail closed if the built bundle identity and source candidate disagree.

This preserves the existing rigorous gate model: runtime behavior changes only when current, attributable physical evidence requires them.

## Runtime behavior

Step 27.0.9 intentionally leaves the 0.0.92 patch-engine path unchanged:

- Gate O retains the physically passing 0.0.90 runtime-reflection surface plus receipt-backed HarmonySharedState/replacement/detour Cecil audit.
- Gate R explicitly initializes the measured AccessTools runtime-detection/cache state.
- Gate S uses exact `HarmonyMethod()` construction, verifies `priority=-1` and `method=null`, assigns only the launcher Prefix `MethodInfo`, then assigns only `PatchProcessor.prefix`.
- `PatchProcessor.AddPrefix(MethodInfo)`, `HarmonyMethod(MethodInfo)`, and `ImportMethod` remain metadata/reference-audited but uninvoked.
- Gate T retains the 0.0.92 T1–T9 decomposition through host preservation preflight, HarmonySharedState runtime reflection, explicit shared-state initialization, one public `PatchProcessor.Patch()` call, and replacement/isolation validation.
- No StS2 type/member is reflected, patched, or invoked.

## New fail-closed release identity check

Before Gate A starts, the iOS UI requires the installed bundle identity to match the source-pinned candidate identity:

- display version `0.0.93`
- build version `93`

If either differs, Step 27 writes an `IDENTITY_FAIL` crash checkpoint and runs no gate.

This does not attempt to prove that an older installed binary is current. Its purpose is to prevent a mixed or incorrectly packaged 0.0.93 build from being treated as valid current-source evidence.

## Crash-checkpoint provenance schema

Every Step-27 crash checkpoint now includes, before phase/gate/detail:

- installed app version/build;
- expected source version/build;
- active Step-27 candidate title; and
- exact Gate-S implementation marker: bounded `HarmonyMethod()` descriptor, `PatchProcessor.AddPrefix(MethodInfo)` runtime invocation forbidden.

Therefore any future checkpoint attributed to 0.0.93 must contain those lines. A file containing the legacy AddPrefix S1 text without the new provenance lines is not 0.0.93 evidence, regardless of timestamp.

## Physical acceptance

From a force-quit/relaunch, install and visibly confirm `0.0.93 (93)`, then run Step 27. If the process terminates abruptly, preserve `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` before another attempt and require its provenance lines to identify 0.0.93 before using its gate/substage as current physical evidence.

If the run continues normally, the unchanged acceptance target remains A–Z `26/26 PASS`, followed by OfflineReady PASS and Foundation 5/5 PASS.

## Still forbidden

No StS2 type/member reflection, patching, or invocation; no StS2 entry point; no Harmony broad discovery/PatchAll/category/class processor; no game/Godot startup; no native game-library load; no trusted/prepared-byte mutation; no broad interpreter or trimming-policy relaxation.

## Documentation policy

This is a routine Step-27 diagnostic/provenance refinement. `docs/MASTER-PLAN.md` is intentionally unchanged.
