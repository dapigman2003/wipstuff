# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `python3 tools/validate_current.py`.

The validator protects the physically closed Step 23–26 boundaries, preserves Step-27 physical 17/25 and successive 14/26 evidence, records the 0.0.88 unstable N–Q observation/fresh-process rejection plus the physical 0.0.89 Gate-S/S1 hard-crash checkpoint, and separately pins Step 27.0.6 / `0.0.90 (90)`. `TrimMode=full`, `MtouchInterpreter=-all`, Step-22 roots, proven `System.Collections.Concurrent`, and the proven Step-25 constructor preservation anchor remain mandatory.

Step 27 still has exactly **26 fail-fast gates A–Z**. Gate O keeps the exact measured 57-instruction AccessTools initializer and bounded host-framework metadata preflight, but does **not** invoke `FrameworkDescription`. Gate R owns the reflected getter invocation plus explicit AccessTools initializer. Gate S uses the bounded no-annotation `HarmonyMethod()` + exact field-assignment path and does not invoke `AddPrefix(MethodInfo)`/`HarmonyMethod(MethodInfo)`. Gate T remains the first intentional `PatchProcessor.Patch()` call. No StS2 member may be reflected, patched, or invoked.

`Step27-CrashCheckpoint.txt` must be synchronously flushed at run start, each gate START/PASS/FAIL, normal progress, and sensitive O/R/S/T substages.

## Host tests

Run `bash scripts/test.sh`.

Host tests enforce A–Z ordering, the launcher probe, and a synthetic AccessTools fixture matching the physically measured runtime-detection/cache initializer. Drift in either `Type.GetType` bool or the lock recursion policy must fail closed.

Expected TRX: `artifacts/test-results/step27.trx`.

## Codemagic / physical acceptance

Workflow: `ios-step-27`. Expected version: `0.0.90 (90)`. Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`.

Start from a force-quit/relaunch and require A–Z **26/26 PASS**, OfflineReady PASS, Foundation 5/5 PASS. Once Gate B starts, force-quit before any retry. If the process terminates without a managed report, preserve `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` before another run.
