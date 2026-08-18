#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Step 17 must preserve the physically proven Step 16 managed-preparation subsystem.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step16.sh

python3 - <<'PY'
from pathlib import Path
import os
import plistlib

parent_mode = os.environ.get('STS2_VALIDATE_AS_PARENT') == '1'

required = [
    Path('src/StS2Launcher.Core/CompatibilityCallSiteGate.cs'),
    Path('src/StS2Launcher.Core/CompatibilityCallSiteGateResult.cs'),
    Path('src/StS2Launcher.Core/CompatibilityCallSiteGateSequence.cs'),
    Path('src/StS2Launcher.Core/CompatibilityCallSiteSummary.cs'),
    Path('src/StS2Launcher.Core/CompatibilityCallSiteProgress.cs'),
    Path('src/StS2Launcher.Core/CompatibilityCallSiteAnalysis.cs'),
    Path('tests/StS2Launcher.Core.Tests/CompatibilityCallSiteAnalysisTests.cs'),
    Path('scripts/build-step17.sh'),
    Path('scripts/run-unit-tests-step17.sh'),
    Path('scripts/codemagic-build-step17.sh'),
    Path('scripts/verify-step17-ipa.sh'),
    Path('docs/STEP-17-DESIGN.md'),
    Path('docs/STEP-17-TEST.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 17 artifact missing: {path}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if not parent_mode:
    if plist.get('CFBundleShortVersionString') != '0.0.46' or str(plist.get('CFBundleVersion')) != '46':
        raise SystemExit('ERROR: standalone Step 17 must be version 0.0.46 (46).')
else:
    if int(str(plist.get('CFBundleVersion') or '0')) < 46:
        raise SystemExit('ERROR: later-step Step 17 regression validation requires build version >= 46.')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
project_markers = [
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
    '<ForceLoad>false</ForceLoad>',
    '<SmartLink>false</SmartLink>',
]
if not parent_mode:
    project_markers.extend([
        '<ApplicationVersion>46</ApplicationVersion>',
        '<ApplicationDisplayVersion>0.0.46</ApplicationDisplayVersion>',
    ])
for marker in project_markers:
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 17 iOS/regression marker missing: {marker}')
if '<TrimmerRootAssembly Include="Mono.Cecil" />' in csproj:
    raise SystemExit('ERROR: Step 17 must preserve the physically proven normal full-trim Mono.Cecil path; do not blanket-root it.')

analysis = Path('src/StS2Launcher.Core/CompatibilityCallSiteAnalysis.cs').read_text()
for marker in (
    'public sealed class CompatibilityCallSiteAnalysis',
    'RunArm64ManagedScopeAsync',
    'RunActualIlCallSiteScanAsync',
    'RunNativePlatformInteropClassification',
    'RunPrimaryDependencyPressureMapAsync',
    'SteamOfflineInstallInspection',
    'SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt',
    'data_sts2_macos_arm64',
    'data_sts2_macos_x86_64',
    'ModuleDefinition.ReadModule',
    'ReadingMode = ReadingMode.Deferred',
    'method.Body.Instructions',
    'instruction.Operand is not MethodReference target',
    'System.Reflection.Emit',
    'ExpressionCompile',
    'HarmonyRuntimePatch',
    'PInvokeDefinitions',
    'PInvokeCallSites',
    'System.Diagnostics.Process',
    'Microsoft.Win32.Registry',
    'Godot/GodotSharp',
    'Steamworks',
    'FMOD',
    'Spine',
    'ComputeSha1HexAsync(path, cancellationToken)',
    'All Step 17 scan candidates receipt SHA-1 preserved: YES',
    'Assembly dependency resolution attempted: NO',
    'Game assembly loaded/executed: NO',
):
    if marker not in analysis:
        raise SystemExit(f'ERROR: Step 17 call-site-analysis marker missing: {marker}')

# Step 17 is a strictly read-only analysis class. Do not resolve/load/execute
# dependencies or gain any network/Steam/download dependency.
for forbidden in (
    '.Resolve(',
    'Assembly.Load(',
    'Activator.CreateInstance(',
    'MethodInfo.Invoke',
    '.Write(',
    'File.Write',
    'File.Delete',
    'File.Copy',
    'File.Move',
    'Directory.CreateDirectory',
    'Directory.Delete',
    'Directory.Move',
    'FileMode.Create',
    'FileMode.CreateNew',
    'FileMode.Append',
    'SteamClient',
    'HttpClient',
    'ClientWebSocket',
    'SteamSessionStore',
    'SteamContentDiscoveryAttempt',
    'SteamResumableDepotDownloadAttempt',
):
    if forbidden in analysis:
        raise SystemExit(f'ERROR: Step 17 read-only call-site boundary gained forbidden behavior: {forbidden}')

seq = Path('src/StS2Launcher.Core/CompatibilityCallSiteGateSequence.cs').read_text()
summary = Path('src/StS2Launcher.Core/CompatibilityCallSiteSummary.cs').read_text()
for marker in (
    'var expected = (CompatibilityCallSiteGate)(_results.Count + 1);',
    'Cannot advance after the first failed compatibility call-site gate.',
    '_results.Count == 4',
    'COMPATIBILITY CALL-SITE ANALYSIS PASS — {PassedGates}/4',
):
    if marker not in seq + summary:
        raise SystemExit(f'ERROR: Step 17 ordered-gate contract missing: {marker}')

testproj = Path('tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj').read_text()
if '<PackageReference Include="Mono.Cecil" Version="0.11.6" />' not in testproj:
    raise SystemExit('ERROR: Step 17 tests directly use Cecil and must pin Mono.Cecil 0.11.6 explicitly.')

tests = Path('tests/StS2Launcher.Core.Tests/CompatibilityCallSiteAnalysisTests.cs').read_text()
for marker in (
    'OrderedCompatibilityCallSiteGatesReachFourOfFourPass',
    'CompatibilityCallSiteGatesStopAfterFirstFailure',
    'Arm64AnalysisUsesActualIlCallsAndExcludesX8664Duplicate',
    'ExpressionCompile=1',
    'P/Invoke definitions: 1',
    'Godot/GodotSharp=1',
    'Steamworks=1',
    'All Step 17 scan candidates receipt SHA-1 preserved: YES',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 17 host-test marker missing: {marker}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
root_markers = [
    'Step 17 — Compatibility Call-Site Analysis (ordered gates A–D)',
    'Run Gates A–D — ARM64 Scope → Actual IL Calls → Native/Platform → Dependency Map',
    'RunCompatibilityCallSiteAnalysisAsync',
    '_compatibilityCallSiteAnalysis.RunArm64ManagedScopeAsync',
    '_compatibilityCallSiteAnalysis.RunActualIlCallSiteScanAsync',
    '_compatibilityCallSiteAnalysis.RunNativePlatformInteropClassification',
    '_compatibilityCallSiteAnalysis.RunPrimaryDependencyPressureMapAsync',
    'PASS: STEP 17 COMPATIBILITY CALL-SITE ANALYSIS — 4/4',
    'Run Gates A–D — Cecil Fixture → IL Rewrite → Real StS2 Metadata',
    'Run Foundation 5/5 Regression',
]
if not parent_mode:
    root_markers.extend([
        'STEP 17 — COMPATIBILITY CALL-SITE ANALYSIS',
        'Version 0.0.46',
        'Steps 01–16 are complete on the physical iPhone.',
    ])
for marker in root_markers:
    if marker not in root:
        raise SystemExit(f'ERROR: Step 17 UI/gate marker missing: {marker}')

build = Path('scripts/build-step17.sh').read_text()
for marker in (
    'bash scripts/validate-step17.sh',
    'fixtures/StS2Launcher.Step16.Fixture/StS2Launcher.Step16.Fixture.csproj',
    'bash scripts/build-godot-step15.sh',
    'dotnet publish "$PROJECT"',
    'Step15GodotSmokeProject',
    'Step16Fixtures',
    'StS2-Launcher-Step-17.ipa',
):
    if marker not in build:
        raise SystemExit(f'ERROR: Step 17 build-wrapper marker missing: {marker}')

verify = Path('scripts/verify-step17-ipa.sh').read_text()
verify_markers = [
    'Step16Fixtures/StS2Launcher.Step16.Fixture.dll',
    'cmp -s "$FIXTURE_SOURCE" "$FIXTURE"',
    'Real StS2/proprietary payload in IPA: none',
    'DiskArbitration',
    'AudioUnit.framework',
]
if not parent_mode:
    verify_markers.extend([
        '0.0.46',
        'BUILD_VERSION" == "46"',
        'Expected device UI: STEP 17 — COMPATIBILITY CALL-SITE ANALYSIS',
    ])
for marker in verify_markers:
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 17 IPA verification marker missing: {marker}')

if not parent_mode:
    codemagic = Path('codemagic.yaml').read_text()
    for marker in (
        'ios-step-17:',
        'Step 17 - Compatibility Call-Site Analysis',
        'max_build_duration: 120',
        '$HOME/.cache/sts2launcher/godot-step15',
        'bash scripts/codemagic-build-step17.sh',
        'artifacts/StS2-Launcher-Step-17.ipa',
        'artifacts/step17-build-summary.txt',
    ):
        if marker not in codemagic:
            raise SystemExit(f'ERROR: Step 17 Codemagic marker missing: {marker}')

print('Step 17 Compatibility Call-Site Analysis source validation: PASS')
print('  Steps 01-16 regression guards retained')
print('  Gate A: OfflineReady + iOS-relevant macOS arm64/shared managed scope')
print('  Gate B: concrete IL method-reference/dynamic-risk call sites')
print('  Gate C: P/Invoke/native module + platform-sensitive API classification')
print('  Gate D: primary arm64 sts2.dll dependency pressure map + post-scan SHA-1 proof')
print('  No dependency resolution, game rewrite/load/execute, FMOD/Spine runtime integration, Cloud or Workshop added')
PY
