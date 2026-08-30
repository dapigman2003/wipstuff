# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 35.0.9 reports

`Documents/StS2Launcher/Reports/Step35-CurrentRun.txt`

Current-run manifest written before Gate A. It records Run ID, initialization UTC, PID, app version/build, and exact run-specific journal/static-map filenames.

`Documents/StS2Launcher/Reports/Step35-LastCheckpoint.txt`

Overwrite-on-each-event convenience file, independently flushed after the run-specific journal append. It must carry the same Run ID as the journal/static map.

`Documents/StS2Launcher/Reports/Step35-CrashCheckpoint-<RunId>.txt`

Run-specific synchronously flushed journal. For 0.0.132 the decisive new markers are:

- `INMETHOD_024` — `NullPlatformUtilStrategy..ctor entered`;
- `INMETHOD_NPxxx_PRE` — immediately before a specific non-base `call`/`callvirt`/`newobj` in that constructor;
- `INMETHOD_NPxxx_POST` — immediately after that same call-like instruction returns;
- preserved downstream `INMETHOD_025/026`, `180/181`, and `182/183` markers remain available if execution advances beyond the constructor.

`Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`

Run-specific static map generated from the **exact closed transformed source** after semantic checks and before CLR admission. 0.0.132 adds a `[NULL PLATFORM CTOR IL]` section with exact constructor token/IL and `CALLSITE#xxx` ordinals. The NP marker ordinal is designed to map directly to these callsites.

`Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

Normal managed report written only if managed control returns to the UI `finally`. It may be absent after a hard termination. Any 0.0.132 4/4 result is diagnostic completion only, **not Step-35 closure**.

## How to interpret 0.0.132

After a hard termination, use `Step35-CurrentRun.txt` first and confirm Run ID/PID match across artifacts. Then use the last durable NP marker and the same-run `[NULL PLATFORM CTOR IL]` section:

- final `NPxxx_PRE` without `NPxxx_POST`: failure while entering/executing that exact call/newobj;
- final `NPxxx_POST`: that call returned; inspect the next marker or subsequent entry marker;
- `INMETHOD_024` with no NP marker: failure before the first swept non-base call;
- `INMETHOD_025` or later: constructor localization is passed and downstream markers regain authority.

Resolver events remain context/frontier evidence and are not root-cause attribution by themselves. A matching `.ips` is useful independent evidence when available but is not required for this marker experiment.

## Preserved Step-35 physical evidence

- `docs/history/reports/STEP-35.0-PHYSICAL-HARD-TERMINATION-SUMMARY.txt` — 0.0.123 hard termination.
- `docs/history/reports/STEP-35.0.1-PHYSICAL-EXECUTEVERYEARLY-INVOKE-CRASH-LOCALIZATION.txt` — 0.0.124 exact invocation localization.
- `docs/history/reports/STEP-35.0.2-PHYSICAL-REPEATED-HARD-TERMINATION-AND-TELEMETRY-CORRELATION.txt` — 0.0.125 repeated failure/correlation gap.
- `docs/history/reports/STEP-35.0.3-PHYSICAL-SAME-RUN-CORRELATION-AND-INVOKE-FRONTIER.txt` — 0.0.126 same-run exact frontier.
- `docs/history/reports/STEP-35.0.4-PHYSICAL-GATE-A-CECIL-WRITE-RESOLUTION-FAILURE.txt` — 0.0.127 Gate-A writer failure.
- `docs/history/reports/STEP-35.0.5-PHYSICAL-GATE-A-DEFERRED-OPEN-FAILURE.txt` — 0.0.128 Immediate-read ordering failure.
- `docs/history/reports/STEP-35.0.7-PHYSICAL-SAVEMANAGER-LOCALIZATION-0.0.130.txt` — 0.0.130 working bridge and SaveManager-path frontier.
- `docs/history/reports/STEP-35.0.8-PHYSICAL-NULLPLATFORM-CONSTRUCTOR-LOCALIZATION-0.0.131.txt` — 0.0.131 physical frontier inside `NullPlatformUtilStrategy..ctor`, before `GodotFileIo..ctor`.

Raw `.ips` files are intentionally not stored in source because they can contain device-specific identifiers.
