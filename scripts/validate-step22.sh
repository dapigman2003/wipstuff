#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Preserve every physically closed boundary through Step 20 with the existing parent chain.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step20.sh

python3 - <<'PY'
from pathlib import Path
import hashlib, plistlib, re

required = [
    Path('src/StS2Launcher.Core/HostFrameworkClosureRootSet.cs'),
    Path('src/StS2Launcher.Core/HostFrameworkClosureFoundation.cs'),
    Path('src/StS2Launcher.Core/HostFrameworkClosureGate.cs'),
    Path('tests/StS2Launcher.Core.Tests/HostFrameworkClosureFoundationTests.cs'),
    Path('scripts/build-step22.sh'),
    Path('scripts/run-unit-tests-step22.sh'),
    Path('scripts/codemagic-build-step22.sh'),
    Path('scripts/verify-step22-ipa.sh'),
    Path('docs/STEP-22-HOST-FRAMEWORK-CLOSURE.md'),
    Path('docs/STEP-22-TEST.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 22 artifact missing: {path}')

# The physically passed Step 21 planner and the Step 21.1 exporter remain unchanged. Step 22
# deliberately changes host build roots around them rather than weakening their binding policy.
protected = {
    Path('src/StS2Launcher.Core/PreparedRuntimeFrameworkBinding.cs'): '8c878149a5aa71f5d225261124f4452ecbf5c051e9de549369d123ed175efe37',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingPlan.cs'): '9b3144fcb87b5465b3b238c367168d8f45475673558edbca6576cfe862c00fe5',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGate.cs'): 'e5e06a95e92fa70e4ecbf193bb113cb678f6acb320881b81249da830f9383f8a',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGateResult.cs'): '450d12fa837eb857c3db18de6057b65fd2a0de83b61726ff8562f148686b920e',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingGateSequence.cs'): '1a3f082f3341a864021a08bffd86722786a27c393ed46ea0b0aa549666444a12',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingProgress.cs'): '31e5b0bf826796c024eee8d6fd7cf02827d41a977a1121230bb20afca95e71db',
    Path('src/StS2Launcher.Core/RuntimeFrameworkBindingSummary.cs'): '19d61ebe55251f3d656d817bdba41e1d38e46cee74ed50c665ba77d7e296b95e',
    Path('src/StS2Launcher.Core/RuntimeBindingDiagnosticsExporter.cs'): '5cb627d536defd25e2730b23ef87af391f60596acaf9b12cbf285527f9d18882',
    Path('tests/StS2Launcher.Core.Tests/PreparedRuntimeFrameworkBindingTests.cs'): '8d2ff599d5ee698850c202d3900cbfac892f6500f61f18fda137295f8bedb6b6',
    Path('tests/StS2Launcher.Core.Tests/RuntimeBindingDiagnosticsExporterTests.cs'): 'fa23d0bffbaec63ef26d6d80da5db637d8934a72f3a82324800525ab82e4107c',
}
for path, expected in protected.items():
    actual = hashlib.sha256(path.read_bytes()).hexdigest()
    if actual != expected:
        raise SystemExit(f'ERROR: physically proven Step 21/21.1 boundary changed in Step 22: {path}\nexpected {expected}\nactual   {actual}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if plist.get('CFBundleShortVersionString') != '0.0.58' or str(plist.get('CFBundleVersion')) != '58':
    raise SystemExit('ERROR: Step 22 must be version 0.0.58 (58).')
if plist.get('UIFileSharingEnabled') is not True or plist.get('LSSupportsOpeningDocumentsInPlace') is not True:
    raise SystemExit('ERROR: Step 22 must retain Step 21.1 Files diagnostic access.')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
for marker in (
    '<ApplicationVersion>58</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.58</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<MtouchInterpreter>-all</MtouchInterpreter>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
    '<ForceLoad>false</ForceLoad>',
    '<SmartLink>false</SmartLink>',
    'STEP22 INTERPRETER POLICY: MtouchInterpreter=$(MtouchInterpreter); UseInterpreter=$(UseInterpreter); PublishAot=$(PublishAot)',
):
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 22 iOS/regression marker missing: {marker}')
if '<UseInterpreter>true</UseInterpreter>' in csproj or '<PublishAot>true</PublishAot>' in csproj:
    raise SystemExit('ERROR: Step 22 must preserve the narrow Mono interpreter/AOT policy.')

expected_seeds = [
'netstandard','System.Data.Common','System.Diagnostics.Contracts','System.Diagnostics.StackTrace','System.Diagnostics.TraceSource','System.Diagnostics.Tracing','System.IO.FileSystem.DriveInfo','System.IO.MemoryMappedFiles','System.Net.Ping','System.Net.Quic','System.Numerics.Vectors','System.Reflection.Metadata','System.Runtime.CompilerServices.Unsafe','System.Runtime.Loader','System.Runtime.Serialization.Json','System.Runtime.Serialization.Primitives','System.Runtime.Serialization.Xml','System.Threading.Tasks.Parallel','System.Threading.ThreadPool','System.Xml.XDocument','System.Xml.XmlSerializer','System.Xml.XPath']
for name in expected_seeds:
    marker=f'<TrimmerRootAssembly Include="{name}" />'
    if csproj.count(marker) != 1:
        raise SystemExit(f'ERROR: Step 22 measured root missing/duplicated: {name}')

rootset = Path('src/StS2Launcher.Core/HostFrameworkClosureRootSet.cs').read_text()
if rootset.count('new("') != 44:
    raise SystemExit('ERROR: Step 22 expected host closure must contain exactly 44 measured framework simple names.')
for name in expected_seeds:
    if f'"{name}",' not in rootset:
        raise SystemExit(f'ERROR: Step 22 direct seed not represented in root-set source: {name}')
for required_name in ('System.Private.DataContractSerialization','System.Private.Xml.Linq','System.Net.HttpListener','System.Transactions.Local'):
    if f'new("{required_name}"' not in rootset:
        raise SystemExit(f'ERROR: Step 22 transitive host-closure identity missing: {required_name}')

foundation = Path('src/StS2Launcher.Core/HostFrameworkClosureFoundation.cs').read_text()
for marker in (
    'AssemblyLoadContext.Default.LoadFromAssemblyName(requested)',
    'Gate B is intentionally diagnostic',
    'Runtime closure ready for first real CLR load: YES',
    'Prepared System.*/netstandard framework assemblies: 0',
    'RunRuntimePayloadClassificationAsync',
    'RunHostFrameworkBindingPlan()',
    'RunPreparedRuntimeAssemblySetAsync',
    'RunClosureAuditAsync',
    'EnsureNoStS2AssemblyLoaded()',
):
    if marker not in foundation:
        raise SystemExit(f'ERROR: Step 22 closure invariant missing: {marker}')
for forbidden in ('.Write(', 'Assembly.LoadFile(', 'Assembly.LoadFrom(', 'LoadFromStream('):
    if forbidden in foundation:
        raise SystemExit(f'ERROR: Step 22 closure wrapper must not write/load private game assemblies directly: {forbidden}')

ui = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 22 — HOST FRAMEWORK CLOSURE FOUNDATION', 'Version 0.0.58',
    'Run Step 22 A–D — Root Host BCL → Recompute Closure → Prepare Host-Bound Set → Audit',
    'RunHostFrameworkClosureFoundationAsync',
    'HOST FRAMEWORK CLOSURE FOUNDATION PASS — 4/4',
):
    if marker not in ui and marker != 'HOST FRAMEWORK CLOSURE FOUNDATION PASS — 4/4':
        raise SystemExit(f'ERROR: Step 22 UI marker missing: {marker}')
