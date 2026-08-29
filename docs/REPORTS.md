# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 35.0.3 reports

`Documents/StS2Launcher/Reports/Step35-CurrentRun.txt`

Small current-run manifest written before Gate A. It records Run ID, initialization UTC, PID, app version/build, and the exact run-specific crash-journal/static-map filenames. Use this file first when collecting evidence after a termination.

`Documents/StS2Launcher/Reports/Step35-LastCheckpoint.txt`

Overwrite-on-each-event convenience file. It is independently flushed after the run-specific journal append and records the same Run ID plus the latest durable checkpoint line. It is not a substitute for the full run-specific journal.

`Documents/StS2Launcher/Reports/Step35-CrashCheckpoint-<RunId>.txt`

Run-specific synchronously flushed crash-localization journal. Each event records UTC, Run ID, process ID, managed thread ID, and the B/C/resolver frontier marker. The filename is never reused within a run.

`Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`

Run-specific static map generated after exact source/transformed semantic checks and before CLR admission. It contains the exact transformed `ExecuteVeryEarly` wrapper and async `MoveNext` IL/callsite map, metadata scopes, numbered callsites, and await-registration candidates. It carries the same Run ID/PID as the journal and is never runtime input.

`Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

Normal deterministic managed report. It is produced only when managed control reaches the UI `finally`; it may be absent after a native/runtime/kernel hard termination.

## Preserved Step-35 physical evidence

- `docs/history/reports/STEP-35.0-PHYSICAL-HARD-TERMINATION-SUMMARY.txt` — sanitized 0.0.123 main-thread PC=0x0 hard termination.
- `docs/history/reports/STEP-35.0.1-PHYSICAL-EXECUTEVERYEARLY-INVOKE-CRASH-LOCALIZATION.txt` — sanitized 0.0.124 evidence proving Gate B PASS and hard termination inside synchronous ExecuteVeryEarly invocation after planned resolver activity.
- `docs/history/reports/STEP-35.0.2-PHYSICAL-REPEATED-HARD-TERMINATION-AND-TELEMETRY-CORRELATION.txt` — sanitized 0.0.125 repeated hard-kill evidence and proof that the available static map/crash report came from different runs.

Raw `.ips` files are intentionally not stored in source because they can contain device-specific identifiers.

## Latest physical closures

- `docs/history/reports/STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.122 Step 34 4/4 exact transformed `PrewarmJit()` execution.
- `docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.121 Step 33 4/4 transformed-primary-only CLR admission.
- `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.120 Step 32 4/4 exact private real-StS2 semantic rewrite.
