#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Preserve the physically proven Step 15 Godot subsystem as a parent regression.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step15.sh

python3 - <<'PY'
from pathlib import Path
import os
import plistlib
import re

parent_mode = os.environ.get('STS2_VALIDATE_AS_PARENT') == '1'

required = [
    Path('src/StS2Launcher.Core/ManagedPreparationGate.cs'),
    Path('src/StS2Launcher.Core/ManagedPreparationGateResult.cs'),
    Path('src/StS2Launcher.Core/ManagedPreparationGateSequence.cs'),
    Path('src/StS2Launcher.Core/ManagedPreparationSummary.cs'),
    Path('src/StS2Launcher.Core/ManagedPreparationProgress.cs'),
    Path('src/StS2Launcher.Core/ManagedPreparationFoundation.cs'),
    Path('fixtures/StS2Launcher.Step16.Fixture/StS2Launcher.Step16.Fixture.csproj'),
    Path('fixtures/StS2Launcher.Step16.Fixture/FixtureTarget.cs'),
    Path('tests/StS2Launcher.Core.Tests/ManagedPreparationFoundationTests.cs'),
    Path('scripts/build-step16.sh'),
    Path('scripts/run-unit-tests-step16.sh'),
    Path('scripts/codemagic-build-step16.sh'),
    Path('scripts/verify-step16-ipa.sh'),
    Path('docs/STEP-16-DESIGN.md'),
    Path('docs/STEP-16-TEST.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 16 artifact missing: {path}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if not parent_mode:
    if plist.get('CFBundleShortVersionString') != '0.0.45' or str(plist.get('CFBundleVersion')) != '45':
        raise SystemExit('ERROR: standalone Step 16.1 must be version 0.0.45 (45).')
else:
    if int(str(plist.get('CFBundleVersion') or '0')) < 45:
        raise SystemExit('ERROR: later-step Step 16 regression validation requires build version >= 45.')

iosproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
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
        '<ApplicationVersion>45</ApplicationVersion>',
        '<ApplicationDisplayVersion>0.0.45</ApplicationDisplayVersion>',
    ])
for marker in project_markers:
    if marker not in iosproj:
        raise SystemExit(f'ERROR: Step 16 iOS regression marker missing: {marker}')
if '<TrimmerRootAssembly Include="Mono.Cecil" />' in iosproj:
    raise SystemExit('ERROR: Step 16 should first prove the statically referenced Cecil surface under normal full-trim analysis; do not blanket-root Mono.Cecil without device evidence.')

coreproj = Path('src/StS2Launcher.Core/StS2Launcher.Core.csproj').read_text()
for marker in (
    '<PackageReference Include="SteamKit2" Version="3.4.0" />',
    '<PackageReference Include="Mono.Cecil" Version="0.11.6" />',
):
    if marker not in coreproj:
        raise SystemExit(f'ERROR: Step 16 Core dependency marker missing: {marker}')

fixture_proj = Path('fixtures/StS2Launcher.Step16.Fixture/StS2Launcher.Step16.Fixture.csproj').read_text()
fixture = Path('fixtures/StS2Launcher.Step16.Fixture/FixtureTarget.cs').read_text()
for marker in (
    '<TargetFramework>net9.0</TargetFramework>',
    '<AssemblyName>StS2Launcher.Step16.Fixture</AssemblyName>',
    'STEP16_CECIL_FIXTURE_V1',
    'public static int RewriteMe() => 7;',
):
    if marker not in fixture_proj + fixture:
        raise SystemExit(f'ERROR: Step 16 project-owned fixture marker missing: {marker}')
for forbidden in ('SteamKit2', 'SlayTheSpire2', 'sts2.dll', 'FMOD', 'Spine', 'GodotSharp', 'DllImport'):
    if forbidden in fixture:
        raise SystemExit(f'ERROR: Step 16 fixture broadened beyond a tiny project-owned IL target: {forbidden}')

