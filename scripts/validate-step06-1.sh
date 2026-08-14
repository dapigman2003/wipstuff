#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

bash scripts/validate-foundation.sh

python3 - <<'PY'
from pathlib import Path
import plistlib

core_proj = Path('src/StS2Launcher.Core/StS2Launcher.Core.csproj').read_text()
ios_proj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if plist.get('CFBundleShortVersionString') != '0.0.24' or str(plist.get('CFBundleVersion')) != '24':
    raise SystemExit('ERROR: source Info.plist must be Step 06.1 version 0.0.24 (24).')

if '<PackageReference Include="SteamKit2" Version="3.4.0" />' not in core_proj:
    raise SystemExit('ERROR: SteamKit2 must remain pinned to 3.4.0.')
for marker in (
    '<ApplicationVersion>24</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.24</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    'Step052RemoveMacOnlyDiskArbitrationFramework',
):
    if marker not in ios_proj:
        raise SystemExit(f'ERROR: iOS project marker missing: {marker}')

# Steps 01-05 proven foundation remains intact.
core_self_test = Path('src/StS2Launcher.Core/CoreSelfTest.cs').read_text()
if 'const int total = 12;' not in core_self_test:
    raise SystemExit('ERROR: Core 12/12 self-test changed unexpectedly.')
credential_verifier = Path('src/StS2Launcher.Core/CredentialStoreVerifier.cs').read_text()
for marker in ('const int total = 7;', 'store.Set(key, firstValue)', 'store.Set(key, secondValue)', 'store.Delete(key)'):
    if marker not in credential_verifier:
        raise SystemExit(f'ERROR: credential verifier marker missing: {marker}')
factory = Path('src/StS2Launcher.Core/SteamHttpClientFactory.cs').read_text()
for marker in ('HttpClientPurpose.CMWebSocket', 'new SocketsHttpHandler()', 'ProductInfoHeaderValue("SteamKit"'):
    if marker not in factory:
        raise SystemExit(f'ERROR: proven HTTP factory marker missing: {marker}')
probe = Path('src/StS2Launcher.Core/SteamConnectionProbe.cs').read_text()
for marker in ('ProtocolTypes.WebSocket', 'WithHttpClientFactory(Factory)', 'ConnectedCallback', 'DisconnectedCallback'):
    if marker not in probe:
        raise SystemExit(f'ERROR: Step 05 regression probe marker missing: {marker}')

# Step 06 credential auth stays intact; Step 06.1 adds only mobile confirmation polling.
auth = Path('src/StS2Launcher.Core/SteamAuthenticationAttempt.cs').read_text()
for marker in (
    'BeginAuthSessionViaCredentialsAsync',
    'PollingWaitForResultAsync(token)',
    'SteamUser.LogOnDetails',
    'AccessToken = pollResult.RefreshToken',
    'LoggedOnCallback',
    'EOSType.IOSUnknown',
    'IsPersistentSession = false',
    'GuardData = null',
    'ProtocolTypes.WebSocket',
    'WithHttpClientFactory(Factory)',
    'MobileApprovalCompleted',
    'TimeSpan timeout',
):
    if marker not in auth:
        raise SystemExit(f'ERROR: Step 06.1 auth marker missing: {marker}')

guard = Path('src/StS2Launcher.Core/SteamGuardChallengeAuthenticator.cs').read_text()
for marker in (
    'GetDeviceCodeAsync',
    'GetEmailCodeAsync',
    'AcceptDeviceConfirmationAsync',
    'MobileApprovalRequested = true',
    'Task.FromResult(true)',
    'WaitingForMobileApproval',
    'Task.FromException',
):
    if marker not in guard:
        raise SystemExit(f'ERROR: Step 06.1 mobile-approval policy marker missing: {marker}')

# 06.1 must not broaden into manual code entry or token persistence.
combined = '\n'.join(p.read_text(errors='ignore') for p in Path('src').rglob('*.cs'))
for forbidden in (
    'SendSteamGuardCodeAsync',
    'TwoFactorCode =',
    'AuthCode =',
    'refresh-token"',
    'access-token"',
    'steam-password"',
    'GuardData = _',
):
    if forbidden in combined:
        raise SystemExit(f'ERROR: Step 06.1 contains out-of-scope code/persistence marker: {forbidden}')

# No ownership/content work.
for forbidden in ('SteamApps', 'PICS', '2868840', 'GetDepot', 'DepotManifest', 'CDN.Client'):
    if forbidden in auth:
        raise SystemExit(f'ERROR: Step 06.1 broadened into later scope: {forbidden}')

root_view = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 06.1 — STEAM GUARD MOBILE APPROVAL',
    'Version 0.0.24',
    'Run Foundation 5/5 Regression',
    'Start Step 06.1 Authentication',
    'WAITING FOR STEAM GUARD',
    'TimeSpan.FromMinutes(3)',
    'Mobile approval requested:',
    'Mobile approval completed:',
    'Credential/token/Guard persistence: NONE',
    'Ownership request: NOT RUN',
    'SecureTextEntry = secure',
    '_passwordField.Text = string.Empty',
):
    if marker not in root_view:
        raise SystemExit(f'ERROR: Step 06.1 UI marker missing: {marker}')

# Unit tests preserve foundation and prove the new mobile-poll contract.
test_text = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))
for marker in (
    'ExistingCoreRegressionSelfTestStillPassesTwelveOfTwelve',
    'RoundTripPassesAndLeavesStoreClean',
    'AllFiveFoundationGatesProducePass',
    'DeviceCodeChallengeIsObservedWithoutProvidingCode',
    'EmailChallengePreservesSteamAssociatedMessageWithoutProvidingCode',
    'MobileConfirmationOptsIntoSteamKitPolling',
    'MobileApprovedAuthenticatedResultHasDistinctSummary',
    'TimeoutIsDistinctFromUserCancellation',
    'BlankCredentialsAreRejectedBeforeNetworkWork',
):
    if marker not in test_text:
        raise SystemExit(f'ERROR: Step 06.1 test marker missing: {marker}')
if 'ThrowsException<' in test_text or '[DataTestMethod]' in test_text:
    raise SystemExit('ERROR: incompatible/obsolete MSTest API returned.')

patcher = Path('tools/StS2Launcher.SteamKitIosPatcher/Program.cs').read_text()
for marker in ('ExpectedVersionPrefix = "3.4.0"', 'matches.Length > 1'):
    if marker not in patcher:
        raise SystemExit(f'ERROR: SteamKit patcher guard missing: {marker}')

print('Step 06.1 source validation: PASS')
print('  Steps 01-05 foundation preserved')
print('  Step 06 credential auth preserved')
print('  SteamKit2 3.4.0 retained')
print('  Mobile Steam Guard confirmation opts into same-session polling')
print('  Device/email code entry remains out of scope')
print('  No credential/token/guard-data persistence')
print('  No ownership/download scope added')
PY
