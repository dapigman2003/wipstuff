# Historical Reports

Selected build/device diagnostic outputs are retained here when they materially explain an architectural decision. Step records under `../steps/` remain the primary readable history.
- `STEP-25.0.1-PHYSICAL-GATE-H-REPORT.txt` — physical Step 25.0.1 / 0.0.81 report: Gates A–G PASS, Gate H FAIL at exact Harmony(string) invocation with missing `System.Environment.get_Version()`.
- `STEP-27.0.4-PHYSICAL-FRESH-PROCESS-GUARD-REPORT.txt` — physical 0.0.88 same-process retry rejected at Gate A because `sts2` remained resident in the dedicated Step-27 context; separate user observation reports abrupt process termination around N–Q with no managed report.
