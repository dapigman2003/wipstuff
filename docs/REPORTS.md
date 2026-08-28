# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 35 reports

`Documents/StS2Launcher/Reports/Step35-CrashCheckpoint.txt`

Step 35.0.1 / 0.0.124 adds this synchronously flushed crash-localization file because physical 0.0.123 hard-terminated without executing managed `finally`. Each line records UTC time, process ID, managed thread ID, and a B/C/resolver frontier marker. It is diagnostic-only and must never be used as trusted runtime input. Preserve it before any retry after an abrupt termination.

`Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

This remains the deterministic normal managed report. It is produced on PASS, managed FAIL, cancellation, or other paths that reach the UI `finally`. It may be absent after native/runtime/kernel hard termination.

## Physical Step-35.0 observation

Physical `0.0.123 (123)` opened, reached the visible Gate-B region, and terminated abruptly. No normal Step-35 app report survived. The supplied matching `.ips` identifies the build and records `EXC_BAD_ACCESS / SIGKILL`, faulting main thread and PC=`0x0`, with `CODESIGNING / Invalid Page` termination text. The raw `.ips` is intentionally not stored in the source archive because device crash logs can contain device-specific identifiers; the sanitized evidence summary is preserved in `docs/history/reports/STEP-35.0-PHYSICAL-HARD-TERMINATION-SUMMARY.txt`.

## Latest physical closures

- `docs/history/reports/STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt` — physical `0.0.122` Step 34 4/4: exact transformed `PrewarmJit()` invoked once and returned normally; 8 managed requests = 6 exact host + 2 initializer-free private loads; zero initializer-bearing/unplanned/native escape.
- `docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt` — physical `0.0.121` Step 33 4/4 transformed-primary-only CLR admission.
- `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt` — physical `0.0.120` Step 32 4/4 exact private real-StS2 semantic rewrite.

The active Step-35.0.1 candidate keeps stable `ios-canonical` and the NuGet/Godot/iOS-arm64 `obj` cache paths without changing runtime policy.
