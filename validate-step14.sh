#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Step 14 must preserve the physically proven Step 13 local-offline boundary.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step13.sh

python3 - <<'PY'
from pathlib import Path
import os
import plistlib

parent_mode = os.environ.get('STS2_VALIDATE_AS_PARENT') == '1'

inspection_path = Path('src/StS2Launcher.Core/SteamCompatibilityInventoryInspection.cs')
result_path = Path('src/StS2Launcher.Core/SteamCompatibilityInventoryResult.cs')
progress_path = Path('src/StS2Launcher.Core/SteamCompatibilityInventoryProgress.cs')
root_path = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs')
tests_path = Path('tests/StS2Launcher.Core.Tests/SteamCompatibilityInventoryTests.cs')

for path in (inspection_path, result_path, progress_path, root_path, tests_path):
    if not path.exists():
        raise SystemExit(f'ERROR: Step 14 source file missing: {path}')

inspection = inspection_path.read_text()
result = result_path.read_text()
progress = progress_path.read_text()
root = root_path.read_text()
tests = tests_path.read_text()

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if not parent_mode:
    if plist.get('CFBundleShortVersionString') != '0.0.41' or str(plist.get('CFBundleVersion')) != '41':
        raise SystemExit('ERROR: standalone Step 14 must be version 0.0.41 (41).')
else:
    build = int(str(plist.get('CFBundleVersion') or '0'))
    if build < 41:
        raise SystemExit('ERROR: later-step Step 14 regression validation requires build version >= 41.')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
project_markers = [
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
]
if not parent_mode:
    project_markers.extend([
        '<ApplicationVersion>41</ApplicationVersion>',
        '<ApplicationDisplayVersion>0.0.41</ApplicationDisplayVersion>',
    ])
for marker in project_markers:
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 14 project/regression marker missing: {marker}')

for marker in (
    'public sealed class SteamCompatibilityInventoryInspection',
    'private readonly SteamOfflineInstallInspection _offlineInspection;',
    'var offline = await _offlineInspection.RunAsync(',
    'if (!offline.Success)',
    'SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt',
    'SteamCompatibilityInventoryOutcome.LocalInstallNotReady',
    'SteamCompatibilityInventoryOutcome.Complete',
    'AssetExtensions',
    'GodotContentExtensions',
    'GodotMarkers',
    'FmodMarkers',
    'SpineMarkers',
    'ReflectionMarkers',
    'DynamicCodeMarkers',
    'PlatformMarkers',
    '"System.Reflection.Emit"',
    '"DynamicMethod"',
    '"AssemblyBuilder"',
    '"Expression.Compile"',
    '"GodotSharp"',
    '"FMOD"',
    '"Spine"',
    'DetectNativeBinaryKindAsync',
    'ScanInterestingBinaryMarkersAsync',
    'Encoding.Latin1.GetString',
    'HasManagedMetadataSignature',
    'SteamSessionConsulted: false',
    'NetworkAccessAttempted: false',
    'ManagedInstallModified: false',
    'GameLaunchAttempted: false',
):
    if marker not in inspection:
        raise SystemExit(f'ERROR: Step 14 inventory marker missing: {marker}')

# The Step 14 inspector is read-only. It may open existing files, but it may not
# create/delete/move/copy/write managed-install content or invoke Steam/game code.
for forbidden in (
    'SteamSessionStore',
    'SteamClient',
    'SteamKit2',
    'HttpClient',
    'ClientWebSocket',
    'SocketsHttpHandler',
    'SteamContentDiscoveryAttempt',
    'SteamResumableDepotDownloadAttempt',
    'SteamOwnershipVerificationAttempt',
    'File.Write',
    'File.Append',
    'File.Delete',
    'File.Copy',
    'File.Move',
    'Directory.CreateDirectory',
    'Directory.Delete',
    'Directory.Move',
    'FileMode.Create',
    'FileMode.CreateNew',
    'FileMode.Append',
    'Assembly.Load(',
    'AssemblyLoadContext',
    'Activator.CreateInstance(',
    'Mono.Cecil',
    'LaunchGame',
    'StartGodot',
    'SteamWorkshop',
    'SteamCloud',
):
    if forbidden in inspection:
        raise SystemExit(f'ERROR: Step 14 read-only inspector gained forbidden dependency/mutation/execution: {forbidden}')

