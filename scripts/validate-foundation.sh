#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

python3 - <<'PY'
from pathlib import Path
import xml.etree.ElementTree as ET

required = [
    'src/StS2Launcher.Core/StS2Launcher.Core.csproj',
    'src/StS2Launcher.Core/CoreSelfTest.cs',
    'src/StS2Launcher.Core/CredentialStoreVerifier.cs',
    'src/StS2Launcher.Core/FoundationVerificationResult.cs',
    'src/StS2Launcher.Core/SteamHttpClientFactory.cs',
    'src/StS2Launcher.Core/SteamConnectionProbe.cs',
    'src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj',
    'src/StS2Launcher.Step05.iOS/AppDelegate.cs',
    'src/StS2Launcher.Step05.iOS/SceneDelegate.cs',
    'src/StS2Launcher.Step05.iOS/Platform/KeychainCredentialStore.cs',
    'src/StS2Launcher.Step05.iOS/Platform/KeychainProbe.cs',
    'tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj',
    'tests/StS2Launcher.Core.Tests/LauncherControllerTests.cs',
    'tests/StS2Launcher.Core.Tests/CredentialStoreVerifierTests.cs',
    'tests/StS2Launcher.Core.Tests/SteamFoundationTests.cs',
    'tests/StS2Launcher.Core.Tests/FoundationVerificationTests.cs',
    'tools/StS2Launcher.SteamKitIosPatcher/Program.cs',
]
for name in required:
    if not Path(name).is_file():
        raise SystemExit(f'ERROR: missing foundation file: {name}')

core_proj = Path('src/StS2Launcher.Core/StS2Launcher.Core.csproj').read_text()
if '<PackageReference Include="SteamKit2" Version="3.4.0" />' not in core_proj:
    raise SystemExit('ERROR: foundation SteamKit2 pin changed.')

ios_proj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
for marker in (
    '<TargetFramework>net9.0-ios</TargetFramework>',
    '<RuntimeIdentifier>ios-arm64</RuntimeIdentifier>',
    '<SupportedOSPlatformVersion>18.0</SupportedOSPlatformVersion>',
    '<ApplicationId>com.community.sts2launcher</ApplicationId>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    'Step052RemoveMacOnlyDiskArbitrationFramework',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
):
    if marker not in ios_proj:
        raise SystemExit(f'ERROR: proven iOS foundation marker missing: {marker}')

core_self = Path('src/StS2Launcher.Core/CoreSelfTest.cs').read_text()
if 'const int total = 12;' not in core_self:
    raise SystemExit('ERROR: Core 12/12 regression changed.')

credential = Path('src/StS2Launcher.Core/CredentialStoreVerifier.cs').read_text()
for marker in ('const int total = 7;', 'store.Set(key, firstValue)', 'store.Set(key, secondValue)', 'store.Delete(key)'):
    if marker not in credential:
        raise SystemExit(f'ERROR: credential regression marker missing: {marker}')

keychain = Path('src/StS2Launcher.Step05.iOS/Platform/KeychainCredentialStore.cs').read_text()
for marker in ('SecKind.GenericPassword', 'SecAccessible.AfterFirstUnlockThisDeviceOnly', 'SecKeyChain.Add', 'SecKeyChain.Remove'):
    if marker not in keychain:
        raise SystemExit(f'ERROR: Keychain marker missing: {marker}')

factory = Path('src/StS2Launcher.Core/SteamHttpClientFactory.cs').read_text()
for marker in ('HttpClientPurpose.CMWebSocket', 'new SocketsHttpHandler()', 'new HttpClient()', 'ProductInfoHeaderValue("SteamKit"'):
    if marker not in factory:
        raise SystemExit(f'ERROR: Steam HTTP foundation marker missing: {marker}')

probe = Path('src/StS2Launcher.Core/SteamConnectionProbe.cs').read_text()
for marker in ('ProtocolTypes.WebSocket', 'WithHttpClientFactory(Factory)', 'ConnectedCallback', 'DisconnectedCallback', 'CmWebSocketFactoryUsed'):
    if marker not in probe:
        raise SystemExit(f'ERROR: Steam CM regression marker missing: {marker}')

patcher = Path('tools/StS2Launcher.SteamKitIosPatcher/Program.cs').read_text()
for marker in ('ExpectedVersionPrefix = "3.4.0"', 'matches.Length > 1'):
    if marker not in patcher:
        raise SystemExit(f'ERROR: SteamKit patcher guard missing: {marker}')

test_proj = Path('tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj').read_text()
for marker in ('<TargetFramework>net9.0</TargetFramework>', '<IsTestProject>true</IsTestProject>', 'MSTest.TestFramework'):
    if marker not in test_proj:
        raise SystemExit(f'ERROR: foundation test project marker missing: {marker}')

test_text = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))
for marker in (
    'ExistingCoreRegressionSelfTestStillPassesTwelveOfTwelve',
    'RoundTripPassesAndLeavesStoreClean',
    'CmWebSocketPurposeUsesSocketsHttpHandlerPolicy',
    'ThreeOfThreeResultRequiresAllConnectionGates',
    'AllFiveFoundationGatesProducePass',
):
    if marker not in test_text:
        raise SystemExit(f'ERROR: foundation unit-test marker missing: {marker}')
if 'ThrowsException<' in test_text or '[DataTestMethod]' in test_text:
    raise SystemExit('ERROR: incompatible/obsolete MSTest API detected.')

print('Steps 01-05 foundation validation: PASS')
print('  Core 12/12 contract retained')
print('  Keychain 7/7 contract retained')
print('  Steam CM 3/3 implementation retained')
print('  iOS trim/link compatibility fixes retained')
print('  foundation host unit tests retained')
PY
