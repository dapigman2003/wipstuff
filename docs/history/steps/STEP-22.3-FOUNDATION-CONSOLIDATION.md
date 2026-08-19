# Step 22.3 — Foundation Consolidation

## Purpose

Freeze the physically proven Step 22 runtime-binding behavior and improve maintainability/diagnostics before Step 23 crosses the first real `sts2.dll` CLR-load boundary.

This step is intentionally **behavior-neutral with respect to StS2 compatibility**.

## Changes

### Protected runtime behavior

All 97 Core `.cs` files present in the physically passed Step 22.2 candidate are retained byte-for-byte and checked by `tools/validation/protected-step22.2-core.sha256`. New Core behavior is limited to the additive `DeviceTestReportWriter` diagnostic utility.

### Source structure

Core files and tests are organized into Foundation, Steam, Compatibility, Godot, Runtime, Diagnostics/TestSupport folders without namespace/type changes. `RootViewController` is split into focused sealed-partial files.

### Test/report consolidation

Every current on-device verification/test boundary writes a shareable deterministic `.txt` report. Host unit tests, static validation, and IPA verification also emit plain-text reports.

Seven active scripts replace the large mixed collection of current and obsolete step wrappers. Historical scripts/docs are retained under `history/`.

### Safe optimization

The launcher Documents root is computed once and reused. Host-built Step 20 fixtures are reused by the iOS packaging path when already produced by the test stage, avoiding unnecessary duplicate fixture builds. Shared test temporary-directory infrastructure removes repeated helper implementations.

No performance-sensitive installer/binding algorithm was changed merely for cleanup; physically proven Core behavior remains frozen.

## Device acceptance

Step 22.3 should not introduce a new compatibility gate. Acceptance is regression-oriented:

1. App starts and displays `STEP 22.3 — FOUNDATION CONSOLIDATION`, version `0.0.61`.
2. Run `Step 22` A–D regression: 4/4, zero blockers, Runtime closure ready=YES.
3. Run OfflineReady: PASS.
4. Run Foundation 5/5: PASS.
5. Verify the corresponding text reports exist in Files under `StS2Launcher/Reports` and can be shared.

If any regression fails, Step 23 remains blocked.
