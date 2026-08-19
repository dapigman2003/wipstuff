#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step20.sh

python3 - <<'PY'
from pathlib import Path
import hashlib, plistlib, re

required = [
    Path('src/StS2Launcher.Core/HostFrameworkClosureRootSet.cs'),
    Path('src/StS2Launcher.Core/HostFrameworkClosureFoundation.cs'),
    Path('src/StS2Launcher.Core/HostFrameworkClosureGate.cs'),
    Path('tests/StS2Launcher.Core.Tests/HostFrameworkClosureFoundationTests.cs'),
    Path('scripts/build-step22-2.sh'),
    Path('scripts/run-unit-tests-step22-2.sh'),
    Path('scripts/codemagic-build-step22-2.sh'),
    Path('scripts/verify-step22-2-ipa.sh'),
    Path('docs/STEP-22.2-HOST-BINDING-FRONTIER-CORRECTION.md'),
    Path('docs/STEP-22.2-TEST.md'),
]
for path in required:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 22.2 artifact missing: {path}')

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
    Path('src/StS2Launcher.Core/HostFrameworkClosureGate.cs'): '061977397924b2bb4f5c150ce5beefe6a94ce1892aebdf889596079a2eb0fc57',
}
for path, expected in protected.items():
    actual = hashlib.sha256(path.read_bytes()).hexdigest()
    if actual != expected:
        raise SystemExit(f'ERROR: proven Step 21/closure-gate boundary changed unexpectedly: {path}\nexpected {expected}\nactual   {actual}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if plist.get('CFBundleShortVersionString') != '0.0.60' or str(plist.get('CFBundleVersion')) != '60':
    raise SystemExit('ERROR: Step 22.2 must be version 0.0.60 (60).')
if plist.get('UIFileSharingEnabled') is not True or plist.get('LSSupportsOpeningDocumentsInPlace') is not True:
    raise SystemExit('ERROR: Step 22.2 must retain Files diagnostic access.')

expected_roots = [
'netstandard','System.Data.Common','System.Diagnostics.Contracts','System.Diagnostics.StackTrace','System.Diagnostics.TraceSource','System.Diagnostics.Tracing','System.IO.FileSystem.DriveInfo','System.IO.MemoryMappedFiles','System.Net.Ping','System.Net.Quic','System.Numerics.Vectors','System.Reflection.Metadata','System.Runtime.CompilerServices.Unsafe','System.Runtime.Loader','System.Runtime.Serialization.Json','System.Runtime.Serialization.Primitives','System.Runtime.Serialization.Xml','System.Threading.Tasks.Parallel','System.Threading.ThreadPool','System.Xml.XDocument','System.Xml.XmlSerializer','System.Xml.XPath']
rootset = Path('src/StS2Launcher.Core/HostFrameworkClosureRootSet.cs').read_text()
roots_block = rootset.split('DirectTrimmerRoots',1)[1].split('// Complete 44-name',1)[0]
direct = re.findall(r'^\s*"([^"]+)",\s*$', roots_block, re.M)
if direct != expected_roots:
    raise SystemExit(f'ERROR: Step 22.2 must preserve the physically proven 22-root list exactly.\nexpected={expected_roots}\nactual={direct}')
expected_frontier = re.findall(r'new\("([^"]+)"', rootset)
if len(expected_frontier) != 44 or len({x.lower() for x in expected_frontier}) != 44:
    raise SystemExit('ERROR: Step 22.2 diagnostic frontier must retain exactly 44 unique measured framework identities.')
if not set(x.lower() for x in direct).issubset(set(x.lower() for x in expected_frontier)):
    raise SystemExit('ERROR: every required direct root must also exist in the 44-name diagnostic frontier.')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
for marker in (
    '<ApplicationVersion>60</ApplicationVersion>', '<ApplicationDisplayVersion>0.0.60</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>', '<MtouchInterpreter>-all</MtouchInterpreter>',
    '<TrimmerRootAssembly Include="SteamKit2" />', '<TrimmerRootAssembly Include="protobuf-net" />', '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />', '<ForceLoad>false</ForceLoad>', '<SmartLink>false</SmartLink>',
    'STEP22 INTERPRETER POLICY: MtouchInterpreter=$(MtouchInterpreter); UseInterpreter=$(UseInterpreter); PublishAot=$(PublishAot)',
):
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 22.2 iOS/regression marker missing: {marker}')
if '<UseInterpreter>true</UseInterpreter>' in csproj or '<PublishAot>true</PublishAot>' in csproj:
    raise SystemExit('ERROR: Step 22.2 must preserve the narrow Mono interpreter/AOT policy.')
for name in expected_roots:
    if csproj.count(f'<TrimmerRootAssembly Include="{name}" />') != 1:
        raise SystemExit(f'ERROR: physically proven Step 22 root missing/duplicated: {name}')
framework_roots = re.findall(r'<TrimmerRootAssembly Include="((?:System\.[^"]+)|netstandard)"\s*/>', csproj)
if framework_roots != expected_roots:
    raise SystemExit(f'ERROR: Step 22.2 must not add speculative framework roots. actual={framework_roots}')

foundation = Path('src/StS2Launcher.Core/HostFrameworkClosureFoundation.cs').read_text()
for marker in (
    'Step22.2-HostBindingFrontierDiagnostics.txt',
    'var direct = observations.Where(item => item.DirectRoot).ToArray();',
    'if (directFailed != 0)',
    'Required host-binding frontier roots:',
    'transitive-only diagnostic misses',
    'Gate A rule: only the 22 measured direct host-binding roots are required.',
    'Gate B recomputes the real sts2.dll dependency plan and is authoritative for any residual binding blockers.',
    'AssemblyLoadContext.Default.LoadFromAssemblyName(requested)',
    'RunRuntimePayloadClassificationAsync', 'RunHostFrameworkBindingPlan()', 'RunPreparedRuntimeAssemblySetAsync', 'RunClosureAuditAsync',
    'Runtime closure ready for first real CLR load: YES', 'Prepared System.*/netstandard framework assemblies: 0',
    'EnsureNoStS2AssemblyLoaded()',
):
    if marker not in foundation:
        raise SystemExit(f'ERROR: Step 22.2 binding-frontier invariant missing: {marker}')
# Ensure the old 44/44 blocking criterion is gone.
for forbidden_marker in ('if (failed != 0)', 'Host framework closure incomplete:'):
    if forbidden_marker in foundation:
        raise SystemExit(f'ERROR: Step 22.2 still contains the obsolete full-44 blocking Gate A rule: {forbidden_marker}')
for forbidden in ('.Write(', 'Assembly.LoadFile(', 'Assembly.LoadFrom(', 'LoadFromStream('):
    if forbidden in foundation:
        raise SystemExit(f'ERROR: Step 22.2 wrapper must not write/load private game assemblies directly: {forbidden}')

tests = Path('tests/StS2Launcher.Core.Tests/HostFrameworkClosureFoundationTests.cs').read_text()
for marker in ('Assert.AreEqual(22, HostFrameworkClosureRootSet.DirectTrimmerRoots.Count);','Assert.AreEqual(44, HostFrameworkClosureRootSet.ExpectedHostClosure.Count);','DirectRootSeeds_AreLoadableOnNet9Host','Direct host-binding root is missing from the measured diagnostic frontier'):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 22.2 host-test invariant missing: {marker}')

ui = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 22.2 — HOST BINDING FRONTIER CORRECTION','Version 0.0.60',
    'Run Step 22.2 A–D — Qualify 22 Roots → Recompute Closure → Prepare Host-Bound Set → Audit',
    '22/22 required direct roots passed','transitive-only','Gate B then recomputes the real sts2.dll graph',
    'Export Current Runtime Binding Diagnostics to Files',
):
    if marker not in ui:
        raise SystemExit(f'ERROR: Step 22.2 UI marker missing: {marker}')

