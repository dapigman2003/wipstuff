# Reports and Diagnostics

Long diagnostic output should be written to files instead of forcing screenshots.

## Device reports

Current regression/test reports are written atomically under:

`Documents/StS2Launcher/Reports/*.txt`

and are visible through:

`Files → On My iPhone → StS2 Launcher → StS2Launcher → Reports`

Specialized full diagnostics may use stable files directly under `Documents/StS2Launcher/`, for example the runtime-binding or framework-frontier reports.

The current Step 23 first-load report is:

`Documents/StS2Launcher/Reports/Step23-FirstRealGameLoad.txt`

It contains the ordered A–D result, preflight module-initializer/PInvoke counts, real primary load identity/context, planned host/private binding counts, native-resolution count, byte/OfflineReady audit, and the explicit no-entry-point/no-member-invocation boundary.

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
