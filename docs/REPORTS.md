# Reports and Diagnostics

Long diagnostic output should be written to files instead of forcing screenshots.

## Device reports

Current regression/test reports are written atomically under:

`Documents/StS2Launcher/Reports/*.txt`

and are visible through:

`Files → On My iPhone → StS2 Launcher → StS2Launcher → Reports`

Specialized full diagnostics may use stable files directly under `Documents/StS2Launcher/`, for example the runtime-binding or framework-frontier reports.

The active Step 26 report is:

`Documents/StS2Launcher/Reports/Step26-ControlledHarmonyProcessorCreation.txt`

It records the ordered A–I result; Step 23/24 metadata and runtime replay; exact Harmony type-initializer and constructor metadata preflight; exact targeted runtime type/type-initializer/constructor/member resolution; the explicit Harmony type-initialization barrier and its audit; the one controlled Harmony object construction; managed/native resolver observations; post-construction context membership; plan/file hashes; OfflineReady audit; and the explicit no-patching/no-game-reflection/no-Godot/no-native-game boundary.

The physically closed Step 24 report remains available as:

`Documents/StS2Launcher/Reports/Step24-ControlledManagedInitialization.txt`

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
