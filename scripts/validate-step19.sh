#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Step 19.2 may add only the expression-host compatibility subsystem. Keep the
# physically closed Step 18 boundary and everything beneath it regression-protected.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step18.sh

python3 - <<'PY'
from pathlib import Path
import hashlib
import os
import plistlib

parent_mode = os.environ.get("STS2_VALIDATE_AS_PARENT") == "1"

required = [
    Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibility.cs'),
    Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibilityGate.cs'),
    Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibilityGateResult.cs'),
    Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibilityGateSequence.cs'),
    Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibilityProgress.cs'),
    Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibilitySummary.cs'),
    Path('tests/StS2Launcher.Core.Tests/ExpressionInterpreterCompatibilityTests.cs'),
    Path('scripts/build-step19.sh'),
    Path('scripts/run-unit-tests-step19.sh'),
    Path('scripts/codemagic-build-step19.sh'),
    Path('scripts/verify-step19-ipa.sh'),
    Path('docs/STEP-19-DESIGN.md'),
    Path('docs/STEP-19-TEST.md'),
    Path('docs/STEP-19.2-FRAMEWORK-BOUNDARY-FIX.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 19 artifact missing: {path}')

# Exact hashes protect the physically proven Step 18/17 implementation from
# accidental edits while Step 19.2 evolves.
protected_hashes = {
    Path('src/StS2Launcher.Core/RealAssemblyRewriteWorkspace.cs'):
        'eea878b5674f8cb81d6c925072a1273fef7128b8e1d1122c768ae9d8aba948b6',
    Path('tests/StS2Launcher.Core.Tests/RealAssemblyRewriteWorkspaceTests.cs'):
        '1f5c63118f7082d4d2455f197f81ffd076727642c9691a976d2c06166c06ff04',
    Path('src/StS2Launcher.Core/CompatibilityCallSiteAnalysis.cs'):
        'ad918f6a6840bb70b9bbd5c4c6d8202e2818fbb3077977806450add99c9b285b',
}
for path, expected in protected_hashes.items():
    actual = hashlib.sha256(path.read_bytes()).hexdigest()
    if actual != expected:
        raise SystemExit(f'ERROR: physically proven regression-protected file changed: {path}\nexpected {expected}\nactual   {actual}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if parent_mode:
    try:
        build_number = int(str(plist.get('CFBundleVersion')))
    except ValueError as exc:
        raise SystemExit('ERROR: Step 19 parent validation requires a numeric build version.') from exc
    if build_number < 54:
        raise SystemExit('ERROR: Step 19 parent validation requires build >= 54.')
else:
    if plist.get('CFBundleShortVersionString') != '0.0.54' or str(plist.get('CFBundleVersion')) != '54':
        raise SystemExit('ERROR: Step 19.2 must be version 0.0.54 (54).')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
version_markers = () if parent_mode else (
    '<ApplicationVersion>54</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.54</ApplicationDisplayVersion>',
)
for marker in version_markers + (
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
    '<ForceLoad>false</ForceLoad>',
    '<SmartLink>false</SmartLink>',
):
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 19.2 iOS/regression marker missing: {marker}')
if '<TrimmerRootAssembly Include="Mono.Cecil" />' in csproj:
    raise SystemExit('ERROR: preserve the physically proven full-trim Mono.Cecil path; do not blanket-root Cecil.')

core = Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibility.cs').read_text()
for marker in (
    'public sealed class ExpressionInterpreterCompatibility',
    'WorkRootName = "Step19-ExpressionInterpreterCompatibility"',
    'SourceRootName = "source"',
    'PreparedRootName = "prepared"',
    'RunInterpreterCapabilityAndWorkspaceCloneAsync',
    'RunRealCompileTargetDiscovery',
    'RunHostFallbackPreparedCopy',
    'RunIsolationAuditAsync',
    'var captured = 17;',
    'CreateExpression().Compile();',
    'Compile(preferInterpretation: false)',
    'Compile(preferInterpretation: true)',
    'RuntimeFeature.IsDynamicCodeSupported',
    'RuntimeFeature.IsDynamicCodeCompiled',
    'OperatingSystem.IsIOS()',
    'iOS no-dynamic-code fallback precondition proven:',
    'Host System.Linq.Expressions identity:',
    'SteamOfflineInstallInspection',
    'data_sts2_macos_arm64',
    'data_sts2_macos_x86_64',
    'Every workspace source copy receipt SHA-1 verified: YES',
    'IsExpressionCompileMethod',
    'System.Linq.Expressions.LambdaExpression',
    'System.Linq.Expressions.Expression`1',
    'CaptureStrongNameState(module)',
    'IsPlatformFrameworkImplementationAssembly',
    'ModuleAttributes.ILOnly',
    'var rewriteTargets = Array.Empty<TargetAssemblySnapshot>();',
    'const long rewriteSupported = 0;',
    'const bool noRewriteRequired = true;',
    'Assemblies selected for Cecil mutation: 0',
    'Gate B compatibility disposition: HOST RUNTIME FALLBACK — NO GAME/APPLICATION IL REWRITE REQUIRED',
    'copied desktop System.* framework/ReadyToRun images are diagnostic payload inputs only',
    'Step 19.2 invariant violated: expression compatibility must not select any assembly for Cecil mutation.',
    'File.Copy(sourcePath, destinationPath, overwrite: false);',
    'Step 19 no-op prepared copy differs from its verified source:',
    'Cecil assembly writes performed by Gate C: 0',
    'Strong-name flags/public keys/tokens modified: NO',
    'System.* framework implementation assemblies written by Cecil: NO',
    'Non-IL-only/ReadyToRun-or-mixed-mode assemblies written by Cecil: NO',
    'Consumer/game assemblies rewritten: NO',
    'Step 19.2 isolation invariant violated: this compatibility class must complete with zero managed assembly mutations.',
    'Step 19.2 no-op prepared file differs from its receipt-backed source:',
    'Prepared assemblies intentionally rewritten: 0',
    'Gate C/Gate D managed assembly Cecil writes: 0',
    'Compatibility disposition: HOST RUNTIME FALLBACK — NO GAME/APPLICATION IL REWRITE REQUIRED',
    'Original Step 12 install unchanged: YES',
    'WorkspaceOnlyAssemblyResolver',
    'AssemblyIdentityMatches(name, candidate)',
    'AssemblyIdentityMatchesIgnoringVersion(name, candidate)',
    '[workspace version-unified]',
    'RejectingCatalogProbeResolver',
    'MetadataResolver = metadataResolver',
    'EnsureWorkspaceResolverBound(module, resolver)',
    'Fallback to runtime/system/live-install/network resolver paths: NO',
    'Game assembly loaded/executed: NO',
    'Stage: {stage}',
):
    if marker not in core:
        raise SystemExit(f'ERROR: Step 19.2 compatibility marker missing: {marker}')

# The production Step 19.2 implementation is intentionally read-only from Cecil's
# perspective. If this changes, the subsystem has regressed back toward the 19.1
# mixed-mode/framework mutation failure.
for forbidden in (
    '.Write(',
    'module.Attributes &= ~ModuleAttributes.StrongNameSigned',
    'ApplyPreferInterpretationRewrite',
    'CreatePreferInterpretationOverload',
    'SetInt32ConstantToOnePreservingEncoding',
    'StrongNameKeyPair',
    'StrongNameKeyBlob',
    'StrongNameKeyContainer',
    'DefaultAssemblyResolver',
    'Assembly.Load(',
    'Assembly.LoadFrom(',
    'Assembly.LoadFile(',
    'Activator.CreateInstance(',
    'MethodInfo.Invoke',
    'new DynamicMethod(',
    'AssemblyBuilder.DefineDynamicAssembly',
    'SteamClient',
    'HttpClient',
    'ClientWebSocket',
    'SteamContentDiscoveryAttempt',
    'SteamResumableDepotDownloadAttempt',
):
    if forbidden in core:
        raise SystemExit(f'ERROR: Step 19.2 gained forbidden mutation/runtime/network behavior: {forbidden}')

# Game-facing Cecil opens stay centralized through the Step 18-proven explicit
# resolver helper plus strict workspace dependency/catalog probes.
if core.count('ModuleDefinition.ReadModule(') != 3:
    raise SystemExit('ERROR: Step 19.2 expected exactly three direct Cecil ReadModule call sites (bound read helper + workspace dependency open + rejecting catalog probe).')

for marker in (
    'var preparedRoot = Path.Combine(_workRoot, PreparedRootName);',
    'var sourcePath = ResolveChildPath(workspace.SourceRoot, relative);',
    'var installPath = ResolveChildPath(workspace.ManagedRoot, relative);',
    'var preparedHash = ComputeSha1Hex(destinationPath);',
    'if (!preparedHash.Equals(sourceHash, StringComparison.OrdinalIgnoreCase))',
):
    if marker not in core:
        raise SystemExit(f'ERROR: Step 19.2 byte-identical isolation marker missing: {marker}')

seq = Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibilityGateSequence.cs').read_text()
summary = Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibilitySummary.cs').read_text()
for marker in (
    'var expected = (ExpressionInterpreterCompatibilityGate)(_results.Count + 1);',
    'Cannot advance after the first failed expression-interpreter compatibility gate.',
    '_results.Count == 4',
    'EXPRESSION INTERPRETER COMPATIBILITY PASS — {PassedGates}/4',
):
    if marker not in seq + summary:
        raise SystemExit(f'ERROR: Step 19.2 ordered-gate contract missing: {marker}')

testproj = Path('tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj').read_text()
if '<PackageReference Include="Mono.Cecil" Version="0.11.6" />' not in testproj:
    raise SystemExit('ERROR: Step 19.2 host tests must keep the direct Mono.Cecil 0.11.6 pin.')

tests = Path('tests/StS2Launcher.Core.Tests/ExpressionInterpreterCompatibilityTests.cs').read_text()
for marker in (
    'OrderedExpressionInterpreterGatesReachFourOfFourPass',
    'ExpressionInterpreterGatesStopAfterFirstFailure',
    'RealWorkspaceExpressionCompileCallsUseHostFallbackAndPreparedTreeStaysByteIdentical',
    'StrongNameConsumerTargetRemainsByteIdenticalWithIdentityAndSignatureStateUntouched',
    'NoConsumerExpressionCompileTargetPassesAsNoRewriteRequiredAndPreparedTreeStaysIdentical',
    'FrameworkImplementationCompileSitesAreDiagnosticOnlyAndNeverRewritten',
    'NonFrameworkNonIlOnlyConsumerIsClassifiedReadOnlyAndNeverWritten',
    'ClearIlOnlyCorFlag',
    'GenericParameterless',
    'LiteralFalseShort',
    'LiteralFalseLong',
    'UnsafeBranchTargetParameterless',
    'CrossingShortBranchParameterless',
    'OpCodes.Ldc_I4_S, (sbyte)0',
    'OpCodes.Ldc_I4, 0',
    'Direct Compile() sites structurally safe for the old insertion design: 2',
    'Direct Compile sites inside non-System.* consumer assemblies: 9',
    'Direct Compile sites inside System.* framework implementation assemblies: 9',
    'Direct Compile sites inside non-IL-only/ReadyToRun-or-mixed-mode images: 9',
    'Assemblies selected for Cecil mutation: 0',
    'Cecil assembly writes performed by Gate C: 0',
    'HOST RUNTIME FALLBACK — NO GAME/APPLICATION IL REWRITE REQUIRED',
    'Original Step 12 install unchanged: YES',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 19.2 host-test marker missing: {marker}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
standalone_ui_markers = () if parent_mode else (
    'STEP 19.2 — EXPRESSION INTERPRETER COMPATIBILITY',
    'Version 0.0.54',
    'MONO.CECIL 0.11.6 • HOST RUNTIME FALLBACK / FRAMEWORK BOUNDARY / ZERO-WRITE ISOLATION',
    'Steps 01–18 are complete on the physical iPhone.',
)
for marker in standalone_ui_markers + (
    'Step 19.2 — Expression Interpreter Compatibility (ordered gates A–D)',
    'Run Gates A–D — Host Fallback → Framework Boundary → Zero-Write Prep → Isolation Audit',
    'Compile(), Compile(preferInterpretation: false), and Compile(preferInterpretation: true)',
    'RunExpressionInterpreterCompatibilityAsync',
    '_expressionInterpreterCompatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync',
    '_expressionInterpreterCompatibility.RunRealCompileTargetDiscovery',
    '_expressionInterpreterCompatibility.RunHostFallbackPreparedCopy',
    '_expressionInterpreterCompatibility.RunIsolationAuditAsync',
    'STEP 19.2 GATE C — zero Cecil writes; build byte-identical prepared tree and prove immediate SHA-1 equality.',
    'PASS: STEP 19.2 EXPRESSION INTERPRETER COMPATIBILITY — 4/4',
    'Step 18 remains closed/protected',
    'Run Foundation 5/5 Regression',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 19.2 UI/gate marker missing: {marker}')

build = Path('scripts/build-step19.sh').read_text()
for marker in (
    'bash scripts/validate-step19.sh',
    'fixtures/StS2Launcher.Step16.Fixture/StS2Launcher.Step16.Fixture.csproj',
    'bash scripts/build-godot-step15.sh',
    'dotnet publish "$PROJECT"',
    'Step15GodotSmokeProject',
    'Step16Fixtures',
    'StS2-Launcher-Step-19.ipa',
):
    if marker not in build:
        raise SystemExit(f'ERROR: Step 19.2 build-wrapper marker missing: {marker}')

verify = Path('scripts/verify-step19-ipa.sh').read_text()
for marker in (
    '0.0.54',
    'BUILD_VERSION" == "54"',
    'Step16Fixtures/StS2Launcher.Step16.Fixture.dll',
    'cmp -s "$FIXTURE_SOURCE" "$FIXTURE"',
    'Real StS2/proprietary payload in IPA: none',
    'DiskArbitration',
    'AudioUnit.framework',
    'Expected device UI: STEP 19.2 — EXPRESSION INTERPRETER COMPATIBILITY',
):
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 19.2 IPA verification marker missing: {marker}')

run_tests = Path('scripts/run-unit-tests-step19.sh').read_text()
for marker in (
    'dotnet test "$TEST_PROJECT"',
    'LogFileName=step19.trx',
    'step19-unit-tests.log',
):
    if marker not in run_tests:
        raise SystemExit(f'ERROR: Step 19.2 host-test runner marker missing: {marker}')

cm_script = Path('scripts/codemagic-build-step19.sh').read_text()
for marker in (
    'DOTNET_SDK_VERSION="${DOTNET_SDK_VERSION:-9.0.314}"',
    'DOTNET_WORKLOAD_SET="${DOTNET_WORKLOAD_SET:-9.0.314.3}"',
    'bash scripts/validate-step19.sh',
    'bash scripts/run-unit-tests-step19.sh',
    'bash scripts/build-step19.sh',
    'bash scripts/verify-step19-ipa.sh artifacts/StS2-Launcher-Step-19.ipa',
    'Step 18 regression: real assembly rewrite workspace 4/4 + OfflineReady + Foundation closure retained',
    'artifacts/step19-build-summary.txt',
):
    if marker not in cm_script:
        raise SystemExit(f'ERROR: Step 19.2 Codemagic-build marker missing: {marker}')

if not parent_mode:
    codemagic = Path('codemagic.yaml').read_text()
    for marker in (
        'ios-step-19-2:',
        'Step 19.2 - Host Expression Fallback and Framework Boundary',
        'max_build_duration: 120',
        '$HOME/.cache/sts2launcher/godot-step15',
        'bash scripts/codemagic-build-step19.sh',
        'artifacts/StS2-Launcher-Step-19.ipa',
        'artifacts/step19-build-summary.txt',
    ):
        if marker not in codemagic:
            raise SystemExit(f'ERROR: Step 19.2 Codemagic workflow marker missing: {marker}')

# Source archives must not ship game/proprietary payloads. Historical docs may
# mention names, so inspect file names rather than arbitrary text.
for path in Path('.').rglob('*'):
    if not path.is_file():
        continue
    normalized = str(path).replace('\\', '/').lower()
    name = path.name.lower()
    if name == 'sts2.dll' or 'slaythespire2.app/' in normalized or name.startswith('libfmod') or 'spine_godot' in name:
        raise SystemExit(f'ERROR: Step 19.2 source archive contains forbidden game/proprietary payload: {path}')

print('Step 19.2 Expression Interpreter Compatibility source validation: PASS' + (' (parent regression mode)' if parent_mode else ''))
print('  Steps 01-18 regression guards retained; critical Step 17/18 implementation hashes unchanged')
print('  Gate A: Compile()/Compile(false)/Compile(true) host proof + dynamic-code flags + fresh receipt-backed arm64/shared workspace')
print('  Gate B: read-only direct Compile classification across consumer/framework and IL-only/ReadyToRun boundaries; zero mutation targets')
print('  Gate C: zero Cecil assembly writes; complete prepared tree must remain byte-identical to verified source')
print('  Gate D: complete source/prepared/live-install SHA-1 isolation audit with zero managed mutations')
print('  Copied desktop framework mutation, game execution, Harmony/MonoMod, broad Reflection.Emit replacement, FMOD/Spine runtime integration, Cloud and Workshop remain out of scope')
PY
