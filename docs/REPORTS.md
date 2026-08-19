# Shareable Text Reports

Step 22.3 makes text files the default diagnostic handoff format so long device results do not require screenshots.

## iOS Files location

Files → On My iPhone → StS2 Launcher → StS2Launcher → Reports

Each current verification/test overwrites one deterministic **latest** report instead of creating unbounded timestamped logs. Writes use a temporary file followed by atomic replacement.

Current report names include:

- `Foundation-5of5.txt`
- `Step12-ManagedInstall.txt`
- `Step13-OfflineReady.txt`
- `Step14-CompatibilityInventory.txt`
- `Step15-GodotFoundation.txt`
- `Step16-ManagedPreparation.txt`
- `Step17-CompatibilityCallSites.txt`
- `Step18-RealAssemblyRewrite.txt`
- `Step19-ExpressionInterpreter.txt`
- `Step20-DynamicManagedExecution.txt`
- `Step21-RuntimeFrameworkBinding.txt`
- `Step22-HostBindingFrontier.txt`
- `TestSetup-Repair.txt`
- `TestSetup-Update.txt`
- `TestSetup-DownloadCacheClear.txt`
- `TestSetup-FreshDownload.txt`

The existing full binding-plan export remains at `Documents/StS2Launcher/Step21.1-RuntimeBindingDiagnostics.txt`, and Step 22 framework availability diagnostics retain their dedicated output files.

## Report schema and privacy

The shared writer records result/detail text plus app/runtime/architecture capability metadata. It does **not** read the username/password fields or session token storage. The report schema explicitly excludes Steam passwords, refresh tokens, Steam Guard material, and Apple signing secrets.

Reports are output-only. No launcher runtime path consumes these text files as trusted input.

## Codemagic/host reports

Codemagic publishes plain-text files under `artifacts/reports/`, including:

- `static-validation.txt`
- `host-unit-tests.txt`
- `ipa-verification.txt`
- `build-environment.txt`
- `ios-workload.txt`
- `ios-build.txt`
- `build-summary.txt`

TRX test results and lower-level logs are still retained as secondary artifacts.
