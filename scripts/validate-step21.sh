#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Step 21 adds only the prepared runtime/framework-binding subsystem. Preserve
# the physically closed Step 20 dynamic managed execution foundation and every
# earlier regression boundary beneath it.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step20.sh

python3 - <<'PY'
from pathlib import Path
import hashlib
import plistlib
import re

required = [
    Path('src/StS2Launcher.Core/PreparedRuntimeFrameworkBinding.cs'),
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingPlan.cs'),
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGate.cs'),
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGateResult.cs'),
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGateSequence.cs'),
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingProgress.cs'),
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingSummary.cs'),
    Path('tests/StS2Launcher.Core.Tests/PreparedRuntimeFrameworkBindingTests.cs'),
    Path('scripts/build-step21.sh'),
    Path('scripts/run-unit-tests-step21.sh'),
    Path('scripts/codemagic-build-step21.sh'),
    Path('scripts/verify-step21-ipa.sh'),
    Path('docs/STEP-21-DESIGN.md'),
    Path('docs/STEP-21-TEST.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 21 artifact missing: {path}')

# Exact hashes protect the physically proven Steps 17-20 implementation lines
# from accidental edits while Step 21 is introduced.
protected_hashes = {
    Path('src/StS2Launcher.Core/CompatibilityCallSiteAnalysis.cs'):
        'ad918f6a6840bb70b9bbd5c4c6d8202e2818fbb3077977806450add99c9b285b',
    Path('src/StS2Launcher.Core/RealAssemblyRewriteWorkspace.cs'):
        'eea878b5674f8cb81d6c925072a1273fef7128b8e1d1122c768ae9d8aba948b6',
    Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibility.cs'):
        '2396ce56891de43d6839ab6028a38668de010184b46c535ac7c552b85d8c2742',
    Path('tests/StS2Launcher.Core.Tests/ExpressionInterpreterCompatibilityTests.cs'):
        '10470e826b72bd5163b3872beaeedf01ab61aa14830f876312b77b852aa2d9b8',
    Path('src/StS2Launcher.Core/DynamicManagedExecutionFoundation.cs'):
        'ae9e7e2cc236f7309d2924eb9999c9eb98ecb997a4f49a0f88fac8ca2d86ad46',
    Path('tests/StS2Launcher.Core.Tests/DynamicManagedExecutionFoundationTests.cs'):
        'f2ba252f61c66dd0767447aff822accd931654740c78fa96b5a01886eb98b8de',
}
for path, expected in protected_hashes.items():
    actual = hashlib.sha256(path.read_bytes()).hexdigest()
    if actual != expected:
        raise SystemExit(f'ERROR: physically proven regression-protected file changed: {path}\nexpected {expected}\nactual   {actual}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if plist.get('CFBundleShortVersionString') != '0.0.56' or str(plist.get('CFBundleVersion')) != '56':
    raise SystemExit('ERROR: Step 21 must be version 0.0.56 (56).')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
for marker in (
    '<ApplicationVersion>56</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.56</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<MtouchInterpreter>-all</MtouchInterpreter>',
    '<Target Name="Step21ValidateInterpreterPolicy"',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
    '<ForceLoad>false</ForceLoad>',
    '<SmartLink>false</SmartLink>',
):
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 21 iOS/regression marker missing: {marker}')
for forbidden in (
    '<UseInterpreter>true</UseInterpreter>',
    '<PublishAot>true</PublishAot>',
    '<TrimmerRootAssembly Include="Mono.Cecil" />',
):
    if forbidden in csproj:
        raise SystemExit(f'ERROR: Step 21 iOS policy contains forbidden/broader setting: {forbidden}')

core = Path('src/StS2Launcher.Core/PreparedRuntimeFrameworkBinding.cs').read_text()
for marker in (
    'public sealed class PreparedRuntimeFrameworkBinding',
    'WorkRootName = "Step21-PreparedRuntimeBinding"',
    'SourceRootName = "source"',
    'PreparedRootName = "prepared"',
    'PlanFileName = "runtime-binding-plan.json"',
    'RunRuntimePayloadClassificationAsync',
    'RunHostFrameworkBindingPlan',
    'RunPreparedRuntimeAssemblySetAsync',
    'RunClosureAuditAsync',
    'SteamOfflineInstallInspection',
    'IsMacOsArm64ManagedPath',
    'IsMacOsX8664ManagedPath',
    'IsPrimaryArm64StS2Path',
    'ModuleAttributes.ILOnly',
    'AssemblyLoadContext.Default.LoadFromAssemblyName',
    'IsHostFrameworkProbeCandidate',
    'ResolveWorkspaceReference',
    'WorkspaceVersionTooLow',
    'WorkspaceVersionAmbiguity',
    'WorkspaceByteAmbiguity',
    'HostFrameworkUnavailable',
    'NonIlOnlyWorkspaceAssembly',
    'HostPrivateSimpleNameConflict',
    'Runtime closure ready for first real CLR load:',
    'blockers are first-class plan output, not a Gate B failure',
    'Cecil assembly writes performed by Step 21 Gate C: 0',
    'Prepared assembly bytes remain receipt-identical: YES',
    'Post-preparation OfflineReady exact-tree verification: YES',
    'EnsureNoStS2AssemblyLoaded',
    'StS2 assembly loaded/executed: NO',
):
    if marker not in core:
        raise SystemExit(f'ERROR: Step 21 runtime/framework-binding marker missing: {marker}')

# Step 21 is planning/preparation only. It may ask the default ALC to bind
# framework-shaped contracts, but may not CLR-load a game/private assembly or
# write managed assembly bytes.
for forbidden in (
    'LoadFromStream(',
    'LoadFromAssemblyPath(',
    'Assembly.Load(',
    'Assembly.LoadFrom(',
    'Assembly.LoadFile(',
    'module.Write(',
    'assembly.Write(',
    '.Invoke(',
    'HttpClient',
    'ClientWebSocket',
    'SteamClient',
):
    if forbidden in core:
        raise SystemExit(f'ERROR: Step 21 production subsystem gained forbidden execution/network/write behavior: {forbidden}')
if core.count('AssemblyLoadContext.Default.LoadFromAssemblyName') != 1:
    raise SystemExit('ERROR: Step 21 expects exactly one framework-only default ALC bind probe site.')

plan = Path('src/StS2Launcher.Core/RuntimeFrameworkBindingPlan.cs').read_text()
for marker in (
    'RuntimeFrameworkBindingPlanDocument',
    'RuntimeBindingPreparedAssembly[] PreparedAssemblies',
    'RuntimeBindingHostFramework[] HostFrameworkBindings',
    'RuntimeBindingBlocker[] Blockers',
    'RuntimeBindingEdge[] Edges',
    'bool RuntimeClosureReady',
    'RuntimeFrameworkBindingJsonContext',
):
    if marker not in plan:
        raise SystemExit(f'ERROR: Step 21 persisted-plan contract missing: {marker}')

seq = Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGateSequence.cs').read_text()
summary = Path('src/StS2Launcher.Core/RuntimeFrameworkBindingSummary.cs').read_text()
for marker in (
    'var expected = (RuntimeFrameworkBindingGate)(_results.Count + 1);',
    'Cannot advance after the first failed runtime/framework-binding gate.',
    'Results.Count == 4',
    'PREPARED RUNTIME / FRAMEWORK BINDING PASS — 4/4',
):
    if marker not in seq + summary:
        raise SystemExit(f'ERROR: Step 21 ordered-gate contract missing: {marker}')

tests = Path('tests/StS2Launcher.Core.Tests/PreparedRuntimeFrameworkBindingTests.cs').read_text()
for marker in (
    'OrderedRuntimeFrameworkBindingGatesReachFourOfFourPass',
    'RuntimeFrameworkBindingGatesStopAfterFirstFailure',
    'RealStyleGraphPrefersHostSystemRuntimeAndPreparesOnlyPrivateIlAssemblies',
    'MissingPrivateDependencyIsExplicitPlanBlockerButDoesNotCorruptPreparationAudit',
    'SystemNamedPackageFallsBackToVerifiedWorkspaceWhenHostCannotProvideIt',
    'System.Linq.Expressions',
    'Runtime closure ready for first real CLR load: YES',
    'Runtime closure ready for first real CLR load: NO',
    'Original Step 12 managed install unchanged: YES',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 21 host-test marker missing: {marker}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 21 — PREPARED RUNTIME / FRAMEWORK BINDING',
    'Version 0.0.56',
    'REAL DEPENDENCY GRAPH • HOST FRAMEWORK MAP • PREPARED IL SET • CLOSURE AUDIT',
    'Steps 01–20 are complete and closed on the physical iPhone.',
    'Step 21 — Prepared Runtime / Framework Binding (ordered gates A–D)',
    'Run Gates A–D — Classify Runtime → Bind Host Frameworks → Prepare IL Set → Closure Audit',
    'RunPreparedRuntimeFrameworkBindingAsync',
    '_preparedRuntimeFrameworkBinding.RunRuntimePayloadClassificationAsync',
    '_preparedRuntimeFrameworkBinding.RunHostFrameworkBindingPlan',
    '_preparedRuntimeFrameworkBinding.RunPreparedRuntimeAssemblySetAsync',
    '_preparedRuntimeFrameworkBinding.RunClosureAuditAsync',
    'PASS: STEP 21 PREPARED RUNTIME / FRAMEWORK BINDING — 4/4',
    'Run OfflineReady + Foundation 5/5 to close Step 21',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 21 UI/gate marker missing: {marker}')

build = Path('scripts/build-step21.sh').read_text()
for marker in (
    'bash scripts/validate-step21.sh',
    'dotnet publish "$PROJECT"',
    'STEP21 INTERPRETER POLICY: MtouchInterpreter=-all',
    'Step20DynamicFixtures',
    'StS2-Launcher-Step-21.ipa',
):
    if marker not in build:
        raise SystemExit(f'ERROR: Step 21 build-wrapper marker missing: {marker}')

run_tests = Path('scripts/run-unit-tests-step21.sh').read_text()
for marker in (
    'STS2_STEP20_FIXTURE_ROOT',
    'dotnet test "$TEST_PROJECT"',
    'LogFileName=step21.trx',
    'step21-unit-tests.log',
):
    if marker not in run_tests:
        raise SystemExit(f'ERROR: Step 21 host-test runner marker missing: {marker}')

cm = Path('scripts/codemagic-build-step21.sh').read_text()
for marker in (
    'DOTNET_SDK_VERSION="${DOTNET_SDK_VERSION:-9.0.314}"',
    'DOTNET_WORKLOAD_SET="${DOTNET_WORKLOAD_SET:-9.0.314.3}"',
    'bash scripts/validate-step21.sh',
    'bash scripts/run-unit-tests-step21.sh',
    'bash scripts/build-step21.sh',
    'bash scripts/verify-step21-ipa.sh artifacts/StS2-Launcher-Step-21.ipa',
    'artifacts/step21-build-summary.txt',
):
    if marker not in cm:
        raise SystemExit(f'ERROR: Step 21 Codemagic-build marker missing: {marker}')

verify = Path('scripts/verify-step21-ipa.sh').read_text()
for marker in (
    '0.0.56',
    'BUILD_VERSION" == "56"',
    'Step20DynamicFixtures',
    'StS2Launcher.Step20.DynamicFixture.dll',
    'Expected device UI: STEP 21 — PREPARED RUNTIME / FRAMEWORK BINDING',
):
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 21 IPA verification marker missing: {marker}')

codemagic = Path('codemagic.yaml').read_text()
for marker in (
    'ios-step-21:',
    'Step 21 - Prepared Runtime / Framework Binding',
    'max_build_duration: 120',
    'bash scripts/codemagic-build-step21.sh',
    'artifacts/StS2-Launcher-Step-21.ipa',
    'artifacts/step21-build-summary.txt',
):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Step 21 Codemagic workflow marker missing: {marker}')

# Repository source must never contain game/proprietary payloads.
for path in Path('.').rglob('*'):
    if not path.is_file():
        continue
    normalized = str(path).replace('\\', '/').lower()
    name = path.name.lower()
    if name == 'sts2.dll' or 'slaythespire2.app/' in normalized or name.startswith('libfmod') or 'spine_godot' in name:
        raise SystemExit(f'ERROR: Step 21 source archive contains forbidden game/proprietary payload: {path}')

print('Step 21 Prepared Runtime / Framework Binding source validation: PASS')
print('  Steps 01-20 parent regression validation retained; Steps 17-20 implementation hashes protected')
print('  Gate A: exact OfflineReady ARM64/shared source clone + managed identity/IL-only classification, no game CLR load')
print('  Gate B: real sts2.dll AssemblyRef graph -> iOS-host framework binding / verified private binding / explicit blocker')
print('  Gate C: zero Cecil writes; byte-identical IL-only private/game prepared set + source-generated deterministic JSON plan')
print('  Gate D: complete source/prepared/live hash audit + persisted-plan integrity + host/private simple-name isolation')
print('  4/4 plan pass is deliberately distinct from Runtime closure ready YES/NO; real game CLR load remains out of scope')
PY
