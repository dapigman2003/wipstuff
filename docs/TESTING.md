# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `python3 tools/validate_current.py`.

The validator protects the physically closed Step 23–26 boundaries and Step-27 refinement evidence through the physical `0.0.94` T5 observer checkpoint. Step 27.0.11 / `0.0.95 (95)` retains all 26 A–Z gates while introducing one bounded runtime-image compatibility substitution for the exact admitted Harmony 2.4.2 `HarmonySharedState::.cctor`. `TrimMode=full`, `MtouchInterpreter=-all`, Step-22 roots, proven `System.Collections.Concurrent`, and existing Step-25/27 preservation anchors remain mandatory.

Static validation pins all of the following: exact source patch-engine metadata admission before normalization; one in-memory Cecil rewrite; unchanged managed assembly identity; exact 11-instruction normalized cctor; distinct source/runtime SHA-1; no write to receipt-backed source/live/prepared files; exact private-memory load only for admitted 0Harmony; T5a runtime-image rehash; one T5b `RunClassConstructor`; T6 direct-state/null-methodAddress/version/generated-assembly validation; and exactly one later public `PatchProcessor.Patch()` call. No StS2 member may be reflected, patched, or invoked.

`Step27-CrashCheckpoint.txt` must be synchronously flushed at run start, each gate START/PASS/FAIL, normal progress, and sensitive O/R/S/T substages.

## Host tests

Run `bash scripts/test.sh`.

Host tests enforce A–Z ordering, launcher-probe invariants, AccessTools metadata, and fail-closed patch-engine metadata admission. The current local execution environment may not contain the .NET SDK; absence of `dotnet` is not recorded as a PASS and Codemagic must execute the suite before installation.

Expected TRX: `artifacts/test-results/step27.trx`.

## Codemagic / physical acceptance

Workflow: `ios-step-27`. Expected version: `0.0.95 (95)`. Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`.

Start from a force-quit/relaunch and require A–Z **26/26 PASS**, OfflineReady PASS, Foundation 5/5 PASS. Once Gate B starts, force-quit before any retry. If the process terminates without a managed report, preserve `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` before another run.

For 0.0.95, require the checkpoint to identify `App version: 0.0.95 (95)`, the Step 27.0.11 candidate, the bounded Gate-S implementation, and the Gate-T 11-instruction normalized-cctor implementation. The key next substages are T5a (runtime image reverified), T5b (normalized cctor entered), T6 (normalized cctor returned and state validation started), and then T7/T8/T9 for the public Patch() boundary.
