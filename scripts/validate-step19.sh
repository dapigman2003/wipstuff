#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Step 19 is allowed to add one compatibility subsystem only. The physically closed
# Step 18 boundary (and everything beneath it) must remain regression-protected.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step18.sh

python3 - <<'PY'
from pathlib import Path
import hashlib
import plistlib

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
    Path('docs/STEP-19.1-STRONG-NAME-IDENTITY-FIX.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 19 artifact missing: {path}')

# Exact hashes protect the most important physically proven Step 18/17 implementation
# files from accidental edits while Step 19 is being developed.
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
if plist.get('CFBundleShortVersionString') != '0.0.53' or str(plist.get('CFBundleVersion')) != '53':
    raise SystemExit('ERROR: Step 19 must be version 0.0.53 (53).')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
for marker in (
    '<ApplicationVersion>53</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.53</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
    '<ForceLoad>false</ForceLoad>',
    '<SmartLink>false</SmartLink>',
):
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 19 iOS/regression marker missing: {marker}')
if '<TrimmerRootAssembly Include="Mono.Cecil" />' in csproj:
    raise SystemExit('ERROR: Step 19 must preserve the physically proven full-trim Mono.Cecil path; do not blanket-root Cecil.')

core = Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibility.cs').read_text()
for marker in (
    'public sealed class ExpressionInterpreterCompatibility',
    'WorkRootName = "Step19-ExpressionInterpreterCompatibility"',
    'SourceRootName = "source"',
    'PreparedRootName = "prepared"',
    'RunInterpreterCapabilityAndWorkspaceCloneAsync',
    'RunRealCompileTargetDiscovery',
    'RunPreferInterpretationRewrite',
    'RunIsolationAuditAsync',
    'var captured = 17;',
    'Compile(preferInterpretation: true)',
    'RuntimeFeature.IsDynamicCodeSupported',
    'RuntimeFeature.IsDynamicCodeCompiled',
    'SteamOfflineInstallInspection',
    'data_sts2_macos_arm64',
    'data_sts2_macos_x86_64',
    'Every workspace source copy receipt SHA-1 verified: YES',
    'IsExpressionCompileMethod',
    'System.Linq.Expressions.LambdaExpression',
    'System.Linq.Expressions.Expression`1',
    'Dynamic/non-literal Compile(bool) sites left untouched',
    'CaptureStrongNameState(module)',
    'StrongNameSigned && !strongName.HasPublicKey',
    'module.Attributes &= ~ModuleAttributes.StrongNameSigned',
    'VerifyPreparedStrongNameState',
    'Strong-name public key/token/full assembly identity preserved across every rewritten output: YES',
    'strongNameSignedFlagsCleared != discovery.StrongNameSignedTargetAssemblies',
    'Receipt-backed source strong-name state + prepared public keys/tokens/full identities/signature dispositions reverified: YES',
    'GetParameterlessInsertionHazard',
    'branch/exception-handler entry point',
    'crossing short branch would change displacement',
    'OperandType.ShortInlineBrTarget',
    'SetInt32ConstantToOnePreservingEncoding',
    'case Code.Ldc_I4_0:',
    'case Code.Ldc_I4_S:',
    'case Code.Ldc_I4:',
    'CreatePreferInterpretationOverload',
    'new ParameterDefinition(module.TypeSystem.Boolean)',
    'il.InsertBefore(instruction, il.Create(OpCodes.Ldc_I4_1))',
    'module.Write(temporaryPath, new WriterParameters { WriteSymbols = false })',
    'ReadModuleWithWorkspaceResolver(preparedPath, resolver, ReadingMode.Deferred)',
    'MetadataEquivalentTo(afterFingerprint)',
    'after.TotalDirectCompileSites != before.TotalDirectCompileSites',
    'afterFingerprint.InstructionCount != checked(beforeFingerprint.InstructionCount + rewrittenParameterless)',
    'Original managed install no longer matches receipt SHA-1 after Step 19',
    'Step 19 changed a non-target prepared file',
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
        raise SystemExit(f'ERROR: Step 19 compatibility marker missing: {marker}')

# Every game-facing Cecil module open is centralized through the bound helper or the
# workspace identity resolver. The catalog probe is separately hard-rejected from resolving.
if core.count('ModuleDefinition.ReadModule(') != 3:
    raise SystemExit('ERROR: Step 19 expected exactly three direct Cecil ReadModule call sites (bound read helper + workspace dependency open + rejecting catalog probe).')
for forbidden in (
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
    'module.Write(sourcePath',
    'module.Write(installPath',
    'StrongNameKeyPair',
    'StrongNameKeyBlob',
    'StrongNameKeyContainer',
):
    if forbidden in core:
        raise SystemExit(f'ERROR: Step 19 gained forbidden runtime/network/live-install behavior: {forbidden}')

# Prepared outputs may differ; source and live install are only read/copied/hashed.
for marker in (
    'var preparedRoot = Path.Combine(_workRoot, PreparedRootName);',
    'var sourcePath = ResolveChildPath(workspace.SourceRoot, relative);',
    'var installPath = ResolveChildPath(workspace.ManagedRoot, relative);',
    'File.Copy(sourcePath, destinationPath, overwrite: false);',
    'var temporaryPath = outputPath + ".step19tmp";',
):
    if marker not in core:
        raise SystemExit(f'ERROR: Step 19 launcher-private isolation marker missing: {marker}')

seq = Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibilityGateSequence.cs').read_text()
summary = Path('src/StS2Launcher.Core/ExpressionInterpreterCompatibilitySummary.cs').read_text()
for marker in (
    'var expected = (ExpressionInterpreterCompatibilityGate)(_results.Count + 1);',
    'Cannot advance after the first failed expression-interpreter compatibility gate.',
    '_results.Count == 4',
    'EXPRESSION INTERPRETER COMPATIBILITY PASS — {PassedGates}/4',
):
    if marker not in seq + summary:
        raise SystemExit(f'ERROR: Step 19 ordered-gate contract missing: {marker}')

testproj = Path('tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj').read_text()
if '<PackageReference Include="Mono.Cecil" Version="0.11.6" />' not in testproj:
    raise SystemExit('ERROR: Step 19 host tests must keep the direct Mono.Cecil 0.11.6 pin.')

tests = Path('tests/StS2Launcher.Core.Tests/ExpressionInterpreterCompatibilityTests.cs').read_text()
for marker in (
    'OrderedExpressionInterpreterGatesReachFourOfFourPass',
    'ExpressionInterpreterGatesStopAfterFirstFailure',
    'RealWorkspaceExpressionCompileCallsAreForcedToInterpretationAndInstallStaysUntouched',
    'NoSupportedExpressionCompileTargetFailsAtGateBWithoutPreparedOutput',
    'GenericParameterless',
    'LiteralFalseShort',
    'LiteralFalseLong',
    'UnsafeBranchTargetParameterless',
    'CrossingShortBranchParameterless',
    'OpCodes.Ldc_I4_S, (sbyte)0',
    'OpCodes.Ldc_I4, 0',
    'Parameterless sites skipped for branch/EH/prefix safety: 2',
    'Eligible supported sites selected: 5',
    'StrongNameIdentityTargetIsRewrittenWithPublicKeyIdentityPreservedAndSignedFlagCleared',
    'Selected assemblies with StrongNameSigned set: 1',
    'Modified assemblies with StrongNameSigned cleared in prepared copy: 1',
    'Consumer strong-name reference no longer matches the preserved prepared target identity.',
    'Total real call sites rewritten: 5',
    'Original Step 12 install unchanged: YES',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 19 host-test marker missing: {marker}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 19.1 — EXPRESSION INTERPRETER COMPATIBILITY',
    'Version 0.0.53',
    'Steps 01–18 are complete on the physical iPhone.',
    'Step 19.1 — Expression Interpreter Compatibility (ordered gates A–D)',
    'Run Gates A–D — Interpreter Probe → Real Compile Targets → Rewrite → Isolation Audit',
    'RunExpressionInterpreterCompatibilityAsync',
    '_expressionInterpreterCompatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync',
    '_expressionInterpreterCompatibility.RunRealCompileTargetDiscovery',
    '_expressionInterpreterCompatibility.RunPreferInterpretationRewrite',
    '_expressionInterpreterCompatibility.RunIsolationAuditAsync',
    'PASS: STEP 19.1 EXPRESSION INTERPRETER COMPATIBILITY — 4/4',
    'Step 18 remains closed/protected',
    'Run Foundation 5/5 Regression',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 19 UI/gate marker missing: {marker}')

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
        raise SystemExit(f'ERROR: Step 19 build-wrapper marker missing: {marker}')

verify = Path('scripts/verify-step19-ipa.sh').read_text()
for marker in (
    '0.0.53',
    'BUILD_VERSION" == "53"',
    'Step16Fixtures/StS2Launcher.Step16.Fixture.dll',
    'cmp -s "$FIXTURE_SOURCE" "$FIXTURE"',
    'Real StS2/proprietary payload in IPA: none',
    'DiskArbitration',
    'AudioUnit.framework',
    'Expected device UI: STEP 19.1 — EXPRESSION INTERPRETER COMPATIBILITY',
):
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 19 IPA verification marker missing: {marker}')

run_tests = Path('scripts/run-unit-tests-step19.sh').read_text()
for marker in (
    'dotnet test "$TEST_PROJECT"',
    'LogFileName=step19.trx',
    'step19-unit-tests.log',
):
    if marker not in run_tests:
        raise SystemExit(f'ERROR: Step 19 host-test runner marker missing: {marker}')

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
        raise SystemExit(f'ERROR: Step 19 Codemagic-build marker missing: {marker}')

codemagic = Path('codemagic.yaml').read_text()
for marker in (
    'ios-step-19-1:',
    'Step 19.1 - Strong-Name Identity Expression Compatibility',
    'max_build_duration: 120',
    '$HOME/.cache/sts2launcher/godot-step15',
    'bash scripts/codemagic-build-step19.sh',
    'artifacts/StS2-Launcher-Step-19.ipa',
    'artifacts/step19-build-summary.txt',
):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Step 19 Codemagic workflow marker missing: {marker}')

# Source archives must not ship game/proprietary payloads. Historical docs may mention
# names, so inspect file names rather than arbitrary text.
for path in Path('.').rglob('*'):
    if not path.is_file():
        continue
    normalized = str(path).replace('\\', '/').lower()
    name = path.name.lower()
    if name == 'sts2.dll' or 'slaythespire2.app/' in normalized or name.startswith('libfmod') or 'spine_godot' in name:
        raise SystemExit(f'ERROR: Step 19 source archive contains forbidden game/proprietary payload: {path}')

print('Step 19.1 Expression Interpreter Compatibility source validation: PASS')
print('  Steps 01-18 regression guards retained; critical Step 17/18 implementation hashes unchanged')
print('  Gate A: captured-expression interpreter proof + fresh receipt-backed arm64/shared workspace')
print('  Gate B: real direct Compile call-site discovery + strong-name identity/signature-state classification')
print('  Gate C: structurally-safe sites -> preferInterpretation=true; modified strong-name copies preserve public-key identity and clear only StrongNameSigned')
print('  Gate D: complete source/prepared/live-install SHA-1 isolation audit; only selected prepared assemblies may differ')
print('  Dynamic Compile(bool), malformed strong-name/control-flow-sensitive sites, game execution, Harmony/MonoMod, broad Reflection.Emit replacement, FMOD/Spine runtime integration, Cloud and Workshop remain out of scope')
PY
