# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 35.0.8 reports

`Documents/StS2Launcher/Reports/Step35-CurrentRun.txt`

Current-run manifest written before Gate A. It records Run ID, initialization UTC, PID, app version/build, and the exact run-specific crash-journal/static-map filenames.

`Documents/StS2Launcher/Reports/Step35-LastCheckpoint.txt`

Overwrite-on-each-event convenience file, independently flushed after the run-specific journal append. It must carry the same Run ID as the journal/static map.

`Documents/StS2Launcher/Reports/Step35-CrashCheckpoint-<RunId>.txt`

Run-specific synchronously flushed journal. For 0.0.131 the decisive markers include the existing `C_DIAGNOSTIC_BRIDGE_ARMED` / `INMETHOD_*` sequence plus:

- `INMETHOD_021` — `SaveManager.ConstructDefault`;
- `INMETHOD_022` — `UserDataPathProvider.GetAccountScopedBasePath`;
- `INMETHOD_023` — `PlatformUtil.get_PrimaryPlatform`;
- managed `PlatformUtil..cctor` entry when triggered;
- `INMETHOD_024` — `NullPlatformUtilStrategy..ctor`;
- `INMETHOD_025` — `GodotFileIo..ctor`;
- `INMETHOD_026` — `GodotFileIo.CreateDirectory`;
- `INMETHOD_180/181` — before/after `Godot.DirAccess.DirExistsAbsolute`;
- `INMETHOD_182/183` — before/after `Godot.DirAccess.MakeDirRecursiveAbsolute`.

`Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`

Run-specific static map generated from the **exact closed transformed source** after source/transformed semantic checks and before CLR admission. It describes the exact wrapper/async `MoveNext` artifact; Gate B/C execute a separately identified diagnostic clone.

`Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

Normal managed report written only if managed control returns to the UI `finally`. It may be absent after a hard termination. Any 0.0.131 4/4 result is diagnostic completion only, **not Step-35 closure**.

## How to interpret 0.0.131

After a hard termination, use `Step35-CurrentRun.txt` first and confirm Run ID/PID match across artifacts. The last durable marker is the physical localization result.

- final `INMETHOD_180` without `181`: failure while entering/executing `DirExistsAbsolute`;
- `181` then final `182` without `183`: failure while entering/executing `MakeDirRecursiveAbsolute`;
- both post markers present: do not blame those two calls; continue from the subsequent marker;
- resolver events remain context/frontier evidence and are not root-cause attribution by themselves.

A matching `.ips` is useful independent evidence when available but is not required for this marker experiment.

## Preserved Step-35 physical evidence

- `docs/history/reports/STEP-35.0-PHYSICAL-HARD-TERMINATION-SUMMARY.txt` — 0.0.123 hard termination.
- `docs/history/reports/STEP-35.0.1-PHYSICAL-EXECUTEVERYEARLY-INVOKE-CRASH-LOCALIZATION.txt` — 0.0.124 exact invocation localization.
- `docs/history/reports/STEP-35.0.2-PHYSICAL-REPEATED-HARD-TERMINATION-AND-TELEMETRY-CORRELATION.txt` — 0.0.125 repeated failure/correlation gap.
- `docs/history/reports/STEP-35.0.3-PHYSICAL-SAME-RUN-CORRELATION-AND-INVOKE-FRONTIER.txt` — 0.0.126 same-run exact frontier.
- `docs/history/reports/STEP-35.0.4-PHYSICAL-GATE-A-CECIL-WRITE-RESOLUTION-FAILURE.txt` — 0.0.127 Gate-A writer failure.
- `docs/history/reports/STEP-35.0.5-PHYSICAL-GATE-A-DEFERRED-OPEN-FAILURE.txt` — 0.0.128 Immediate-read ordering failure.
- `docs/history/reports/STEP-35.0.7-PHYSICAL-SAVEMANAGER-LOCALIZATION-0.0.130.txt` — 0.0.130 working bridge and SaveManager-path physical frontier.

Raw `.ips` files are intentionally not stored in source because they can contain device-specific identifiers.
