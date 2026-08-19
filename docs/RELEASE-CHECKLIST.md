# Release Checklist — Step 22.3 Baseline

Use this checklist before any candidate becomes the starting point for Step 23 or later.

## Source

- `bash scripts/validate.sh` passes.
- Protected Step 22.2 Core and platform/native manifests pass.
- No `artifacts`, `bin`, `obj`, `.nuget`, `NativeBuild`, game payloads, credentials, or signing secrets are present in the source ZIP.
- ZIP extracts with one top-level project directory and passes integrity testing.

## Codemagic host/build

- `scripts/test.sh` passes and emits `artifacts/reports/host-unit-tests.txt` plus TRX.
- iOS workload/version pins are correct.
- SteamKit build-only iOS patch telemetry passes.
- Godot native link preflight passes and emits its text report.
- iOS publish emits the `STEP22.3 RUNTIME POLICY` line with `MtouchInterpreter=-all`, no broad `UseInterpreter=true`, no NativeAOT.
- DiskArbitration is removed from generated iOS linker frameworks.
- `scripts/verify-ipa.sh` passes and emits `artifacts/reports/ipa-verification.txt`.
- IPA contains no StS2/proprietary payload and only the expected project-owned Step 16/20 fixture data.

## Physical iPhone

- Header/version: Step 22.3 / 0.0.61.
- Step 22 A–D regression: 4/4.
- Explicit binding blockers: 0.
- Runtime closure ready: YES.
- OfflineReady: PASS.
- Foundation 5/5: PASS.
- Files reports exist for Step 22, OfflineReady, and Foundation and can be shared.

Only after all items above pass should the project branch into Step 23.
