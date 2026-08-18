#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Step 18 must preserve the physically proven Step 17 compatibility-analysis subsystem.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step17.sh

python3 - <<'PY'
from pathlib import Path
import plistlib

required = [
    Path('src/StS2Launcher.Core/RealAssemblyRewriteGate.cs'),
    Path('src/StS2Launcher.Core/RealAssemblyRewriteGateResult.cs'),
    Path('src/StS2Launcher.Core/RealAssemblyRewriteGateSequence.cs'),
    Path('src/StS2Launcher.Core/RealAssemblyRewriteSummary.cs'),
    Path('src/StS2Launcher.Core/RealAssemblyRewriteProgress.cs'),
    Path('src/StS2Launcher.Core/RealAssemblyRewriteWorkspace.cs'),
    Path('tests/StS2Launcher.Core.Tests/RealAssemblyRewriteWorkspaceTests.cs'),
    Path('scripts/build-step18.sh'),
    Path('scripts/run-unit-tests-step18.sh'),
    Path('scripts/codemagic-build-step18.sh'),
    Path('scripts/verify-step18-ipa.sh'),
    Path('docs/STEP-18-DESIGN.md'),
    Path('docs/STEP-18-TEST.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 18 artifact missing: {path}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if plist.get('CFBundleShortVersionString') != '0.0.48' or str(plist.get('CFBundleVersion')) != '48':
    raise SystemExit('ERROR: Step 18 must be version 0.0.48 (48).')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
for marker in (
    '<ApplicationVersion>48</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.48</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
    '<ForceLoad>false</ForceLoad>',
    '<SmartLink>false</SmartLink>',
):
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 18 iOS/regression marker missing: {marker}')
if '<TrimmerRootAssembly Include="Mono.Cecil" />' in csproj:
    raise SystemExit('ERROR: Step 18 must preserve the physically proven normal full-trim Mono.Cecil path; do not blanket-root it.')

workspace = Path('src/StS2Launcher.Core/RealAssemblyRewriteWorkspace.cs').read_text()
for marker in (
    'public sealed class RealAssemblyRewriteWorkspace',
    'WorkRootName = "Step18-RealAssemblyRewrite"',
    'RunWorkspaceCloneAsync',
    'RunPrimaryRoundTrip',
    'RunNeutralIlRewrite',
    'RunIsolationAuditAsync',
    'SteamOfflineInstallInspection',
    'SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt',
    'data_sts2_macos_arm64',
    'data_sts2_macos_x86_64',
    'Every workspace source copy receipt SHA-1 verified: YES',
    'module.Write(outputPath, new WriterParameters { WriteSymbols = false })',
    'Instruction.Create(OpCodes.Nop)',
    'method.Body.GetILProcessor().InsertBefore(first',
    'Behaviorally significant game fix attempted: NO',
    'Original Step 12 install unchanged: YES',
    'ResolveChildPath(workspace.ManagedRoot, relative)',
    'ComputeSha1HexAsync(installPath, cancellationToken)',
    'WorkspaceOnlyAssemblyResolver',
    'Dependency resolver scope: SHA-1-verified Step 18 workspace ONLY',
    'Fallback to runtime/system/live-install/network resolver paths: NO',
    'Resolved dependency file SHA-1 rechecked immediately before Cecil open: YES',
    '_trustedFileSha1.TryGetValue(candidate, out var expectedSha1)',
    'Game assembly loaded/executed: NO',
):
    if marker not in workspace:
        raise SystemExit(f'ERROR: Step 18 rewrite-workspace marker missing: {marker}')

# The Step 18 class is local-file/Cecil only. Steam/network/runtime loading remains forbidden.
for forbidden in (
    'DefaultAssemblyResolver',
    'Assembly.Load(',
    'Activator.CreateInstance(',
    'MethodInfo.Invoke',
    'SteamClient',
    'HttpClient',
    'ClientWebSocket',
    'SteamSessionStore',
    'SteamContentDiscoveryAttempt',
    'SteamResumableDepotDownloadAttempt',
):
    if forbidden in workspace:
        raise SystemExit(f'ERROR: Step 18 rewrite workspace gained forbidden runtime/network behavior: {forbidden}')

# Guard the critical isolation shape: real-install paths are only opened for receipt reads/hashes;
# all Cecil writes target outputPath beneath the project-owned Step 18 work root.
if 'module.Write(sourcePath' in workspace or 'module.Write(installPath' in workspace:
    raise SystemExit('ERROR: Step 18 contains a Cecil write targeting a source/install path.')
for marker in (
    'var roundTripRoot = Path.Combine(_workRoot, RoundTripRootName);',
    'var rewriteRoot = Path.Combine(_workRoot, RewrittenRootName);',
    'var destinationPath = ResolveChildPath(sourceRoot, relative);',
):
    if marker not in workspace:
        raise SystemExit(f'ERROR: Step 18 launcher-private output-root marker missing: {marker}')

seq = Path('src/StS2Launcher.Core/RealAssemblyRewriteGateSequence.cs').read_text()
summary = Path('src/StS2Launcher.Core/RealAssemblyRewriteSummary.cs').read_text()
for marker in (
    'var expected = (RealAssemblyRewriteGate)(_results.Count + 1);',
    'Cannot advance after the first failed real-assembly rewrite gate.',
    '_results.Count == 4',
    'REAL ASSEMBLY REWRITE WORKSPACE PASS — {PassedGates}/4',
):
    if marker not in seq + summary:
        raise SystemExit(f'ERROR: Step 18 ordered-gate contract missing: {marker}')

tests = Path('tests/StS2Launcher.Core.Tests/RealAssemblyRewriteWorkspaceTests.cs').read_text()
for marker in (
    'OrderedRealAssemblyRewriteGatesReachFourOfFourPass',
    'RealAssemblyRewriteGatesStopAfterFirstFailure',
    'RealArm64AssemblyCopyRoundTripNeutralRewriteAndIsolationPass',
    'data_sts2_macos_arm64/sts2.dll',
    'data_sts2_macos_x86_64/sts2.dll',
    'insert one IL NOP at method entry',
    'WriteSyntheticAssemblyWithExternalEnumDefault',
    'GodotSharp',
    'Workspace-only dependency resolutions observed:',
    'Original Step 12 install unchanged: YES',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 18 host-test marker missing: {marker}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 18.1 — REAL ASSEMBLY REWRITE WORKSPACE',
    'Version 0.0.48',
    'Steps 01–17 are complete on the physical iPhone.',
    'Step 18 — Real Assembly Rewrite Workspace (ordered gates A–D)',
    'Run Gates A–D — Clone ARM64 → Real Roundtrip → Neutral NOP → Isolation Audit',
    'RunRealAssemblyRewriteWorkspaceAsync',
    '_realAssemblyRewriteWorkspace.RunWorkspaceCloneAsync',
    '_realAssemblyRewriteWorkspace.RunPrimaryRoundTrip',
    '_realAssemblyRewriteWorkspace.RunNeutralIlRewrite',
    '_realAssemblyRewriteWorkspace.RunIsolationAuditAsync',
    'PASS: STEP 18 REAL ASSEMBLY REWRITE WORKSPACE — 4/4',
    'Run Gates A–D — ARM64 Scope → Actual IL Calls → Native/Platform → Dependency Map',
    'Run Foundation 5/5 Regression',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 18 UI/gate marker missing: {marker}')

build = Path('scripts/build-step18.sh').read_text()
for marker in (
    'bash scripts/validate-step18.sh',
    'fixtures/StS2Launcher.Step16.Fixture/StS2Launcher.Step16.Fixture.csproj',
    'bash scripts/build-godot-step15.sh',
    'dotnet publish "$PROJECT"',
    'Step15GodotSmokeProject',
    'Step16Fixtures',
    'StS2-Launcher-Step-18.ipa',
):
    if marker not in build:
        raise SystemExit(f'ERROR: Step 18 build-wrapper marker missing: {marker}')

verify = Path('scripts/verify-step18-ipa.sh').read_text()
for marker in (
    '0.0.48',
    'BUILD_VERSION" == "48"',
    'Step16Fixtures/StS2Launcher.Step16.Fixture.dll',
    'cmp -s "$FIXTURE_SOURCE" "$FIXTURE"',
    'Real StS2/proprietary payload in IPA: none',
    'DiskArbitration',
    'AudioUnit.framework',
    'Expected device UI: STEP 18.1 — REAL ASSEMBLY REWRITE WORKSPACE',
):
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 18 IPA verification marker missing: {marker}')

codemagic = Path('codemagic.yaml').read_text()
for marker in (
    'ios-step-18-1:',
    'Step 18.1 - Workspace-Only Dependency Resolution',
    'max_build_duration: 120',
    '$HOME/.cache/sts2launcher/godot-step15',
    'bash scripts/codemagic-build-step18.sh',
    'artifacts/StS2-Launcher-Step-18.ipa',
    'artifacts/step18-build-summary.txt',
):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Step 18 Codemagic marker missing: {marker}')

print('Step 18 Real Assembly Rewrite Workspace source validation: PASS')
print('  Steps 01-17 regression guards retained')
print('  Gate A: receipt-backed macOS arm64/shared managed payload cloned into launcher-private workspace')
print('  Gate B: real copied primary sts2.dll Cecil write/reopen using strict workspace-only dependency resolution')
print('  Gate C: semantics-neutral one-NOP IL rewrite on copied primary assembly only')
print('  Gate D: complete workspace-source + original-install SHA-1 isolation audit')
print('  Cecil writer-required dependency resolution is allowed only inside the verified Step 18 workspace; no runtime/system/live-install/network fallback, Assembly.Load/game execution, FMOD/Spine runtime integration, Cloud or Workshop added')
PY
