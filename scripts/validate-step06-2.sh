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
if plist.get('CFBundleShortVersionString') != '0.0.25' or str(plist.get('CFBundleVersion')) != '25':
    raise SystemExit('ERROR: source Info.plist must be Step 06.2 version 0.0.25 (25).')

if '<PackageReference Include="SteamKit2" Version="3.4.0" />' not in core_proj:
    raise SystemExit('ERROR: SteamKit2 must remain pinned to 3.4.0.')
for marker in (
    '<ApplicationVersion>25</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.25</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    'Step052RemoveMacOnlyDiskArbitrationFramework',
):
    if marker not in ios_proj:
        raise SystemExit(f'ERROR: iOS project marker missing: {marker}')

# Steps 01-05 physical foundation remains regression-protected.
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

# Step 06 / 06.1 auth and mobile Guard remain intact, now requesting a
# persistent refresh token and saving it only after a successful Steam logon.
auth = Path('src/StS2Launcher.Core/SteamAuthenticationAttempt.cs').read_text()
for marker in (
    'BeginAuthSessionViaCredentialsAsync',
    'PollingWaitForResultAsync(token)',
    'SteamUser.LogOnDetails',
    'AccessToken = pollResult.RefreshToken',
    'LoggedOnCallback',
    'EOSType.IOSUnknown',
    'IsPersistentSession = true',
    'GuardData = null',
    'ProtocolTypes.WebSocket',
    'WithHttpClientFactory(Factory)',
    'MobileApprovalCompleted',
    '_sessionStore.Save(new SteamSavedSession(',
    'pollResult.RefreshToken',
    'SessionPersisted: sessionPersisted',
):
    if marker not in auth:
        raise SystemExit(f'ERROR: Step 06.2 auth/persistence marker missing: {marker}')

# Save must occur only after the successful LoggedOn result check.
ok_check = auth.find('if (logonResult != EResult.OK)')
save_call = auth.find('_sessionStore.Save(new SteamSavedSession(')
if ok_check < 0 or save_call < 0 or save_call < ok_check:
    raise SystemExit('ERROR: session persistence must occur after the successful LoggedOn result gate.')

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
        raise SystemExit(f'ERROR: Step 06.1 mobile-approval regression marker missing: {marker}')

# Keychain session format must remain tiny, versioned, and reflection-free.
session = Path('src/StS2Launcher.Core/SteamSavedSession.cs').read_text()
store = Path('src/StS2Launcher.Core/SteamSessionStore.cs').read_text()
for marker in (
    'RefreshToken=<redacted>',
    'public string RefreshToken { get; }',
):
    if marker not in session:
        raise SystemExit(f'ERROR: saved-session secrecy marker missing: {marker}')
for marker in (
    'StorageKey = "steam.session.v1"',
    'STS2-STEAM-SESSION-V1',
    '_credentialStore.Set(StorageKey, payload)',
    '_credentialStore.Get(StorageKey)',
    '_credentialStore.Delete(StorageKey)',
    'Convert.ToBase64String',
    'Convert.FromBase64String',
):
    if marker not in store:
        raise SystemExit(f'ERROR: saved-session store marker missing: {marker}')
for forbidden in ('System.Text.Json', 'Newtonsoft', 'Password', 'GuardData'):
    if forbidden in store:
        raise SystemExit(f'ERROR: saved-session store broadened beyond minimal refresh-token material: {forbidden}')

resume = Path('src/StS2Launcher.Core/SteamSessionResumeAttempt.cs').read_text()
for marker in (
    '_sessionStore.Load()',
    'ProtocolTypes.WebSocket',
    'WithHttpClientFactory(Factory)',
    'AccessToken = savedSession.RefreshToken',
    'Username = savedSession.AccountName',
    'LoggedOnCallback',
    'logonResult != EResult.OK',
    'savedSession.SteamId64',
    'IdentityMatched: identityMatched',
):
    if marker not in resume:
        raise SystemExit(f'ERROR: saved-session resume marker missing: {marker}')
