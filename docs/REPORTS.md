# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 35.0.5 reports

`Documents/StS2Launcher/Reports/Step35-CurrentRun.txt`

Small current-run manifest written before Gate A. It records Run ID, initialization UTC, PID, app version/build, and the exact run-specific crash-journal/static-map filenames. Use this file first when collecting evidence after a termination.

`Documents/StS2Launcher/Reports/Step35-LastCheckpoint.txt`

Overwrite-on-each-event convenience file. It is independently flushed after the run-specific journal append and records the same Run ID plus the latest durable checkpoint line. It is not a substitute for the full run-specific journal.

`Documents/StS2Launcher/Reports/Step35-CrashCheckpoint-<RunId>.txt`

Run-specific synchronously flushed crash-localization journal. Each event records UTC, Run ID, process ID, managed thread ID, and the A/B/C/resolver/in-method frontier marker. For 0.0.128 the decisive new events are `C_DIAGNOSTIC_BRIDGE_ARMED` and `INMETHOD_*`. The filename is never reused within a run.

`Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`

Run-specific static map generated from the **exact closed transformed source** after source/transformed semantic checks and before CLR admission. It contains the exact transformed `ExecuteVeryEarly` wrapper and async `MoveNext` IL/callsite map, metadata scopes, numbered callsites, and await-registration candidates. It carries the same Run ID/PID as the journal and is never runtime input. The map describes the exact source artifact; Gate B/C of 0.0.128 execute a separately identified diagnostic clone.

`Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

Normal deterministic managed report. It is produced only when managed control reaches the UI `finally`; it may be absent after a native/runtime/kernel hard termination. Any 0.0.128 4/4 result must be labeled diagnostic completion and **not Step-35 closure**.

## How to interpret 0.0.128

After a hard termination, first use `Step35-CurrentRun.txt` to identify the same-run journal/static map. Confirm Run ID/PID match. Then inspect `Step35-LastCheckpoint.txt` and the journal tail. If `C_DIAGNOSTIC_BRIDGE_ARMED` is present, the final durable `INMETHOD_*` marker identifies the last selected instrumented game method/type initializer entered before termination. If the bridge is armed but no `INMETHOD_*` marker is durable, the bridge/first-entry boundary becomes the immediate diagnostic frontier.

The diagnostic clone preserves assembly identity/MVID but is not byte-identical to the exact closed Step-32 transformed artifact. Therefore its reports may localize a failure but cannot close exact Step 35.

## Preserved Step-35 physical evidence

- `docs/history/reports/STEP-35.0-PHYSICAL-HARD-TERMINATION-SUMMARY.txt` — sanitized 0.0.123 main-thread PC=0x0 hard termination.
- `docs/history/reports/STEP-35.0.1-PHYSICAL-EXECUTEVERYEARLY-INVOKE-CRASH-LOCALIZATION.txt` — sanitized 0.0.124 evidence proving Gate B PASS and hard termination inside synchronous exact `ExecuteVeryEarly` invocation after planned resolver activity.
- `docs/history/reports/STEP-35.0.2-PHYSICAL-REPEATED-HARD-TERMINATION-AND-TELEMETRY-CORRELATION.txt` — sanitized 0.0.125 repeated hard-kill evidence and proof that the available static map/crash report came from different runs.
- `docs/history/reports/STEP-35.0.3-PHYSICAL-SAME-RUN-CORRELATION-AND-INVOKE-FRONTIER.txt` — sanitized 0.0.126 same-run evidence proving Gate B PASS, exact invocation entry, planned resolver activity, and the final durable `System.Collections.Concurrent 8 -> 9` host-resolution event with no `C_INVOKE_RETURNED`.

Raw `.ips` files are intentionally not stored in source because they can contain device-specific identifiers.

## Latest physical closures

- `docs/history/reports/STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.122 Step 34 4/4 exact transformed `PrewarmJit()` execution.
- `docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.121 Step 33 4/4 transformed-primary-only CLR admission.
- `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.120 Step 32 4/4 exact private real-StS2 semantic rewrite.
