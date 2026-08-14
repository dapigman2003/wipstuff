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
if plist.get('CFBundleShortVersionString') != '0.0.26' or str(plist.get('CFBundleVersion')) != '26':
    raise SystemExit('ERROR: source Info.plist must be Step 06.3 version 0.0.26 (26).')

if '<PackageReference Include="SteamKit2" Version="3.4.0" />' not in core_proj:
    raise SystemExit('ERROR: SteamKit2 must remain pinned to 3.4.0.')
for marker in (
    '<ApplicationVersion>26</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.26</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    'Step052RemoveMacOnlyDiskArbitrationFramework',
):
    if marker not in ios_proj:
        raise SystemExit(f'ERROR: iOS project marker missing: {marker}')

# Steps 01-05 foundation remains regression-protected.
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

# Step 06/06.1 credential auth + mobile approval + Step 06.2 persistence remain intact.
auth = Path('src/StS2Launcher.Core/SteamAuthenticationAttempt.cs').read_text()
for marker in (
    'BeginAuthSessionViaCredentialsAsync',
    'PollingWaitForResultAsync(token)',
    'AccessToken = pollResult.RefreshToken',
    'LoggedOnCallback',
    'EOSType.IOSUnknown',
    'IsPersistentSession = true',
    'GuardData = null',
    'ProtocolTypes.WebSocket',
    'WithHttpClientFactory(Factory)',
    'MobileApprovalCompleted',
    '_sessionStore.Save(new SteamSavedSession(',
    'SessionPersisted: sessionPersisted',
):
    if marker not in auth:
        raise SystemExit(f'ERROR: prior authentication/persistence marker missing: {marker}')

ok_check = auth.find('if (logonResult != EResult.OK)')
save_call = auth.find('_sessionStore.Save(new SteamSavedSession(')
if ok_check < 0 or save_call < 0 or save_call < ok_check:
    raise SystemExit('ERROR: session persistence must remain after successful LoggedOn result gate.')

guard = Path('src/StS2Launcher.Core/SteamGuardChallengeAuthenticator.cs').read_text()
for marker in (
    'GetDeviceCodeAsync',
    'GetEmailCodeAsync',
    'AcceptDeviceConfirmationAsync',
    'MobileApprovalRequested = true',
    'Task.FromResult(true)',
    'WaitingForMobileApproval',
):
    if marker not in guard:
        raise SystemExit(f'ERROR: Step 06.1 mobile-approval marker missing: {marker}')

# Step 06.2 Keychain storage remains tiny and secret-safe.
session = Path('src/StS2Launcher.Core/SteamSavedSession.cs').read_text()
store = Path('src/StS2Launcher.Core/SteamSessionStore.cs').read_text()
for marker in ('RefreshToken=<redacted>', 'public string RefreshToken { get; }'):
    if marker not in session:
        raise SystemExit(f'ERROR: saved-session secrecy marker missing: {marker}')
for marker in (
    'StorageKey = "steam.session.v1"',
    'STS2-STEAM-SESSION-V1',
    '_credentialStore.Set(StorageKey, payload)',
    '_credentialStore.Get(StorageKey)',
    '_credentialStore.Delete(StorageKey)',
):
    if marker not in store:
        raise SystemExit(f'ERROR: saved-session store marker missing: {marker}')
for forbidden in ('System.Text.Json', 'Newtonsoft', 'Password', 'GuardData'):
    if forbidden in store:
        raise SystemExit(f'ERROR: saved-session store broadened unexpectedly: {forbidden}')

# Step 06.3: explicit invalid-local / identity-mismatch outcomes and a conservative recovery policy.
resume_result = Path('src/StS2Launcher.Core/SteamSessionResumeResult.cs').read_text()
for marker in (
    'InvalidLocalSession = 6',
    'IdentityMismatch = 7',
    'SAVED SESSION INVALID — local record',
    'SAVED SESSION INVALID — identity mismatch',
):
    if marker not in resume_result:
        raise SystemExit(f'ERROR: Step 06.3 resume-result marker missing: {marker}')

resume = Path('src/StS2Launcher.Core/SteamSessionResumeAttempt.cs').read_text()
for marker in (
    'Outcome: SteamSessionResumeOutcome.InvalidLocalSession',
    'outcome = SteamSessionResumeOutcome.IdentityMismatch',
    '_sessionStore.Load()',
    'ProtocolTypes.WebSocket',
    'WithHttpClientFactory(Factory)',
    'AccessToken = savedSession.RefreshToken',
    'LoggedOnCallback',
    'savedSession.SteamId64',
):
    if marker not in resume:
        raise SystemExit(f'ERROR: Step 06.3 resume marker missing: {marker}')
for forbidden in ('BeginAuthSessionViaCredentialsAsync', 'PollingWaitForResultAsync', '\n                Password =', 'SteamGuardChallengeAuthenticator'):
    if forbidden in resume:
        raise SystemExit(f'ERROR: saved-session resume must remain password/Guard-free: {forbidden}')

