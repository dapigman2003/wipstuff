# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `python3 tools/validate_current.py`.

The validator protects the physically closed Step 23–26 boundaries and Step-27 refinement history through the 0.0.93 T5 checkpoint. Step 27.0.10 / `0.0.94 (94)` retains all 26 A–Z gates and the existing patch algorithm while adding bounded in-flight observability around the exact HarmonySharedState cctor boundary. `TrimMode=full`, `MtouchInterpreter=-all`, Step-22 roots, proven `System.Collections.Concurrent`, and the existing Step-25/27 preservation anchors remain mandatory.

Gate T sequencing is pinned as follows: T1/T2 host preservation; T3/T4 exact HarmonySharedState runtime reflection; T5a observer arming; T5b unchanged `RunClassConstructor`; T6 post-cctor validation; T7/T8 exactly one public `PatchProcessor.Patch()` invocation/return; T9 replacement/isolation validation. No StS2 member may be reflected, patched, or invoked.

`Step27-CrashCheckpoint.txt` must be synchronously flushed at run start, each gate START/PASS/FAIL, normal progress, sensitive O/R/S/T substages, and each bounded T5 cctor observation.

## Host tests

Run `bash scripts/test.sh`.

Host tests enforce A–Z ordering, launcher-probe invariants, AccessTools metadata, and fail-closed patch-engine metadata admission. Static validation additionally pins that the T5 observers are armed only around the single existing `RuntimeHelpers.RunClassConstructor(harmonySharedStateType.TypeHandle)` call and are removed before later validation/Patch().

Expected TRX: `artifacts/test-results/step27.trx`.

## Codemagic / physical acceptance

Workflow: `ios-step-27`. Expected version: `0.0.94 (94)`. Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`.

Start from a force-quit/relaunch and require A–Z **26/26 PASS**, OfflineReady PASS, Foundation 5/5 PASS. Once Gate B starts, force-quit before any retry. If the process terminates without a managed report, preserve `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` before another run.

For 0.0.94, require the checkpoint to identify `App version: 0.0.94 (94)`, the Step 27.0.10 candidate, the bounded Gate-S implementation, and the Gate-T cctor-observer implementation. A last T5 observer line should be treated as a causal milestone only; do not infer an unobserved source instruction.
