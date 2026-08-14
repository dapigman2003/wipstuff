#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

required_files=(
  README.md
  THIRD_PARTY.md
  codemagic.yaml
  global.json
  docs/STEP-05-FINAL-TEST.md
  docs/TESTING.md
  docs/CURRENT-STATUS.md
  src/StS2Launcher.Core/StS2Launcher.Core.csproj
  src/StS2Launcher.Core/LauncherController.cs
  src/StS2Launcher.Core/CoreSelfTest.cs
  src/StS2Launcher.Core/ICredentialStore.cs
  src/StS2Launcher.Core/CredentialStoreVerifier.cs
  src/StS2Launcher.Core/FoundationVerificationResult.cs
  src/StS2Launcher.Core/SteamHttpClientFactory.cs
  src/StS2Launcher.Core/SteamConnectionProbe.cs
  src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj
  src/StS2Launcher.Step05.iOS/Info.plist
  src/StS2Launcher.Step05.iOS/AppDelegate.cs
  src/StS2Launcher.Step05.iOS/SceneDelegate.cs
  src/StS2Launcher.Step05.iOS/RootViewController.cs
  src/StS2Launcher.Step05.iOS/Platform/KeychainCredentialStore.cs
  src/StS2Launcher.Step05.iOS/Platform/KeychainProbe.cs
  tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj
  tests/StS2Launcher.Core.Tests/LauncherControllerTests.cs
  tests/StS2Launcher.Core.Tests/CredentialStoreVerifierTests.cs
  tests/StS2Launcher.Core.Tests/SteamFoundationTests.cs
  tests/StS2Launcher.Core.Tests/FoundationVerificationTests.cs
  tools/StS2Launcher.SteamKitIosPatcher/StS2Launcher.SteamKitIosPatcher.csproj
  tools/StS2Launcher.SteamKitIosPatcher/Program.cs
  scripts/run-unit-tests.sh
  scripts/build-step05-final.sh
  scripts/verify-step05-final-ipa.sh
  scripts/codemagic-build.sh
)

for required in "${required_files[@]}"; do
  [[ -f "$required" ]] || {
    echo "ERROR: missing required final Step 05 file: $required" >&2
    exit 2
  }
done

python3 - <<'PY'
from pathlib import Path
import plistlib
import xml.etree.ElementTree as ET

# Core project: one production dependency, pinned SteamKit 3.4.0.
core_root = ET.parse('src/StS2Launcher.Core/StS2Launcher.Core.csproj').getroot()
core_packages = {
    e.attrib.get('Include'): e.attrib.get('Version')
    for e in core_root.iter('PackageReference')
}
if core_packages != {'SteamKit2': '3.4.0'}:
    raise SystemExit(f'ERROR: Core package set changed: {core_packages}')

# iOS build contract and proven trim/link workarounds.
ios_root = ET.parse('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').getroot()
props = {}
for group in ios_root.findall('PropertyGroup'):
    for child in group:
        props[child.tag] = (child.text or '').strip()
expected = {
    'TargetFramework': 'net9.0-ios',
    'RuntimeIdentifier': 'ios-arm64',
    'SupportedOSPlatformVersion': '18.0',
    'ApplicationId': 'com.community.sts2launcher',
    'ApplicationVersion': '22',
    'ApplicationDisplayVersion': '0.0.22',
    'TrimMode': 'full',
}
for key, value in expected.items():
    if props.get(key) != value:
        raise SystemExit(f'ERROR: iOS property {key} expected {value!r}, got {props.get(key)!r}')

trim_roots = [
    n.attrib.get('Include') for n in ios_root.iter('TrimmerRootAssembly')
    if n.attrib.get('Include')
]
if trim_roots != ['SteamKit2', 'protobuf-net', 'protobuf-net.Core']:
    raise SystemExit(f'ERROR: final trim roots changed: {trim_roots}')

framework_target = next(
    (n for n in ios_root.iter('Target')
     if n.attrib.get('Name') == 'Step052RemoveMacOnlyDiskArbitrationFramework'),
    None)
if framework_target is None:
    raise SystemExit('ERROR: DiskArbitration generated-framework filter missing.')
if framework_target.attrib.get('AfterTargets') != '_LoadLinkerOutput':
    raise SystemExit('ERROR: framework filter AfterTargets changed.')
if framework_target.attrib.get('BeforeTargets') != '_ComputeLinkNativeExecutableInputs':
    raise SystemExit('ERROR: framework filter BeforeTargets changed.')