for forbidden in (
    'BeginAuthSessionViaCredentialsAsync',
    'PollingWaitForResultAsync',
    '\n                Password =',
    'SteamGuardChallengeAuthenticator',
):
    if forbidden in resume:
        raise SystemExit(f'ERROR: saved-session resume must not request password/new Guard auth: {forbidden}')

keychain = Path('src/StS2Launcher.Step05.iOS/Platform/KeychainCredentialStore.cs').read_text()
for marker in (
    'com.community.sts2launcher.credentials',
    'SecAccessible.AfterFirstUnlockThisDeviceOnly',
    'SecKind.GenericPassword',
):
    if marker not in keychain:
        raise SystemExit(f'ERROR: Step 06.2 Keychain policy marker missing: {marker}')

# No manual Guard codes, ownership, or content work in Step 06.2.
core_combined = '\n'.join(p.read_text(errors='ignore') for p in Path('src/StS2Launcher.Core').glob('*.cs'))
for forbidden in (
    'SendSteamGuardCodeAsync',
    'TwoFactorCode =',
    'AuthCode =',
    'NewGuardData',
):
    if forbidden in core_combined:
        raise SystemExit(f'ERROR: Step 06.2 contains out-of-scope Guard persistence/code entry: {forbidden}')
for forbidden in ('SteamApps', 'PICS', '2868840', 'GetDepot', 'DepotManifest', 'CDN.Client'):
    if forbidden in auth or forbidden in resume:
        raise SystemExit(f'ERROR: Step 06.2 broadened into ownership/content scope: {forbidden}')

root_view = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 06.2 — KEYCHAIN SESSION RESUME',
    'Version 0.0.25',
    'Run Foundation 5/5 Regression',
    'Authenticate + Save Session',
    'Resume Saved Session (No Password)',
    'Sign Out / Clear Saved Session',
    'Refresh token: PRESENT (not displayed)',
    'Session persisted to Keychain:',
    'Password persistence: NONE',
    'Steam Guard secret/code persistence: NONE',
    'Ownership request: NOT RUN',
    'SecureTextEntry = secure',
    '_passwordField.Text = string.Empty',
    '_sessionStore.Clear()',
):
    if marker not in root_view:
        raise SystemExit(f'ERROR: Step 06.2 UI marker missing: {marker}')

# Unit tests preserve foundation and prove storage/resume result contracts.
test_text = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))
for marker in (
    'ExistingCoreRegressionSelfTestStillPassesTwelveOfTwelve',
    'RoundTripPassesAndLeavesStoreClean',
    'AllFiveFoundationGatesProducePass',
    'MobileConfirmationOptsIntoSteamKitPolling',
    'PersistedMobileApprovedAuthenticatedResultHasDistinctSummary',
    'SaveLoadRoundTripPreservesIdentityAndToken',
    'SaveOverwritesPreviousSession',
    'ClearRemovesSavedSessionAndIsIdempotent',
    'MalformedStoredPayloadIsRejected',
    'SavedSessionToStringNeverExposesRefreshToken',
    'ResumeAuthenticatedResultRequiresIdentityMatchInContract',
    'ResumeNoSavedSessionIsDistinctFromFailure',
):
    if marker not in test_text:
        raise SystemExit(f'ERROR: Step 06.2 test marker missing: {marker}')
if 'ThrowsException<' in test_text or '[DataTestMethod]' in test_text:
    raise SystemExit('ERROR: incompatible/obsolete MSTest API returned.')

patcher = Path('tools/StS2Launcher.SteamKitIosPatcher/Program.cs').read_text()
for marker in ('ExpectedVersionPrefix = "3.4.0"', 'matches.Length > 1'):
    if marker not in patcher:
        raise SystemExit(f'ERROR: SteamKit patcher guard missing: {marker}')

print('Step 06.2 source validation: PASS')
print('  Steps 01-05 foundation preserved')
print('  Step 06/06.1 authentication + mobile Guard flow preserved')
print('  Persistent auth session requested')
print('  Refresh token + account identity stored only in device-bound iOS Keychain')
print('  Password and Steam Guard secrets/codes are never persisted')
print('  Password-free saved-session resume implemented')
print('  Stored-vs-returned SteamID identity check required')
print('  Explicit sign-out clears the saved Keychain session')
print('  No ownership/download scope added')
PY