for marker in (
    'OfflineReadyPreconditionVerified',
    'GodotContentFiles',
    'ManagedAssemblyFiles',
    'NativeBinaryFiles',
    'GodotSharpIndicatorFiles',
    'FmodIndicatorFiles',
    'SpineIndicatorFiles',
    'ReflectionIndicatorFiles',
    'DynamicCodeIndicatorFiles',
    'PlatformSpecificFiles',
    'PotentialIosBlockerSignals',
    'DependencyNotes',
    'ManagedInstallModified',
    'GameLaunchAttempted',
    'COMPATIBILITY INVENTORY PASS',
):
    if marker not in result:
        raise SystemExit(f'ERROR: Step 14 result-contract marker missing: {marker}')

for marker in (
    'VerifyingOfflineInstall',
    'ReadingReceipt',
    'ClassifyingFiles',
    'ScanningManagedAssemblies',
    'Complete',
):
    if marker not in progress:
        raise SystemExit(f'ERROR: Step 14 progress marker missing: {marker}')

ui_markers = [
    'Step 14 — read-only compatibility inventory',
    'Inventory Installed Game Compatibility (Read Only)',
    'RunCompatibilityInventoryAsync',
    'COMPATIBILITY INVENTORY: NOT RUN',
    'FormatCompatibilityInventoryDetail',
    'Potential iOS blocker signals:',
    'Network access attempted by Step 14:',
    'Managed install modified by Step 14:',
    'Game launch attempted:',
    'Step 14 evidence policy: metadata/path indicators are triage signals',
    'Verify Offline-Ready Install (Local Only)',
    'Run Foundation 5/5 Regression',
]
if not parent_mode:
    ui_markers.extend([
        'STEP 14 — COMPATIBILITY INVENTORY',
        'Version 0.0.41',
        'Steps 01–13 are complete on the physical iPhone.',
        'Mono.Cecil rewrite / Godot host / game execution: NOT IMPLEMENTED',
    ])
for marker in ui_markers:
    if marker not in root:
        raise SystemExit(f'ERROR: Step 14 UI/regression marker missing: {marker}')

for marker in (
    'CompatibilityInventoryClassifiesInstalledContentReadOnly',
    'CompatibilityInventoryRefusesCorruptInstallBeforeClassification',
    'CompatibilityInventoryRequiresExistingManagedInstall',
    'CompatibilityInventoryResultContractExplicitlyProvesReadOnlyBoundary',
    'Step 14 modified managed file:',
    'Assert.IsFalse(result.SteamSessionConsulted);',
    'Assert.IsFalse(result.NetworkAccessAttempted);',
    'Assert.IsFalse(result.ManagedInstallModified);',
    'Assert.IsFalse(result.GameLaunchAttempted);',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 14 host-test marker missing: {marker}')

# No Step 15+ implementation should enter this boundary.
for path in (inspection_path, result_path, progress_path):
    text = path.read_text()
    for forbidden in (
        'GodotHost',
        'godot_ios',
        'GDExtension',
        'Mono.Cecil',
        'RewriteAssembly',
        'PatchAssembly',
        'RunGame',
        'LaunchGame',
    ):
        if forbidden in text:
            raise SystemExit(f'ERROR: Step 14 broadened into a later boundary: {path}: {forbidden}')

if not parent_mode:
    codemagic = Path('codemagic.yaml').read_text()
    for marker in (
        'ios-step-14:',
        'Step 14 - compatibility inventory',
        'artifacts/StS2-Launcher-Step-14.ipa',
        'artifacts/step14-build-summary.txt',
    ):
        if marker not in codemagic:
            raise SystemExit(f'ERROR: Step 14 Codemagic marker missing: {marker}')

for path in (
    Path('scripts/build-step14.sh'),
    Path('scripts/verify-step14-ipa.sh'),
    Path('docs/STEP-14-TEST.md'),
):
    if not path.exists():
        raise SystemExit(f'ERROR: Step 14 build/test artifact missing: {path}')

print('Step 14 compatibility-inventory regression validation: PASS' if parent_mode else 'Step 14 compatibility-inventory source validation: PASS')
print('  Steps 01-13 regression guards retained')
print('  Step 13 OfflineReady is re-proven before compatibility classification')
print('  Installed receipt/file tree is inspected read-only; no Steam/network dependency')
print('  Managed assemblies are not loaded/executed; only metadata strings are scanned for triage indicators')
print('  Assets, Godot content, managed assemblies, native binaries, GodotSharp, FMOD, Spine, reflection/dynamic-code and platform-specific signals are inventoried')
print('  No Mono.Cecil rewrite, Godot host, game launch, Cloud or Workshop capability added')
PY
