# Historical Reports

Selected build/device diagnostic outputs are retained here when they materially explain an architectural decision. Step records under `../steps/` remain the primary readable history.
- `STEP-25.0.1-PHYSICAL-GATE-H-REPORT.txt` — physical Step 25.0.1 / 0.0.81 report: Gates A–G PASS, Gate H FAIL at exact Harmony(string) invocation with missing `System.Environment.get_Version()`.
- `STEP-27.0.4-PHYSICAL-FRESH-PROCESS-GUARD-REPORT.txt` — physical 0.0.88 same-process retry rejected at Gate A because `sts2` remained resident in the dedicated Step-27 context; separate user observation reports abrupt process termination around N–Q with no managed report.

- `STEP-27.0.5-PHYSICAL-GATE-S-CRASH-CHECKPOINT.txt` — raw 0.0.89 synchronously flushed breadcrumb localizing the hard crash to Gate S/S1 inside `PatchProcessor.AddPrefix(MethodInfo)` before `Patch()`.
- `STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt` — raw 0.0.109 Codemagic host/build output: static validation and external fixtures succeeded, then Core compilation stopped on CS0246 for missing `CallbackProgress<>` before MSTest/iOS publish.
- `STEP-28.0.2-PHYSICAL-CLOSURE.txt` — raw physical 0.0.111 Step-28 report: A–E 5/5, transformed execution 1000 / 1041 / 1041, transformed-only identity admission, OfflineReady 428/428 after execution.
