#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Step 20 adds only the dynamic-managed-execution foundation. Preserve the
# physically closed Step 19.2 subsystem and every earlier gate beneath it.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step19.sh

python3 - <<'PY'
from pathlib import Path
import hashlib
import plistlib
import re

required = [
    Path('src/StS2Launcher.Core/DynamicManagedExecutionFoundation.cs'),
    Path('src/StS2Launcher.Core/DynamicManagedExecutionGate.cs'),
    Path('src/StS2Launcher.Core/DynamicManagedExecutionGateResult.cs'),
    Path('src/StS2Launcher.Core/DynamicManagedExecutionGateSequence.cs'),
    Path('src/StS2Launcher.Core/DynamicManagedExecutionProgress.cs'),
    Path('src/StS2Launcher.Core/DynamicManagedExecutionSummary.cs'),
    Path('tests/StS2Launcher.Core.Tests/DynamicManagedExecutionFoundationTests.cs'),
    Path('fixtures/StS2Launcher.Step20.DynamicFixture/StS2Launcher.Step20.DynamicFixture.csproj'),
    Path('fixtures/StS2Launcher.Step20.DynamicFixture/DynamicFixtureProbe.cs'),
    Path('fixtures/StS2Launcher.Step20.DependencyFixture/StS2Launcher.Step20.DependencyFixture.csproj'),
    Path('fixtures/StS2Launcher.Step20.DependencyFixture/DependencyProbe.cs'),
    Path('fixtures/StS2Launcher.Step20.RootFixture/StS2Launcher.Step20.RootFixture.csproj'),
    Path('fixtures/StS2Launcher.Step20.RootFixture/RootFixtureProbe.cs'),
    Path('scripts/build-step20.sh'),
    Path('scripts/run-unit-tests-step20.sh'),
    Path('scripts/codemagic-build-step20.sh'),
    Path('scripts/verify-step20-ipa.sh'),
    Path('docs/STEP-20-DESIGN.md'),
    Path('docs/STEP-20-TEST.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 20 artifact missing: {path}')

# Exact hashes protect the physically proven compatibility-analysis/rewrite/
# expression-boundary implementation from accidental Step 20 edits.
protected_hashes = {
    Path('src/StS2Launcher.Core/CompatibilityCallSiteAnalysis.cs'):
        'ad918f6a6840bb70b9bbd5c4c6d8202e2818fbb3077977806450add99c9b285b',
    Path('src/StS2Launcher.Core/RealAssemblyRewriteWorkspace.cs'):
        'eea878b5674f8cb81d6c925072a1273fef7128b8e1d1122c768ae9d8aba948b6',
    Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibility.cs'):
        '2396ce56891de43d6839ab6028a38668de010184b46c535ac7c552b85d8c2742',
    Path('tests/StS2Launcher.Core.Tests/ExpressionInterpreterCompatibilityTests.cs'):
        '10470e826b72bd5163b3872beaeedf01ab61aa14830f876312b77b852aa2d9b8',
}
for path, expected in protected_hashes.items():
    actual = hashlib.sha256(path.read_bytes()).hexdigest()
    if actual != expected:
        raise SystemExit(f'ERROR: physically proven regression-protected file changed: {path}\nexpected {expected}\nactual   {actual}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if plist.get('CFBundleShortVersionString') != '0.0.55' or str(plist.get('CFBundleVersion')) != '55':
    raise SystemExit('ERROR: Step 20 must be version 0.0.55 (55).')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
for marker in (
    '<ApplicationVersion>55</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.55</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<MtouchInterpreter>-all</MtouchInterpreter>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
    '<ForceLoad>false</ForceLoad>',
    '<SmartLink>false</SmartLink>',
):
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 20 iOS/regression marker missing: {marker}')
for forbidden in (
    '<UseInterpreter>true</UseInterpreter>',
    '<PublishAot>true</PublishAot>',
    '<TrimmerRootAssembly Include="Mono.Cecil" />',
):
    if forbidden in csproj:
        raise SystemExit(f'ERROR: Step 20 iOS policy contains forbidden/broader setting: {forbidden}')

# No fixture can be a project/content/bundle resource input to dotnet publish.
# They must enter the .app only after publish in build-step20.sh.
for project_path in (
    Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj'),
    Path('src/StS2Launcher.Core/StS2Launcher.Core.csproj'),
    Path('tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj'),
):
    text = project_path.read_text()
    if 'StS2Launcher.Step20.' in text:
        raise SystemExit(f'ERROR: Step 20 external fixture leaked into a build-time project reference/resource: {project_path}')

core = Path('src/StS2Launcher.Core/DynamicManagedExecutionFoundation.cs').read_text()
for marker in (
    'public sealed class DynamicManagedExecutionFoundation',
    'WorkRootName = "Step20-DynamicManagedExecution"',
    'BundleFixtureDirectoryName = "Step20DynamicFixtures"',
    'ManifestFileName = "step20-fixtures.sha256"',
    'StS2Launcher.Step20.DynamicFixture.dll',
    'StS2Launcher.Step20.DependencyFixture.dll',
    'StS2Launcher.Step20.RootFixture.dll',
    'RunFixtureIntegrityAndOfflineReadyAsync',
    'RunDynamicFixtureExecution',
    'RunPrivateDependencyResolution',
    'RunIsolationAuditAsync',
    'SteamOfflineInstallInspection',
    'SHA256.Create()',
    'ReadManagedIdentity',
    'ModuleAttributes.ILOnly',
    'ValidateFixtureReferenceBoundary',
    'method.IsPInvokeImpl',
    'Step20-GateB-DynamicFixture',
    'Step20-GateC-PrivateDependency',
    'new Step20FixtureLoadContext',
    'LoadFromStream(stream)',
    'InvokeInt32Probe',
    'Dynamic fixture result:',
    'Dependent fixture result:',
    'Verified private dependency loads:',
    'AssemblyIdentityMatches',
    'private resolver refuses fallback for non-framework assembly',
    'Step 20 private dependency SHA-256 changed immediately before load',
    'Post-execution OfflineReady exact-tree verification: YES',
    'EnsureNoStS2AssemblyLoaded',
    'StS2 assembly loaded/executed: NO',
    'Network attempted by Step 20: NO',
    'Real managed install modified: NO',
):
    if marker not in core:
        raise SystemExit(f'ERROR: Step 20 dynamic-managed-execution marker missing: {marker}')

# Step 20 is fixture-only. No game CLR loading, network, or broad convenience
# loading APIs are allowed in the new production subsystem.
for forbidden in (
    'Assembly.Load(',
    'Assembly.LoadFrom(',
    'Assembly.LoadFile(',
    'Assembly.LoadFile',
    'Assembly.LoadFrom',
    'HttpClient',
    'ClientWebSocket',
    'SteamClient',
    'SteamContentDiscoveryAttempt',
    'SteamResumableDepotDownloadAttempt',
    'sts2.dll"',
    'GodotSharp.dll',
):
    if forbidden in core:
        raise SystemExit(f'ERROR: Step 20 production subsystem gained forbidden game/network/load behavior: {forbidden}')

# The one reflection invoke is intentional and must remain centralized in the
# exact-hash project-owned probe helper.
if core.count('.Invoke(null, null)') != 1:
    raise SystemExit('ERROR: Step 20 expected exactly one project-owned reflective probe invocation.')
if core.count('LoadFromStream(stream)') != 2:
    raise SystemExit('ERROR: Step 20 expected exactly two LoadFromStream sites (root/direct + exact private dependency).')

seq = Path('src/StS2Launcher.Core/DynamicManagedExecutionGateSequence.cs').read_text()
summary = Path('src/StS2Launcher.Core/DynamicManagedExecutionSummary.cs').read_text()
for marker in (
    'var expected = (DynamicManagedExecutionGate)(_results.Count + 1);',
    'Cannot advance after the first failed dynamic-managed-execution gate.',
    '_results.Count == 4',
    'DYNAMIC MANAGED EXECUTION FOUNDATION PASS — {PassedGates}/4',
):
    if marker not in seq + summary:
        raise SystemExit(f'ERROR: Step 20 ordered-gate contract missing: {marker}')

fixtures = {
    'DynamicFixtureProbe.cs': Path('fixtures/StS2Launcher.Step20.DynamicFixture/DynamicFixtureProbe.cs').read_text(),
    'DependencyProbe.cs': Path('fixtures/StS2Launcher.Step20.DependencyFixture/DependencyProbe.cs').read_text(),
    'RootFixtureProbe.cs': Path('fixtures/StS2Launcher.Step20.RootFixture/RootFixtureProbe.cs').read_text(),
}
for marker in ('public static int Run()', 'Identity(value)', 'Identity(32)', 'finally'):
    if marker not in fixtures['DynamicFixtureProbe.cs']:
        raise SystemExit(f'ERROR: Step 20 dynamic fixture lost nontrivial IL marker: {marker}')
if 'public static int Add(int left, int right) => left + right;' not in fixtures['DependencyProbe.cs']:
    raise SystemExit('ERROR: Step 20 dependency fixture contract changed.')
if 'DependencyProbe.Add(40, 2)' not in fixtures['RootFixtureProbe.cs']:
    raise SystemExit('ERROR: Step 20 root/dependency fixture contract changed.')

root_fixture_project = Path('fixtures/StS2Launcher.Step20.RootFixture/StS2Launcher.Step20.RootFixture.csproj').read_text()
if '../StS2Launcher.Step20.DependencyFixture/StS2Launcher.Step20.DependencyFixture.csproj' not in root_fixture_project:
    raise SystemExit('ERROR: Step 20 root fixture must retain exactly the project-owned private dependency reference.')

unit = Path('tests/StS2Launcher.Core.Tests/DynamicManagedExecutionFoundationTests.cs').read_text()
for marker in (
    'OrderedDynamicManagedExecutionGatesReachFourOfFourPass',
    'DynamicManagedExecutionGatesStopAfterFirstFailure',
    'ProjectOwnedExternalIlAndPrivateDependencyExecuteWithoutTouchingManagedInstall',
    'GateARejectsTamperedBundledFixtureBeforeRuntimeLoad',
    'GateARejectsUnexpectedNonFrameworkFixtureReferenceEvenWithValidHashManifest',
    'Dynamic fixture result: 42 (expected 42)',
    'Dependent fixture result: 42 (expected 42)',
    'Verified private dependency loads: 1',
    'Post-execution OfflineReady exact-tree verification: YES',
    'StS2 assembly loaded/executed: NO',
):
    if marker not in unit:
        raise SystemExit(f'ERROR: Step 20 host-test marker missing: {marker}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 20 — DYNAMIC MANAGED EXECUTION FOUNDATION',
    'Version 0.0.55',
    'MONO INTERPRETER • EXTERNAL IL LOAD / PRIVATE DEPENDENCY / ISOLATION',
    'Steps 01–19 are complete and closed on the physical iPhone.',
    'Step 20 — Dynamic Managed Execution Foundation (ordered gates A–D)',
    'Run Gates A–D — Fixture Integrity → External IL Execute → Private Dependency → Isolation Audit',
    'RunDynamicManagedExecutionFoundationAsync',
    '_dynamicManagedExecutionFoundation.RunFixtureIntegrityAndOfflineReadyAsync',
    '_dynamicManagedExecutionFoundation.RunDynamicFixtureExecution',
    '_dynamicManagedExecutionFoundation.RunPrivateDependencyResolution',
    '_dynamicManagedExecutionFoundation.RunIsolationAuditAsync',
    'PASS: STEP 20 DYNAMIC MANAGED EXECUTION FOUNDATION — 4/4',
    'Run OfflineReady + Foundation 5/5 to close Step 20',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 20 UI/gate marker missing: {marker}')

build = Path('scripts/build-step20.sh').read_text()
for marker in (
    'bash scripts/validate-step20.sh',
    'dotnet publish "$PROJECT"',
    'Building project-owned Step 20 external managed fixtures as data-only payload',
    'Step20DynamicFixtures',
    'step20-fixtures.sha256',
    'StS2-Launcher-Step-20.ipa',
):
    if marker not in build:
        raise SystemExit(f'ERROR: Step 20 build-wrapper marker missing: {marker}')
# Fixture copy must happen after publish completes.
if build.index('dotnet publish "$PROJECT"') > build.index('STEP20_FIXTURE_DIR="$APP/Step20DynamicFixtures"'):
    raise SystemExit('ERROR: Step 20 fixture payload must be inserted only after dotnet publish, never as an AOT/link input.')

run_tests = Path('scripts/run-unit-tests-step20.sh').read_text()
for marker in (
    'STS2_STEP20_FIXTURE_ROOT',
    'step20-fixtures.sha256',
    'dotnet test "$TEST_PROJECT"',
    'LogFileName=step20.trx',
    'step20-unit-tests.log',
):
    if marker not in run_tests:
        raise SystemExit(f'ERROR: Step 20 host-test runner marker missing: {marker}')

cm = Path('scripts/codemagic-build-step20.sh').read_text()
for marker in (
    'DOTNET_SDK_VERSION="${DOTNET_SDK_VERSION:-9.0.314}"',
    'DOTNET_WORKLOAD_SET="${DOTNET_WORKLOAD_SET:-9.0.314.3}"',
    'bash scripts/validate-step20.sh',
    'bash scripts/run-unit-tests-step20.sh',
    'bash scripts/build-step20.sh',
    'bash scripts/verify-step20-ipa.sh artifacts/StS2-Launcher-Step-20.ipa',
    'artifacts/step20-build-summary.txt',
):
    if marker not in cm:
        raise SystemExit(f'ERROR: Step 20 Codemagic-build marker missing: {marker}')

verify = Path('scripts/verify-step20-ipa.sh').read_text()
for marker in (
    '0.0.55',
    'BUILD_VERSION" == "55"',
    'Step20DynamicFixtures',
    'StS2Launcher.Step20.DynamicFixture.dll',
    'StS2Launcher.Step20.DependencyFixture.dll',
    'StS2Launcher.Step20.RootFixture.dll',
    'step20-fixtures.sha256',
    'shasum -a 256 -c step20-fixtures.sha256',
    'Expected device UI: STEP 20 — DYNAMIC MANAGED EXECUTION FOUNDATION',
):
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 20 IPA verification marker missing: {marker}')

codemagic = Path('codemagic.yaml').read_text()
for marker in (
    'ios-step-20:',
    'Step 20 - Dynamic Managed Execution Foundation',
    'max_build_duration: 120',
    '$HOME/.cache/sts2launcher/godot-step15',
    'bash scripts/codemagic-build-step20.sh',
    'artifacts/StS2-Launcher-Step-20.ipa',
    'artifacts/step20-build-summary.txt',
):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Step 20 Codemagic workflow marker missing: {marker}')

# Repository source must never contain game/proprietary payloads. Generated bin/obj/artifacts
# directories are allowed during Codemagic because build-step20 re-runs validation after host tests.
# The source-ZIP packaging audit separately excludes and rejects generated output.
for path in Path('.').rglob('*'):
    if not path.is_file():
        continue
    normalized = str(path).replace('\\', '/').lower()
    name = path.name.lower()
    if name == 'sts2.dll' or 'slaythespire2.app/' in normalized or name.startswith('libfmod') or 'spine_godot' in name:
        raise SystemExit(f'ERROR: Step 20 source archive contains forbidden game/proprietary payload: {path}')

print('Step 20 Dynamic Managed Execution Foundation source validation: PASS')
print('  Steps 01-19 parent regression validation retained; Step 17/18/19 implementation hashes protected')
print('  Build-time launcher assemblies remain AOT-targeted via MtouchInterpreter=-all; interpreter remains linked for runtime/dynamic managed code')
print('  Gate A: OfflineReady + exact SHA-256/Cecil identity/pure-IL/reference-boundary verification of project-owned external fixtures')
print('  Gate B: fresh AssemblyLoadContext loads an after-publish fixture from verified bytes and executes nontrivial IL to 42')
print('  Gate C: exact identity + immediate SHA-256 private dependency load, no non-framework fallback, transitive execution to 42')
print('  Gate D: fixture/manifest rehash + post-execution OfflineReady exact-tree audit + explicit no-sts2-CLR-load proof')
print('  Real StS2 CLR loading, runtime/framework binding, native game integration, Harmony/MonoMod, FMOD/Spine, Cloud and Workshop remain out of scope')
PY
