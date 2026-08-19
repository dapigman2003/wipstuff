#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Preserve every physically closed boundary through Step 20 using the existing
# parent-mode regression chain. Step 21 itself is protected below by exact hashes
# because 21.1 is reporting/export only and must not change its binding logic.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step20.sh

python3 - <<'PY'
from pathlib import Path
import hashlib
import os
import plistlib
import re

required = [
    Path('src/StS2Launcher.Core/RuntimeBindingDiagnosticsExporter.cs'),
    Path('tests/StS2Launcher.Core.Tests/RuntimeBindingDiagnosticsExporterTests.cs'),
    Path('scripts/build-step21-1.sh'),
    Path('scripts/run-unit-tests-step21-1.sh'),
    Path('scripts/codemagic-build-step21-1.sh'),
    Path('scripts/verify-step21-1-ipa.sh'),
    Path('docs/STEP-21.1-DIAGNOSTIC-EXPORT.md'),
    Path('docs/STEP-21.1-TEST.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 21.1 artifact missing: {path}')

# The physically passed Step 21 binding subsystem must remain byte-for-byte unchanged.
protected_step21 = {
    Path('src/StS2Launcher.Core/PreparedRuntimeFrameworkBinding.cs'):
        '8c878149a5aa71f5d225261124f4452ecbf5c051e9de549369d123ed175efe37',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingPlan.cs'):
        '9b3144fcb87b5465b3b238c367168d8f45475673558edbca6576cfe862c00fe5',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGate.cs'):
        'e5e06a95e92fa70e4ecbf193bb113cb678f6acb320881b81249da830f9383f8a',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGateResult.cs'):
        '450d12fa837eb857c3db18de6057b65fd2a0de83b61726ff8562f148686b920e',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGateSequence.cs'):
        '1a3f082f3341a864021a08bffd86722786a27c393ed46ea0b0aa549666444a12',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingProgress.cs'):
        '31e5b0bf826796c024eee8d6fd7cf02827d41a977a1121230bb20afca95e71db',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingSummary.cs'):
        '19d61ebe55251f3d656d817bdba41e1d38e46cee74ed50c665ba77d7e296b95e',
    Path('tests/StS2Launcher.Core.Tests/PreparedRuntimeFrameworkBindingTests.cs'):
        '8d2ff599d5ee698850c202d3900cbfac892f6500f61f18fda137295f8bedb6b6',
}
for path, expected in protected_step21.items():
    actual = hashlib.sha256(path.read_bytes()).hexdigest()
    if actual != expected:
        raise SystemExit(f'ERROR: physically passed Step 21 implementation changed in reporting-only 21.1: {path}\nexpected {expected}\nactual   {actual}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if plist.get('CFBundleShortVersionString') != '0.0.57' or str(plist.get('CFBundleVersion')) != '57':
    raise SystemExit('ERROR: Step 21.1 must be version 0.0.57 (57).')
if plist.get('UIFileSharingEnabled') is not True:
    raise SystemExit('ERROR: Step 21.1 must set UIFileSharingEnabled=true so Documents can be accessed through iOS file sharing.')
if plist.get('LSSupportsOpeningDocumentsInPlace') is not True:
    raise SystemExit('ERROR: Step 21.1 must set LSSupportsOpeningDocumentsInPlace=true so Documents appear in Files/document browser.')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
for marker in (
    '<ApplicationVersion>57</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.57</ApplicationDisplayVersion>',
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
        raise SystemExit(f'ERROR: Step 21.1 iOS/regression marker missing: {marker}')
for forbidden in ('<UseInterpreter>true</UseInterpreter>', '<PublishAot>true</PublishAot>'):
    if forbidden in csproj:
        raise SystemExit(f'ERROR: Step 21.1 broadened interpreter/AOT policy: {forbidden}')

exporter = Path('src/StS2Launcher.Core/RuntimeBindingDiagnosticsExporter.cs').read_text()
for marker in (
    'public sealed class RuntimeBindingDiagnosticsExporter',
    'ReportFileName = "Step21.1-RuntimeBindingDiagnostics.txt"',
    'PreparedRuntimeFrameworkBinding.WorkRootName',
    'PreparedRuntimeFrameworkBinding.PlanFileName',
    'RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument',
    'BLOCKERS BY KIND',
    'UNIQUE BLOCKED REQUESTS',
    'BLOCKERS — COMPLETE',
    'EDGE COUNTS BY BINDING KIND',
    'HOST FRAMEWORK BINDINGS',
    'PREPARED IL-ONLY ASSEMBLIES',
    'Steam credentials, refresh tokens, Steam Guard material, or Apple signing secrets',
    'File.Move(tempPath, reportPath, overwrite: true)',
    'RuntimeClosureReady != (plan.Blockers.Length == 0)',
):
    if marker not in exporter:
        raise SystemExit(f'ERROR: Step 21.1 diagnostic exporter marker missing: {marker}')

# Exporter must be output-only: no Steam/network/runtime loading/Cecil writes and no
# modification of the persisted plan or prepared/source/install trees.
for forbidden in (
    'SteamSessionStore', 'SteamClient', 'HttpClient', 'ClientWebSocket',
    'AssemblyLoadContext', 'Assembly.Load', 'LoadFromStream(', 'LoadFromAssemblyPath(',
    'Mono.Cecil', 'ModuleDefinition', '.Write(',
    'File.Delete(planPath)', 'File.Move(planPath', 'File.Copy(planPath',
):
    if forbidden in exporter:
        raise SystemExit(f'ERROR: Step 21.1 exporter gained forbidden non-reporting behavior: {forbidden}')
if 'ActualLocation' in exporter:
    raise SystemExit('ERROR: Step 21.1 shareable report must not emit host absolute assembly locations.')

# Report is deliberately one text file directly under Documents/StS2Launcher.
if 'Path.Combine(_launcherDataRoot, ReportFileName)' not in exporter:
    raise SystemExit('ERROR: Step 21.1 report is not rooted directly under launcher Documents/StS2Launcher.')

tests = Path('tests/StS2Launcher.Core.Tests/RuntimeBindingDiagnosticsExporterTests.cs').read_text()
for marker in (
    'ExporterGroupsAndListsEveryPersistedBlockerInShareSafeText',
    'ExporterCanReadExistingStep21PlanWithoutRerunningGates',
    'ExporterRejectsMissingPersistedPlanInsteadOfCreatingMisleadingReport',
    'ExporterRejectsInconsistentRuntimeClosureFlag',
    'Shareable report must not leak absolute app/sandbox paths',
    'Runtime closure ready for first real CLR load: NO',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 21.1 host-test marker missing: {marker}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 21.1 — BINDING DIAGNOSTIC EXPORT',
    'Version 0.0.57',
    'Step 21 physically passed A–D',
    'Export Complete Step 21 Binding Diagnostics to Files',
    'RunRuntimeBindingDiagnosticsExportAsync',
    '_runtimeBindingDiagnosticsExporter.ExportAsync',
    'On My iPhone → StS2 Launcher → StS2Launcher →',
    'existing Step 21 plan may be exported immediately',
    '47 explicit binding blockers',
    'No real StS2 CLR load should be attempted yet',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 21.1 UI/export marker missing: {marker}')

# The file-sharing hotfix must not add a document picker/File Provider dependency or
# runtime import/export path. The simple Apple Documents sharing keys are the boundary.
for forbidden in ('UIDocumentPickerViewController', 'NSFileProvider', 'UIActivityViewController'):
    if forbidden in root:
        raise SystemExit(f'ERROR: Step 21.1 unexpectedly added a broader document/file-provider UI dependency: {forbidden}')

for path in (
    Path('scripts/build-step21-1.sh'),
    Path('scripts/run-unit-tests-step21-1.sh'),
    Path('scripts/codemagic-build-step21-1.sh'),
    Path('scripts/verify-step21-1-ipa.sh'),
):
    if path.stat().st_size == 0:
        raise SystemExit(f'ERROR: empty Step 21.1 script: {path}')

build = Path('scripts/build-step21-1.sh').read_text()
for marker in ('validate-step21-1.sh', 'StS2-Launcher-Step-21.1.ipa', 'STEP21.1 INTERPRETER POLICY: MtouchInterpreter=-all'):
    if marker not in build:
        raise SystemExit(f'ERROR: Step 21.1 build marker missing: {marker}')

run_tests = Path('scripts/run-unit-tests-step21-1.sh').read_text()
for marker in ('dotnet test "$TEST_PROJECT"', 'LogFileName=step21-1.trx', 'step21-1-unit-tests.log'):
    if marker not in run_tests:
        raise SystemExit(f'ERROR: Step 21.1 unit-test runner marker missing: {marker}')

cm = Path('scripts/codemagic-build-step21-1.sh').read_text()
for marker in (
    'bash scripts/validate-step21-1.sh',
    'bash scripts/run-unit-tests-step21-1.sh',
    'bash scripts/build-step21-1.sh',
    'bash scripts/verify-step21-1-ipa.sh artifacts/StS2-Launcher-Step-21.1.ipa',
    'artifacts/step21-1-build-summary.txt',
):
    if marker not in cm:
        raise SystemExit(f'ERROR: Step 21.1 Codemagic-build marker missing: {marker}')

verify = Path('scripts/verify-step21-1-ipa.sh').read_text()
for marker in (
    '0.0.57', 'BUILD_VERSION" == "57"',
    'UIFileSharingEnabled', 'LSSupportsOpeningDocumentsInPlace',
    'Expected device UI: STEP 21.1 — BINDING DIAGNOSTIC EXPORT',
):
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 21.1 IPA verification marker missing: {marker}')

codemagic = Path('codemagic.yaml').read_text()
for marker in (
    'ios-step-21-1:',
    'Step 21.1 - Binding Diagnostic Export',
    'bash scripts/codemagic-build-step21-1.sh',
    'artifacts/StS2-Launcher-Step-21.1.ipa',
    'artifacts/step21-1-build-summary.txt',
):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Step 21.1 Codemagic workflow marker missing: {marker}')

# Repository source must never contain game/proprietary payloads or exported user diagnostics.
for path in Path('.').rglob('*'):
    if not path.is_file():
        continue
    normalized = str(path).replace('\\', '/').lower()
    name = path.name.lower()
    if name == 'sts2.dll' or 'slaythespire2.app/' in normalized or name.startswith('libfmod') or 'spine_godot' in name:
        raise SystemExit(f'ERROR: Step 21.1 source archive contains forbidden game/proprietary payload: {path}')
    if name == 'step21.1-runtimebindingdiagnostics.txt'.lower():
        raise SystemExit(f'ERROR: source archive must not contain a user-generated runtime binding report: {path}')

print('Step 21.1 Binding Diagnostic Export source validation: PASS')
print('  Steps 01-20 parent regression chain retained')
print('  Physically passed Step 21 binding/preparation implementation + tests hash-protected byte-for-byte')
print('  Diagnostic exporter reads only persisted runtime-binding-plan.json and writes one share-safe UTF-8 text report')
print('  Report contains grouped + unique + complete blockers, host bindings, prepared identities, and plan SHA-256')
print('  UIFileSharingEnabled + LSSupportsOpeningDocumentsInPlace expose Documents through Files')
print('  Existing Step 21 plan can be exported immediately after app update; no A-D rerun required if plan persists')
PY