foundation = Path('src/StS2Launcher.Core/ManagedPreparationFoundation.cs').read_text()
for marker in (
    'using Mono.Cecil;',
    'using Mono.Cecil.Cil;',
    'AssemblyDefinition.ReadAssembly',
    'ReadingMode.Immediate',
    'ReadingMode.Deferred',
    'assembly.Write(output, new WriterParameters { WriteSymbols = false });',
    'method.Body.Instructions.Clear();',
    'il.Append(il.Create(OpCodes.Ldc_I4, FixtureRewrittenValue));',
    'il.Append(il.Create(OpCodes.Ret));',
    'FixtureOriginalValue = 7',
    'FixtureRewrittenValue = 42',
    'Step16-ManagedPreparation',
    'SteamOfflineInstallInspection',
    'SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt',
    'SelectPrimaryStS2AssemblyRelativePath(sts2Candidates)',
    'data_sts2_macos_arm64/sts2.dll',
    'ModuleDefinition.ReadModule',
    'ValidateReceiptSnapshot(receipt, offline);',
    'offline.Outcome == SteamOfflineInstallOutcome.Cancelled',
    'OfflineReady precondition — {value.Message}',
    'module.GetTypeReferences()',
    'Assembly dependency resolution attempted: NO',
    'All .dll/.exe candidate receipt SHA-1s preserved after inspection: YES',
    'method.IsPInvokeImpl',
    'ComputeSha1HexAsync(path, cancellationToken)',
    'Game assembly loaded/executed: NO',
):
    if marker not in foundation:
        raise SystemExit(f'ERROR: Step 16 managed-preparation marker missing: {marker}')
for forbidden in (
    'Assembly.Load(',
    'AssemblyLoadContext',
    'Activator.CreateInstance(',
    'MethodInfo.Invoke',
    'SteamClient',
    'HttpClient',
    'ClientWebSocket',
    'SteamSessionStore',
    'SteamContentDiscoveryAttempt',
    'SteamResumableDepotDownloadAttempt',
):
    if forbidden in foundation:
        raise SystemExit(f'ERROR: Step 16 Cecil boundary gained forbidden load/network/Steam dependency: {forbidden}')

# Real-install Gate D must not contain a Cecil write or direct filesystem mutation.
start = foundation.index('public async Task<ManagedPreparationGateResult> RunRealStS2MetadataInspectionAsync')
end = foundation.index('    private static AssemblyDefinition ReadAssembly', start)
gate_d = foundation[start:end]
for forbidden in (
    '.Write(',
    'assembly.Modules',
    '.Resolve(',
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
):
    if forbidden in gate_d:
        raise SystemExit(f'ERROR: Step 16 real StS2 Gate D is not read-only: {forbidden}')

seq = Path('src/StS2Launcher.Core/ManagedPreparationGateSequence.cs').read_text()
summary = Path('src/StS2Launcher.Core/ManagedPreparationSummary.cs').read_text()
tests = Path('tests/StS2Launcher.Core.Tests/ManagedPreparationFoundationTests.cs').read_text()
for marker in (
    'var expected = (ManagedPreparationGate)(_results.Count + 1);',
    'Cannot advance after the first failed managed-preparation gate.',
    '_results.Count == 4',
    'MANAGED PREPARATION PASS — {PassedGates}/4',
):
    if marker not in seq + summary:
        raise SystemExit(f'ERROR: Step 16 ordered-gate contract missing: {marker}')
