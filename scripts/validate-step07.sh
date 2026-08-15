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
if plist.get('CFBundleShortVersionString') != '0.0.28' or str(plist.get('CFBundleVersion')) != '28':
    raise SystemExit('ERROR: source Info.plist must be Step 07 version 0.0.28 (28).')

if '<PackageReference Include="SteamKit2" Version="3.4.0" />' not in core_proj:
    raise SystemExit('ERROR: SteamKit2 must remain pinned to 3.4.0.')
for marker in (
    '<ApplicationVersion>28</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.28</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    'Step052RemoveMacOnlyDiskArbitrationFramework',
):
    if marker not in ios_proj:
        raise SystemExit(f'ERROR: iOS project marker missing: {marker}')

# Step 06 / 06.1 / 06.2 / 06.3.1 must remain intact as regressions.
auth = Path('src/StS2Launcher.Core/SteamAuthenticationAttempt.cs').read_text()
for marker in (
    'BeginAuthSessionViaCredentialsAsync',
    'PollingWaitForResultAsync(token)',
    'IsPersistentSession = true',
    'SteamPersistentLogOnDetails.Create(',
    '_sessionStore.Save(new SteamSavedSession(',
    'MobileApprovalCompleted',
):
    if marker not in auth:
        raise SystemExit(f'ERROR: prior authentication marker missing: {marker}')
if 'ShouldRememberPassword = false' in auth or 'steamUser.LogOff()' in auth:
    raise SystemExit('ERROR: Step 06.3.1 persistent-session fix regressed in credential auth.')

guard = Path('src/StS2Launcher.Core/SteamGuardChallengeAuthenticator.cs').read_text()
for marker in ('AcceptDeviceConfirmationAsync', 'Task.FromResult(true)', 'WaitingForMobileApproval'):
    if marker not in guard:
        raise SystemExit(f'ERROR: Step 06.1 mobile-approval marker missing: {marker}')

store = Path('src/StS2Launcher.Core/SteamSessionStore.cs').read_text()
for marker in ('StorageKey = "steam.session.v1"', '_credentialStore.Set(StorageKey, payload)', '_credentialStore.Get(StorageKey)', '_credentialStore.Delete(StorageKey)'):
    if marker not in store:
        raise SystemExit(f'ERROR: Step 06.2 session-store marker missing: {marker}')
for forbidden in ('Password', 'GuardData', 'System.Text.Json', 'Newtonsoft'):
    if forbidden in store:
        raise SystemExit(f'ERROR: session store broadened unexpectedly: {forbidden}')

persistent = Path('src/StS2Launcher.Core/SteamPersistentLogOnDetails.cs').read_text()
for marker in ('ShouldRememberPassword = true', 'LoginID = SteamLoginIdentity.Create()', 'ClientOSType = EOSType.IOSUnknown'):
    if marker not in persistent:
        raise SystemExit(f'ERROR: Step 06.3.1 logon-policy marker missing: {marker}')

resume = Path('src/StS2Launcher.Core/SteamSessionResumeAttempt.cs').read_text()
for marker in ('_sessionStore.Load()', 'SteamPersistentLogOnDetails.Create(', 'LoggedOnCallback', 'savedSession.SteamId64'):
    if marker not in resume:
        raise SystemExit(f'ERROR: Step 06.3.1 resume marker missing: {marker}')
if 'steamUser.LogOff()' in resume or 'ShouldRememberPassword = false' in resume:
    raise SystemExit('ERROR: Step 06.3.1 saved-session semantics regressed.')

policy = Path('src/StS2Launcher.Core/SteamSessionRecoveryPolicy.cs').read_text()
for marker in ('EResult.InvalidPassword', 'EResult.Revoked', 'EResult.Expired', 'KeepSavedSession'):
    if marker not in policy:
        raise SystemExit(f'ERROR: session recovery policy marker missing: {marker}')

# Step 07: exact one-app ownership-ticket boundary.
result = Path('src/StS2Launcher.Core/SteamOwnershipVerificationResult.cs').read_text()
decision = Path('src/StS2Launcher.Core/SteamOwnershipDecision.cs').read_text()
ownership = Path('src/StS2Launcher.Core/SteamOwnershipVerificationAttempt.cs').read_text()

