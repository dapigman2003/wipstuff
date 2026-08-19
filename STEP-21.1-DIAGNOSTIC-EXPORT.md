# Step 21.1 — Binding Diagnostic Export

## Purpose

Step 21 physically passed all four Prepared Runtime / Framework Binding gates, but the audited plan reported 47 explicit binding blockers and `Runtime closure ready for first real CLR load: NO`.

Step 21.1 is deliberately **reporting-only**. It does not change the Step 21 binding algorithm, dependency graph, prepared assembly set, resolver policy, or runtime-readiness decision. The physically passed Step 21 core implementation and its host tests are exact-hash protected by `scripts/validate-step21-1.sh`.

## Why this hotfix exists

The Step 21 UI only shows a small blocker sample. Screenshotting the complete blocker frontier is impractical. Step 21 already persists the authoritative plan as:

`Documents/StS2Launcher/Step21-PreparedRuntimeBinding/plan/runtime-binding-plan.json`

Step 21.1 adds a deterministic exporter that reads that persisted JSON and writes:

`Documents/StS2Launcher/Step21.1-RuntimeBindingDiagnostics.txt`

The text report contains:

- summary and persisted plan SHA-256;
- blocker count and runtime-closure result;
- blocker counts grouped by kind;
- unique blocked requested identities, occurrence counts, kinds, and source assemblies;
- the complete ordered blocker list with kind/source/request/detail;
- dependency-edge counts by binding kind;
- host framework identity mappings (without absolute host paths);
- prepared IL-only assembly identities, relative paths, receipt SHA-1s, and lengths.

The report schema does **not** include Steam credentials, refresh tokens, Steam Guard material, Apple signing secrets, or host absolute assembly locations.

## Files app access

The iOS `Info.plist` enables:

- `UIFileSharingEnabled = true`
- `LSSupportsOpeningDocumentsInPlace = true`

This allows the app's `Documents` directory to appear in Files. The intended path is:

`Files → On My iPhone → StS2 Launcher → StS2Launcher → Step21.1-RuntimeBindingDiagnostics.txt`

Because this exposes the app Documents tree, do not edit or delete other launcher data in Files. The trusted install remains SHA-1 audited by the launcher, but Step 21.1 does not need user edits anywhere in the Documents tree.

## Existing-plan fast path

Installing Step 21.1 over the same bundle ID should preserve the app Documents container. If the Step 21 plan is still present, tap **Export Complete Step 21 Binding Diagnostics to Files** immediately. A Step 21 A–D rerun is not required merely to produce the report.

If the plan is missing, the exporter refuses to create a misleading report and asks for one Step 21 A–D rerun.

## Trust boundary

The exported `.txt` file is output only. No launcher code reads it back as trusted input. The exporter reads only the persisted Step 21 plan and writes only its own text report.

Real StS2 CLR loading remains forbidden while `Runtime closure ready` is `NO`.
