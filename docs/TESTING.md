# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `python3 tools/validate_current.py`.

The validator protects the physically closed Step 23–26 boundaries, preserves the Step-27 physical 17/25 and 14/26 evidence, and separately pins Step 27.0.3 / `0.0.87 (87)`. `TrimMode=full`, `MtouchInterpreter=-all`, Step-22 roots, proven `System.Collections.Concurrent`, and the proven Step-25 constructor preservation anchor remain mandatory.

Step 27 has exactly **26 fail-fast gates A–Z**. Gate O admits only the exact measured 57-instruction AccessTools initializer plus the bounded host-framework preservation preflight. Gate R is its sole execution barrier. Gate T remains the first intentional `PatchProcessor.Patch()` call. No StS2 member may be reflected, patched, or invoked.

## Host tests

Run `bash scripts/test.sh`.

Host tests enforce A–Z ordering, the launcher probe, and a synthetic AccessTools fixture matching the physically measured runtime-detection/cache initializer; drift must fail closed.

Expected TRX: `artifacts/test-results/step27.trx`.

## Codemagic / physical acceptance

Workflow: `ios-step-27`. Expected version: `0.0.87 (87)`. Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`.

From a fresh process require A–Z **26/26 PASS**, OfflineReady PASS, Foundation 5/5 PASS. Gate R must explicitly complete AccessTools with exact measured state; Gate T must be the first patch call; V must return 1041 through reflection/direct routes; W unpatches; Y restores 42; Z finishes cleanly. After Gate T or later, force-quit before retrying.
