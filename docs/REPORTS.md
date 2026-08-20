# Reports and Diagnostics

Long diagnostic output should be written to files instead of forcing screenshots.

## Device reports

Current regression/test reports are written atomically under:

`Documents/StS2Launcher/Reports/*.txt`

and are visible through:

`Files → On My iPhone → StS2 Launcher → StS2Launcher → Reports`

Specialized full diagnostics may use stable files directly under `Documents/StS2Launcher/`, for example the runtime-binding or framework-frontier reports.

The active Step 24 report is:

`Documents/StS2Launcher/Reports/Step24-ControlledManagedInitialization.txt`

It records the ordered A–D result, accepted Step 23 preflight replay, exact initializer target/IL and bounded automatic-initialization-closure audit, exact replayed private context, target load/module-constructor completion barrier, managed/native resolver observations, post-initialization context membership, plan/file hashes, OfflineReady audit, and the explicit no-Harmony-patch/no-game-invocation/no-Godot/no-native-game boundary.

The physically closed Step 23 regression report remains available as:

`Documents/StS2Launcher/Reports/Step23-FirstRealGameLoad.txt`

A current verification overwrites its deterministic latest report. Reports are output-only and are never treated as trusted runtime input.

## Build/host reports

Shareable summaries:

- `artifacts/reports/static-validation.txt`
- `artifacts/reports/host-unit-tests.txt`
- `artifacts/reports/godot-native-preflight.txt`
- `artifacts/reports/ios-build.txt`
- `artifacts/reports/ipa-verification.txt`
- `artifacts/reports/build-summary.txt`

Detailed material remains under `artifacts/logs/` and `artifacts/test-results/`.

## Secret exclusions

Reports must not contain Steam passwords, reusable refresh tokens, Steam Guard secrets/codes, Apple signing secrets/private keys, or other credential UI values. Absolute host paths should be omitted where they do not materially help diagnosis.
