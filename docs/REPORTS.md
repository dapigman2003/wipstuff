# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 35 reports

`Documents/StS2Launcher/Reports/Step35-CrashCheckpoint.txt`

Synchronous crash-localization telemetry. Each line records UTC time, process ID, managed thread ID, and a B/C/resolver frontier marker. Physical 0.0.124 used this file to prove Gate B PASS and localize the hard kill after `C_INVOKE_START` but before `C_INVOKE_RETURNED`.

`Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap.txt`

New in Step 35.0.2 / 0.0.125. Generated after the exact source/transformed semantic checks and before CLR admission. It contains the exact transformed `ExecuteVeryEarly` wrapper and async `MoveNext` IL/callsite map, metadata scopes, numbered callsites, and await-registration candidates. It is output-only, never trusted runtime input, and is not embedded in the source archive because it is derived from the user's installed game image.

`Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

Normal deterministic managed report. It is produced only when managed control reaches the UI `finally`; it may be absent after a native/runtime/kernel hard termination.

## Preserved Step-35 physical evidence

- `docs/history/reports/STEP-35.0-PHYSICAL-HARD-TERMINATION-SUMMARY.txt` — sanitized 0.0.123 main-thread PC=0x0 hard termination.
- `docs/history/reports/STEP-35.0.1-PHYSICAL-EXECUTEVERYEARLY-INVOKE-CRASH-LOCALIZATION.txt` — sanitized 0.0.124 evidence proving Gate B PASS and hard termination inside synchronous ExecuteVeryEarly invocation after planned resolver activity.

Raw `.ips` files are intentionally not stored in source because they can contain device-specific identifiers.

## Latest physical closures

- `docs/history/reports/STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.122 Step 34 4/4 exact transformed `PrewarmJit()` execution.
- `docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.121 Step 33 4/4 transformed-primary-only CLR admission.
- `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.120 Step 32 4/4 exact private real-StS2 semantic rewrite.