if 'Export Current Runtime Binding Diagnostics to Files' not in ui:
    raise SystemExit('ERROR: Step 22 must retain convenient Files export for any residual blocker plan.')

yaml = Path('codemagic.yaml').read_text()
for marker in ('ios-step-22:', 'Step 22 - Host Framework Closure Foundation', 'bash scripts/codemagic-build-step22.sh', 'artifacts/StS2-Launcher-Step-22.ipa'):
    if marker not in yaml:
        raise SystemExit(f'ERROR: Step 22 Codemagic marker missing: {marker}')

# No game/proprietary payloads or user-exported diagnostics in source.
for path in Path('.').rglob('*'):
    if not path.is_file():
        continue
    n=str(path).replace('\\','/').lower(); name=path.name.lower()
    if name == 'sts2.dll' or 'slaythespire2.app/' in n or name.startswith('libfmod') or 'spine_godot' in name:
        raise SystemExit(f'ERROR: Step 22 source contains forbidden game/proprietary payload: {path}')
    if name == 'step21.1-runtimebindingdiagnostics.txt'.lower():
        raise SystemExit(f'ERROR: source archive must not contain exported user diagnostics: {path}')

print('Step 22 Host Framework Closure Foundation source validation: PASS')
print('  Steps 01-20 parent regression chain retained')
print('  Physically proven Step 21 planner + Step 21.1 exporter hash-protected unchanged')
print('  22 measured TrimmerRootAssembly seed roots retained under TrimMode=full + MtouchInterpreter=-all')
print('  Physical Gate A requires complete 44-name Step 21.1 framework frontier from iOS host')
print('  Gates B-D reuse the proven Step 21 planner/preparer/auditor and require zero blockers + no private BCL')
print('  No StS2 CLR load/execution or Cecil write added')
PY