yaml = Path('codemagic.yaml').read_text()
for marker in ('ios-step-22-2:','Step 22.2 - Host Binding Frontier Correction','bash scripts/codemagic-build-step22-2.sh','artifacts/StS2-Launcher-Step-22.2.ipa'):
    if marker not in yaml:
        raise SystemExit(f'ERROR: Step 22.2 Codemagic marker missing: {marker}')

for path in Path('.').rglob('*'):
    if not path.is_file():
        continue
    n=str(path).replace('\\','/').lower(); name=path.name.lower()
    if name == 'sts2.dll' or 'slaythespire2.app/' in n or name.startswith('libfmod') or 'spine_godot' in name:
        raise SystemExit(f'ERROR: Step 22.2 source contains forbidden game/proprietary payload: {path}')
    if name in {'step21.1-runtimebindingdiagnostics.txt','step22.1-hostframeworkavailabilitydiagnostics.txt','step22.2-hostbindingfrontierdiagnostics.txt'}:
        raise SystemExit(f'ERROR: source archive must not contain exported user diagnostics: {path}')

print('Step 22.2 Host Binding Frontier Correction source validation: PASS')
print('  Steps 01-20 parent regression chain retained')
print('  Physically proven Step 21 planner + Step 21.1 exporter hash-protected unchanged')
print('  Exact physically proven 22-root host-binding set preserved; no speculative framework roots added')
print('  Full 44-name probe retained for diagnostics, but only 22 direct roots gate Gate A')
print('  Gate B recomputed real sts2.dll plan is authoritative for residual blockers')
print('  Gates C-D still require zero blockers + no private framework implementations')
print('  No StS2 CLR load/execution or Cecil write added')
PY