for marker in (
    'OrderedManagedPreparationGatesReachFourOfFourPass',
    'ManagedPreparationStopsAtFirstFailingGate',
    'ManagedPreparationRejectsOutOfOrderGate',
    'ProjectOwnedFixtureReadRoundTripAndRewritePass',
    'RealAssemblyInspectionUsesReceiptBackedInstallReadOnly',
    'RealAssemblyInspectionSelectsMacOsArm64Sts2WhenDepotContainsBothArchitectures',
    'Assert.ThrowsExactly<InvalidOperationException>',
    'RewriteMe 7 → 42',
    'Real managed install modified: NO',
    'Post-inspection candidate SHA-1s reverified: 1/1',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 16 host-test marker missing: {marker}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
root_markers = [
    'Step 16 — Managed Preparation Foundation (ordered gates A–D)',
    'Run Gates A–D — Cecil Fixture → IL Rewrite → Real StS2 Metadata',
    'RunManagedPreparationFoundationAsync',
    '_managedPreparationFoundation.RunFixtureRead',
    '_managedPreparationFoundation.RunFixtureRoundTrip',
    '_managedPreparationFoundation.RunControlledIlRewrite',
    '_managedPreparationFoundation.RunRealStS2MetadataInspectionAsync',
    'Step16Fixtures',
    'StS2Launcher.Step16.Fixture.dll',
    'PASS: STEP 16 MANAGED PREPARATION — 4/4',
    'Verify Offline-Ready Install (Local Only)',
    'Run Foundation 5/5 Regression',
]
if not parent_mode:
    root_markers.extend([
        'STEP 16.1 — MANAGED PREPARATION FOUNDATION',
        'Version 0.0.45',
        'Steps 01–15 are complete on the physical iPhone.',
    ])
for marker in root_markers:
    if marker not in root:
        raise SystemExit(f'ERROR: Step 16 UI/gate marker missing: {marker}')
if 'Assembly.Load(' in root:
    raise SystemExit('ERROR: Step 16 UI must not load the fixture/game assembly into the runtime.')

build = Path('scripts/build-step16.sh').read_text()
for marker in (
    'bash scripts/validate-step16.sh',
    'fixtures/StS2Launcher.Step16.Fixture/StS2Launcher.Step16.Fixture.csproj',
    'dotnet build "$FIXTURE_PROJECT" -c Release --no-restore',
    'bash scripts/build-godot-step15.sh',
    'dotnet publish "$PROJECT"',
    'Step15GodotSmokeProject',
    'Step16Fixtures',
    'StS2Launcher.Step16.Fixture.dll',
    'StS2-Launcher-Step-16.1.ipa',
):
    if marker not in build:
        raise SystemExit(f'ERROR: Step 16 build-wrapper marker missing: {marker}')
# Fixture must be copied only after app publish, so it is inert raw data rather than an AOT app input.
if build.index('dotnet publish "$PROJECT"') > build.index('cp "$FIXTURE_DLL" "$FIXTURE_DIR/StS2Launcher.Step16.Fixture.dll"'):
    raise SystemExit('ERROR: Step 16 fixture must be copied into the finished app after publish, not fed into iOS AOT.')

verify = Path('scripts/verify-step16-ipa.sh').read_text()
verify_markers = [
    'Step16Fixtures/StS2Launcher.Step16.Fixture.dll',
    'cmp -s "$FIXTURE_SOURCE" "$FIXTURE"',
    'bundled Step 16 fixture differs from the exact project-owned fixture built earlier in this Codemagic run',
    'Real StS2/proprietary payload in IPA: none',
    'DiskArbitration',
    'AudioUnit.framework',
]
if not parent_mode:
    verify_markers.extend(['0.0.45', 'BUILD_VERSION" == "45"'])
for marker in verify_markers:
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 16 IPA verification marker missing: {marker}')
if '(mono\\.cecil' in verify.lower():
    raise SystemExit('ERROR: Step 16 IPA verifier must no longer reject the intentional Mono.Cecil runtime dependency.')

if not parent_mode:
    codemagic = Path('codemagic.yaml').read_text()
    for marker in (
        'ios-step-16-1:',
        'Step 16.1 - Managed Preparation Foundation Hotfix',
        'max_build_duration: 120',
        '$HOME/.cache/sts2launcher/godot-step15',
        'bash scripts/codemagic-build-step16.sh',
        'artifacts/StS2-Launcher-Step-16.1.ipa',
        'artifacts/step16-build-summary.txt',
    ):
        if marker not in codemagic:
            raise SystemExit(f'ERROR: Step 16 Codemagic marker missing: {marker}')

third_party = Path('THIRD_PARTY.md').read_text()
if '### Mono.Cecil 0.11.6' not in third_party:
    raise SystemExit('ERROR: Step 16 runtime Mono.Cecil attribution missing.')
if not parent_mode and 'Step 16 now intentionally uses Mono.Cecil at runtime' not in third_party:
    raise SystemExit('ERROR: standalone Step 16 runtime Mono.Cecil scope documentation missing.')
if parent_mode and 'Mono.Cecil at runtime' not in third_party:
    raise SystemExit('ERROR: later-step source no longer documents runtime Mono.Cecil usage.')

print('Step 16 Managed Preparation Foundation source validation: PASS')
print('  Steps 01-15 regression guards retained')
print('  Mono.Cecil 0.11.6 added as the runtime metadata/IL file transformer')
print('  Gate A: project-owned fixture read without Assembly.Load')
print('  Gate B: fixture-only write/reopen under launcher-private scratch storage')
print('  Gate C: controlled project-owned IL constant rewrite 7 -> 42')
print('  Gate D: real receipt-backed managed modules read-only; every .dll/.exe candidate SHA-1 rechecked and no dependency resolution')
print('  No real game rewrite/execution, FMOD/Spine integration, Cloud or Workshop added')
PY