removes = [
    n.attrib.get('Remove') for n in framework_target.iter('_LinkerFrameworks')
    if n.attrib.get('Remove')
]
if removes != ['DiskArbitration']:
    raise SystemExit(f'ERROR: framework filter must remove only DiskArbitration; got {removes}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if plist.get('CFBundleIdentifier') != 'com.community.sts2launcher':
    raise SystemExit('ERROR: unexpected bundle ID.')
if plist.get('CFBundleShortVersionString') != '0.0.22':
    raise SystemExit('ERROR: display version regression.')
if str(plist.get('CFBundleVersion')) != '22':
    raise SystemExit('ERROR: build version regression.')

# Unit-test project is a real dotnet test project and remains build-only.
test_root = ET.parse('tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj').getroot()
test_props = {}
for group in test_root.findall('PropertyGroup'):
    for child in group:
        test_props[child.tag] = (child.text or '').strip()
if test_props.get('TargetFramework') != 'net9.0' or test_props.get('IsTestProject') != 'true':
    raise SystemExit(f'ERROR: test project properties changed: {test_props}')
test_packages = {
    e.attrib.get('Include'): e.attrib.get('Version')
    for e in test_root.iter('PackageReference')
}
expected_test_packages = {
    'Microsoft.NET.Test.Sdk': '18.8.1',
    'MSTest.TestAdapter': '4.3.2',
    'MSTest.TestFramework': '4.3.2',
}
if test_packages != expected_test_packages:
    raise SystemExit(f'ERROR: unit-test package set changed: {test_packages}')

# Step 01/02 device wiring stays explicit: app entry, scene setup, visible window,
# and lifecycle callbacks. Host unit tests validate the aggregate gate logic;
# physical iPhone remains the runtime authority for UIKit itself.
program = Path('src/StS2Launcher.Step05.iOS/Program.cs').read_text()
app_delegate = Path('src/StS2Launcher.Step05.iOS/AppDelegate.cs').read_text()
scene = Path('src/StS2Launcher.Step05.iOS/SceneDelegate.cs').read_text()
for required in ('UIApplication.Main', 'typeof(AppDelegate)'):
    if required not in program:
        raise SystemExit(f'ERROR: startup marker missing: {required}')
for required in ('FinishedLaunching', 'GetConfiguration', 'DelegateType = typeof(SceneDelegate)'):
    if required not in app_delegate:
        raise SystemExit(f'ERROR: app delegate marker missing: {required}')
for required in (
    'class SceneDelegate : UIWindowSceneDelegate',
    'public override UIWindow? Window',
    'WillConnect',
    'MakeKeyAndVisible',
    'DidBecomeActive',
    'WillResignActive',
    'WillEnterForeground',
    'DidEnterBackground',
):
    if required not in scene:
        raise SystemExit(f'ERROR: scene/lifecycle marker missing: {required}')

# Step 03 Core and Step 04 credential checks are both executable on-device and
# covered by host unit tests.
core_self_test = Path('src/StS2Launcher.Core/CoreSelfTest.cs').read_text()
if 'const int total = 12;' not in core_self_test:
    raise SystemExit('ERROR: Core 12/12 regression self-test changed unexpectedly.')
credential_verifier = Path('src/StS2Launcher.Core/CredentialStoreVerifier.cs').read_text()
for required in ('const int total = 7;', 'store.Set(key, firstValue)', 'store.Set(key, secondValue)', 'finally', 'store.Delete(key)'):
    if required not in credential_verifier:
        raise SystemExit(f'ERROR: credential verifier marker missing: {required}')
keychain = Path('src/StS2Launcher.Step05.iOS/Platform/KeychainCredentialStore.cs').read_text()
for required in ('SecKind.GenericPassword', 'SecAccessible.AfterFirstUnlockThisDeviceOnly', 'SecKeyChain.Add', 'SecKeyChain.Remove'):
    if required not in keychain:
        raise SystemExit(f'ERROR: Keychain implementation marker missing: {required}')

# Step 05 final runtime is intentionally small: WebSocket-only SteamKit using
# the proven CMWebSocket SocketsHttpHandler policy. All temporary network and
# exception-localization probes must be gone.
factory = Path('src/StS2Launcher.Core/SteamHttpClientFactory.cs').read_text()
for required in ('HttpClientPurpose.CMWebSocket', 'new SocketsHttpHandler()', 'new HttpClient()', 'ProductInfoHeaderValue("SteamKit"'):
    if required not in factory:
        raise SystemExit(f'ERROR: Steam HTTP factory marker missing: {required}')
probe = Path('src/StS2Launcher.Core/SteamConnectionProbe.cs').read_text()
for required in (
    'ProtocolTypes.WebSocket',
    'WithHttpClientFactory(Factory)',
    'new SteamClient(configuration)',
    'ConnectedCallback',
    'DisconnectedCallback',
    'steamClient.Connect()',
    'steamClient.Disconnect()',
    'CmWebSocketFactoryUsed',
):
    if required not in probe:
        raise SystemExit(f'ERROR: final Steam probe marker missing: {required}')
for forbidden in (
    'ProtocolTypes.Tcp',
    'DebugLog.',
    'FirstChanceException',
    'IDebugNetworkListener',
    'ClientWebSocket',
    'SteamKitEndpointReplay',
    'CmNetworkProbe',
    'SocketsHandlerIsolationProbe',
):
    if forbidden in probe:
        raise SystemExit(f'ERROR: superseded diagnostic remains in final Steam probe: {forbidden}')

obsolete_files = [
    'src/StS2Launcher.Core/CmNetworkProbe.cs',
    'src/StS2Launcher.Core/CmNetworkProbeResult.cs',
    'src/StS2Launcher.Core/SocketsHandlerIsolationProbe.cs',
    'src/StS2Launcher.Core/SocketsHandlerIsolationProbeResult.cs',
    'src/StS2Launcher.Core/SteamKitEndpointReplayProbe.cs',
    'src/StS2Launcher.Core/SteamKitEndpointReplayProbeResult.cs',
    'src/StS2Launcher.Core/SteamNetworkTraceListener.cs',
]
for file in obsolete_files:
    if Path(file).exists():
        raise SystemExit(f'ERROR: obsolete diagnostic file was retained: {file}')

# Unit-test coverage markers. These tests intentionally avoid live Steam/iOS
# network or Keychain calls; those stay as physical-device integration gates.
test_text = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))
for required in (
    'ExistingCoreRegressionSelfTestStillPassesTwelveOfTwelve',
    'RoundTripPassesAndLeavesStoreClean',
    'CmWebSocketPurposeUsesSocketsHttpHandlerPolicy',
    'ThreeOfThreeResultRequiresAllConnectionGates',
    'AllFiveFoundationGatesProducePass',
    'UiStartupIsRequired',
    'ActiveLifecycleIsRequired',
):
    if required not in test_text:
        raise SystemExit(f'ERROR: unit-test coverage marker missing: {required}')

