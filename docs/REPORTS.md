# Diagnostic Reports

Current on-device diagnostics write output-only text beneath `Documents/StS2Launcher/Reports/*.txt`. Reports are never trusted runtime input and intentionally exclude Steam passwords/tokens/Guard material and Apple signing secrets.

## Active Step 35 report

`Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

0.0.123 advances one measured managed-initialization boundary beyond the physically closed exact transformed `PrewarmJit()` execution. Gate A re-runs/reverifies the closed transform, exact `ExecuteVeryEarly` wrapper + async state-machine semantics, and prepared resolver plan. Gate B re-establishes transformed-primary-only admission in `StS2Launcher-Step35-VeryEarly`. Gate C invokes only exact transformed `ExecuteVeryEarly()` once and awaits its returned Task for at most 60 seconds under the strict resolver. Gate D re-proves source/transformed/plan/dependency/context isolation. Preserve the first failure exactly, especially exception chain plus resolver/native state.

## Latest physical closures

- `docs/history/reports/STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt` — physical `0.0.122` Step 34 4/4: exact transformed `PrewarmJit()` invoked once and returned normally; 8 managed requests = 6 exact host + 2 initializer-free private loads; zero initializer-bearing/unplanned/native escape.
- `docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt` — physical `0.0.121` Step 33 4/4 transformed-primary-only CLR admission.
- `docs/history/reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt` — physical `0.0.120` Step 32 4/4 exact private real-StS2 semantic rewrite.

The earlier Step-34 Codemagic UI compile failure remains preserved separately; it produced no IPA/device evidence. The active Step-35 candidate keeps stable `ios-canonical` and the NuGet/Godot/iOS-arm64 `obj` cache paths without changing runtime policy.