for marker in (
    'public const uint TargetAppId = 2868840;',
    'SteamApps',
    'GetAppOwnershipTicket(TargetAppId)',
    'AppOwnershipTicketCallback',
    'SteamPersistentLogOnDetails.Create(',
    'savedSession.SteamId64',
    'SteamOwnershipDecision.EvaluateTicket(',
    'ownershipTicketLength = callback.Ticket?.Length ?? 0',
    'steamClient.Disconnect()',
):
    if marker not in ownership:
        raise SystemExit(f'ERROR: Step 07 ownership implementation marker missing: {marker}')

for forbidden in (
    'PICSGet',
    'GetDepotDecryptionKey',
    'DepotManifest',
    'CDN.Client',
    'DownloadManifest',
    'DownloadDepotChunk',
    'RequestFreeLicense',
):
    if forbidden in ownership:
        raise SystemExit(f'ERROR: Step 07 broadened beyond ownership-ticket scope: {forbidden}')

for marker in (
    'result != EResult.OK',
    'returnedAppId != targetAppId',
    'ticketLength <= 0',
    'SteamOwnershipVerificationOutcome.Owned',
):
    if marker not in decision:
        raise SystemExit(f'ERROR: Step 07 ownership decision marker missing: {marker}')

if 'byte[]' in result:
    raise SystemExit('ERROR: ownership result must never retain/expose raw ticket bytes.')
for marker in ('OwnershipTicketLength', 'OwnershipProven', 'OWNERSHIP PASS — App'):
    if marker not in result:
        raise SystemExit(f'ERROR: Step 07 result marker missing: {marker}')

# UI exposes only the ownership result and explicitly states content is not requested.
root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 07 — OWNERSHIP VERIFICATION',
    'Version 0.0.28',
    'APP ID 2868840 • OWNERSHIP TICKET ONLY • NO DOWNLOAD',
    'Verify Slay the Spire 2 Ownership',
    'RunOwnershipVerificationAsync',
    'Ownership ticket payload display/logging/persistence: NONE',
    'PICS request: NOT RUN',
    'Depot/manifest/CDN/download request: NOT RUN',
    'Run Foundation 5/5 Regression',
    'Authenticate + Save Session',
    'Retry Saved Session Now (No Password)',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 07 UI marker missing: {marker}')

# Tests must preserve all prior coverage and add pure ownership classification.
test_text = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))
for marker in (
    'ExistingCoreRegressionSelfTestStillPassesTwelveOfTwelve',
    'RoundTripPassesAndLeavesStoreClean',
    'AllFiveFoundationGatesProducePass',
    'MobileConfirmationOptsIntoSteamKitPolling',
    'SaveLoadRoundTripPreservesIdentityAndToken',
    'PersistentLogOnDetailsMatchPersistentAuthContract',
    'TargetAppIdIsSlayTheSpire2',
    'OkMatchingNonEmptyTicketProvesOwnership',
    'NonOkResultDoesNotProveOwnership',
    'EmptyTicketDoesNotProveOwnership',
    'WrongAppIdDoesNotProveOwnership',
    'OwnershipResultNeverExposesRawTicketBytes',
):
    if marker not in test_text:
        raise SystemExit(f'ERROR: Step 07 test marker missing: {marker}')
if 'ThrowsException<' in test_text or '[DataTestMethod]' in test_text:
    raise SystemExit('ERROR: incompatible/obsolete MSTest API detected.')

# Build pipeline and patcher must remain guarded.
patcher = Path('tools/StS2Launcher.SteamKitIosPatcher/Program.cs').read_text()
for marker in ('ExpectedVersionPrefix = "3.4.0"', 'matches.Length > 1'):
    if marker not in patcher:
        raise SystemExit(f'ERROR: SteamKit patcher guard missing: {marker}')

codemagic = Path('codemagic.yaml').read_text()
for marker in ('ios-step-07:', 'artifacts/StS2-Launcher-Step-07.ipa', 'artifacts/step07-build-summary.txt'):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Codemagic Step 07 marker missing: {marker}')

print('Step 07 source validation: PASS')
print('  Steps 01-05 foundation preserved')
print('  Steps 06-06.3.1 authentication/session behavior preserved')
print('  Target AppID fixed to 2868840')
print('  Ownership uses SteamApps.GetAppOwnershipTicket only')
print('  Ownership requires exact AppID + EResult.OK + non-empty ticket')
print('  Raw ownership ticket bytes are never exposed or persisted')
print('  No PICS/depot/manifest/CDN/download capability added')
PY
