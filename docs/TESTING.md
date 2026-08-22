# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `python3 tools/validate_current.py`.

The validator protects the physically closed Step 23–26 boundaries, preserves the Step-27 physical refinement history through the `0.0.89` Gate-S/S1 hard crash, records physical `0.0.90` entry into `PatchProcessor.Patch()`, records the clean physical `0.0.91` Gate-O resolver/load-counter regression, and separately pins Step 27.0.8 / `0.0.92 (92)`. `TrimMode=full`, `MtouchInterpreter=-all`, Step-22 roots, proven `System.Collections.Concurrent`, and the proven Step-25 constructor preservation anchor remain mandatory.

Step 27 still has exactly **26 fail-fast gates A–Z**. Gate O keeps the exact measured AccessTools initializer and the receipt-backed HarmonySharedState/replacement/detour chain as Cecil metadata audit, while restoring runtime reflection to the physically passing 0.0.90 surface. Gate R owns the reflected `FrameworkDescription` getter plus AccessTools initialization. Gate S keeps the annotation-free bounded `HarmonyMethod()` descriptor path. Gate T1/T2 measure the host dynamic-code preservation surface; T3/T4 measure exact HarmonySharedState runtime reflection; T5/T6 initialize/validate HarmonySharedState; T7/T8 invoke exactly one public `PatchProcessor.Patch()`; T9 validates the replacement. No StS2 member may be reflected, patched, or invoked.

`Step27-CrashCheckpoint.txt` must be synchronously flushed at run start, each gate START/PASS/FAIL, normal progress, and sensitive O/R/S/T substages.

## Host tests

Run `bash scripts/test.sh`.

Host tests enforce A–Z ordering, the launcher probe, and a synthetic AccessTools fixture matching the physically measured runtime-detection/cache initializer. The patch-engine metadata audit also has a fail-closed synthetic negative case so missing shared-state/replacement/detour internals cannot be silently admitted. Drift in either `Type.GetType` bool or the lock recursion policy must fail closed.

Expected TRX: `artifacts/test-results/step27.trx`.

## Codemagic / physical acceptance

Workflow: `ios-step-27`. Expected version: `0.0.92 (92)`. Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`.

Start from a force-quit/relaunch and require A–Z **26/26 PASS**, OfflineReady PASS, Foundation 5/5 PASS. Once Gate B starts, force-quit before any retry. If the process terminates without a managed report, preserve `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` before another run.