policy = Path('src/StS2Launcher.Core/SteamSessionRecoveryPolicy.cs').read_text()
for marker in (
    'ClearSavedSessionAndRequireInteractiveAuthentication',
    'SteamSessionResumeOutcome.InvalidLocalSession',
    'SteamSessionResumeOutcome.IdentityMismatch',
    'EResult.InvalidPassword',
    'EResult.Revoked',
    'EResult.Expired',
    'KeepSavedSession',
):
    if marker not in policy:
        raise SystemExit(f'ERROR: Step 06.3 recovery policy marker missing: {marker}')
for forbidden in ('ServiceUnavailable or', 'RateLimitExceeded or', 'TryAnotherCM or'):
    if forbidden in policy:
        raise SystemExit(f'ERROR: transient Steam result was added to destructive clear policy: {forbidden}')

keychain = Path('src/StS2Launcher.Step05.iOS/Platform/KeychainCredentialStore.cs').read_text()
for marker in ('com.community.sts2launcher.credentials', 'SecAccessible.AfterFirstUnlockThisDeviceOnly', 'SecKind.GenericPassword'):
    if marker not in keychain:
        raise SystemExit(f'ERROR: Keychain policy marker missing: {marker}')

# UI automatically attempts saved-session restore exactly once after Active lifecycle.
root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 06.3 — SESSION RECOVERY',
    'Version 0.0.26',
    'AUTO-RESTORE SAVED SESSION',
    '_automaticRestoreStarted',
    '_lifecycleActive && !_automaticRestoreStarted',
    '_ = RunAutomaticSessionRestoreAsync();',
    'AUTO SESSION PASS — authenticated',
    'SteamSessionRecoveryPolicy.Evaluate(result)',
    '_sessionStore.Clear()',
    'Saved session cleared by recovery:',
    'saved session was preserved',
    'Run Foundation 5/5 Regression',
    'Authenticate + Save Session',
    'Retry Saved Session Now (No Password)',
    'Sign Out / Clear Saved Session',
    'Password persistence: NONE',
    'Ownership request: NOT RUN',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 06.3 UI marker missing: {marker}')

# Do not accidentally broaden into ownership/content/manual Guard code work.
core_combined = '\n'.join(p.read_text(errors='ignore') for p in Path('src/StS2Launcher.Core').glob('*.cs'))
for forbidden in ('SendSteamGuardCodeAsync', 'TwoFactorCode =', 'AuthCode =', 'NewGuardData'):
    if forbidden in core_combined:
        raise SystemExit(f'ERROR: out-of-scope Guard code/persistence added: {forbidden}')
for target in (auth, resume, policy):
    for forbidden in ('SteamApps', 'PICS', '2868840', 'GetDepot', 'DepotManifest', 'CDN.Client'):
        if forbidden in target:
            raise SystemExit(f'ERROR: Step 06.3 broadened into ownership/content scope: {forbidden}')

# Unit tests: foundation + auth/persistence + new recovery policy.
test_text = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))
for marker in (
    'ExistingCoreRegressionSelfTestStillPassesTwelveOfTwelve',
    'RoundTripPassesAndLeavesStoreClean',
    'AllFiveFoundationGatesProducePass',
    'MobileConfirmationOptsIntoSteamKitPolling',
    'SaveLoadRoundTripPreservesIdentityAndToken',
    'ResumeAuthenticatedResultRequiresIdentityMatchInContract',
    'ValidAuthenticatedSessionIsKept',
    'InvalidLocalSessionIsCleared',
    'IdentityMismatchIsCleared',
    'DefinitelyUnusableCredentialResultsAreCleared',
    'TransientOrRoutingResultsPreserveSavedSession',
    'TimeoutAndCancellationPreserveSavedSession',
):
    if marker not in test_text:
        raise SystemExit(f'ERROR: Step 06.3 test marker missing: {marker}')
if 'ThrowsException<' in test_text or '[DataTestMethod]' in test_text:
    raise SystemExit('ERROR: incompatible/obsolete MSTest API returned.')

patcher = Path('tools/StS2Launcher.SteamKitIosPatcher/Program.cs').read_text()
for marker in ('ExpectedVersionPrefix = "3.4.0"', 'matches.Length > 1'):
    if marker not in patcher:
        raise SystemExit(f'ERROR: SteamKit patcher guard missing: {marker}')

print('Step 06.3 source validation: PASS')
print('  Steps 01-05 foundation preserved')
print('  Step 06/06.1 credential auth + mobile Guard preserved')
print('  Step 06.2 Keychain persistence/resume/sign-out preserved')
print('  Saved session auto-restores once after Active lifecycle')
print('  Invalid local record and SteamID mismatch trigger Keychain reset')
print('  InvalidPassword/Revoked/Expired trigger Keychain reset')
print('  Transient/network/service failures preserve the saved session')
print('  Password/Guard secrets remain unpersisted')
print('  No ownership/download scope added')
PY