root_view = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for required in (
    'STEP 05.16 — FOUNDATION FINALIZATION',
    'Version 0.0.22',
    'Run Steps 01–05 Device Verification',
    'CoreSelfTest.Run()',
    '_keychainProbe.RunRoundTrip()',
    '_steamProbe.RunAsync',
    'FoundationVerificationResult',
    'NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN',
):
    if required not in root_view:
        raise SystemExit(f'ERROR: final UI marker missing: {required}')

combined_runtime = '\n'.join(
    p.read_text(errors='ignore')
    for p in list(Path('src/StS2Launcher.Core').glob('*.cs')) +
             list(Path('src/StS2Launcher.Step05.iOS').rglob('*.cs'))
)
for forbidden in (
    'BeginAuthSession',
    'AuthSessionDetails',
    'SteamUser.LogOn',
    'RefreshToken',
    'Password =',
    'TwoFactor',
):
    if forbidden in combined_runtime:
        raise SystemExit(f'ERROR: Step 05 finalization introduced authentication behavior: {forbidden}')

# Build-only patcher remains guarded and does not silently broaden its rewrite.
patcher = Path('tools/StS2Launcher.SteamKitIosPatcher/Program.cs').read_text()
for required in (
    'ExpectedVersionPrefix = "3.4.0"',
    'System.DateTime System.Diagnostics.Process::get_StartTime()',
    'matches.Length > 1',
    'get_UtcNow',
    'STEP05.16 STEAMKIT IOS PATCH: PASS',
):
    if required not in patcher:
        raise SystemExit(f'ERROR: SteamKit iOS patch guard missing: {required}')

codemagic = Path('scripts/codemagic-build.sh').read_text()
unit_index = codemagic.find('bash scripts/run-unit-tests.sh')
workload_index = codemagic.find('workload install ios')
build_index = codemagic.find('bash scripts/build-step05-final.sh')
if min(unit_index, workload_index, build_index) < 0 or not (unit_index < workload_index < build_index):
    raise SystemExit('ERROR: Codemagic must run host unit tests before workload install and iOS publish.')

print('Step 05.16 final foundation/Core/iOS/test boundary validation passed.')
PY

if grep -RniE 'Godot|Harmony|NativeGodotHost' src tests --include='*.cs' --include='*.csproj'; then
  echo "ERROR: final Step 05 source contains a forbidden later-stage subsystem." >&2
  exit 3
fi

for script in scripts/*.sh; do
  bash -n "$script"
done

echo "Step 05.16 repository validation passed."
