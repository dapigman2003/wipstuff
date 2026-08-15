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

if plist.get('CFBundleShortVersionString') != '0.0.29' or str(plist.get('CFBundleVersion')) != '29':
    raise SystemExit('ERROR: source Info.plist must be Step 08 version 0.0.29 (29).')
if '<PackageReference Include="SteamKit2" Version="3.4.0" />' not in core_proj:
    raise SystemExit('ERROR: SteamKit2 must remain pinned to 3.4.0.')
for marker in (
    '<ApplicationVersion>29</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.29</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    'Step052RemoveMacOnlyDiskArbitrationFramework',
):
    if marker not in ios_proj:
        raise SystemExit(f'ERROR: iOS project marker missing: {marker}')

# Steps 06-06.3.1 remain regression-protected.
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
    raise SystemExit('ERROR: persistent-session behavior regressed in credential auth.')

persistent = Path('src/StS2Launcher.Core/SteamPersistentLogOnDetails.cs').read_text()
for marker in ('ShouldRememberPassword = true', 'LoginID = SteamLoginIdentity.Create()', 'ClientOSType = EOSType.IOSUnknown'):
    if marker not in persistent:
        raise SystemExit(f'ERROR: persistent-logon marker missing: {marker}')

resume = Path('src/StS2Launcher.Core/SteamSessionResumeAttempt.cs').read_text()
for marker in ('_sessionStore.Load()', 'SteamPersistentLogOnDetails.Create(', 'LoggedOnCallback', 'savedSession.SteamId64'):
    if marker not in resume:
        raise SystemExit(f'ERROR: saved-session resume marker missing: {marker}')

# Step 07 remains a strict ownership-only regression.
ownership = Path('src/StS2Launcher.Core/SteamOwnershipVerificationAttempt.cs').read_text()
decision = Path('src/StS2Launcher.Core/SteamOwnershipDecision.cs').read_text()
ownership_result = Path('src/StS2Launcher.Core/SteamOwnershipVerificationResult.cs').read_text()
for marker in (
    'public const uint TargetAppId = 2868840;',
    'GetAppOwnershipTicket(TargetAppId)',
    'AppOwnershipTicketCallback',
    'SteamOwnershipDecision.EvaluateTicket(',
    'ownershipTicketLength = callback.Ticket?.Length ?? 0',
):
    if marker not in ownership:
        raise SystemExit(f'ERROR: Step 07 ownership marker missing: {marker}')
for forbidden in ('PICSGet', 'GetDepotDecryptionKey', 'DepotManifest', 'CDN.Client', 'DownloadManifest', 'DownloadDepotChunk'):
    if forbidden in ownership:
        raise SystemExit(f'ERROR: Step 07 ownership operation broadened unexpectedly: {forbidden}')
for marker in ('result != EResult.OK', 'returnedAppId != targetAppId', 'ticketLength <= 0', 'SteamOwnershipVerificationOutcome.Owned'):
    if marker not in decision:
        raise SystemExit(f'ERROR: Step 07 decision marker missing: {marker}')
if 'byte[]' in ownership_result:
    raise SystemExit('ERROR: Step 07 result must not expose raw ownership-ticket bytes.')

# Step 08 is exactly PICS access metadata + product info parsing after ownership.
attempt = Path('src/StS2Launcher.Core/SteamContentDiscoveryAttempt.cs').read_text()
parser = Path('src/StS2Launcher.Core/SteamContentDiscoveryParser.cs').read_text()
result = Path('src/StS2Launcher.Core/SteamContentDiscoveryResult.cs').read_text()
models = Path('src/StS2Launcher.Core/SteamDepotDiscovery.cs').read_text()

for marker in (
    'public const uint TargetAppId = SteamOwnershipVerificationAttempt.TargetAppId;',
    'GetAppOwnershipTicket(TargetAppId)',
    'SteamOwnershipDecision.EvaluateTicket(',
    'PICSGetAccessTokens(TargetAppId, package: null)',
    'PICSGetProductInfo(',
    'new SteamApps.PICSRequest(TargetAppId, accessToken)',
    'metaDataOnly: false',
    'SteamContentDiscoveryParser.Parse(appInfo.KeyValues)',
    'callback.ResponsePending',
    'callback.Apps.TryGetValue(TargetAppId',
    'tokenCallback.AppTokensDenied.Contains(TargetAppId)',
):
    if marker not in attempt:
        raise SystemExit(f'ERROR: Step 08 discovery implementation marker missing: {marker}')

for forbidden in (
    'GetDepotDecryptionKey',
    'DepotManifest',
    'new SteamContent(',
    'CDN.Client',
    'GetServersForSteamPipe',
    'GetManifestRequestCode',
    'DownloadManifest',
    'DownloadDepotChunk',
    'File.Write',
    'FileStream',
    'HttpClient.GetByteArray',
    'RequestFreeLicense',
):
    if forbidden in attempt:
        raise SystemExit(f'ERROR: Step 08 broadened beyond metadata discovery: {forbidden}')

for marker in ('uint.TryParse(depotNode.Name', 'Child(depotNode, "config")', 'Child(depotNode, "manifests")', 'Child(branchNode, "gid")', 'ulong.TryParse(rawManifestId'):
    if marker not in parser:
        raise SystemExit(f'ERROR: Step 08 parser marker missing: {marker}')
for marker in ('SteamManifestDiscovery', 'SteamDepotDiscovery', 'OsList', 'OsArch', 'Language', 'Manifests'):
    if marker not in models:
        raise SystemExit(f'ERROR: Step 08 discovery model marker missing: {marker}')
for marker in ('PicsAccessTokenReceived', 'PicsProductInfoCallbackReceived', 'DepotCount', 'ManifestCount', 'DISCOVERY PASS —'):
    if marker not in result:
        raise SystemExit(f'ERROR: Step 08 result marker missing: {marker}')
if 'byte[]' in result or 'ulong AccessToken' in result or 'ulong PicsAccessToken' in result:
    raise SystemExit('ERROR: Step 08 result must not expose raw ticket bytes or PICS access-token values.')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 08 — DEPOT / MANIFEST DISCOVERY',
    'Version 0.0.29',
    'APP ID 2868840 • PICS METADATA ONLY • NO DOWNLOAD',
    'Step 07 regression — Slay the Spire 2 ownership',
    'Discover StS2 Depots + Manifests',
    'RunContentDiscoveryAsync',
    'PICS access-token value display/logging/persistence: NONE',
    'Depot decryption key request: NOT RUN',
    'Manifest body request: NOT RUN',
    'CDN server/token/chunk/file request: NOT RUN',
    'Run Foundation 5/5 Regression',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 08 UI marker missing: {marker}')

# Host tests preserve earlier gates and exercise the pure Step 08 metadata parser/result contract.
test_text = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))
for marker in (
    'ExistingCoreRegressionSelfTestStillPassesTwelveOfTwelve',
    'RoundTripPassesAndLeavesStoreClean',
    'AllFiveFoundationGatesProducePass',
    'TargetAppIdIsSlayTheSpire2',
    'OkMatchingNonEmptyTicketProvesOwnership',
    'TargetAppIdRemainsSlayTheSpire2',
    'ParserExtractsDepotPlatformAndVisibleBranchManifests',
    'ParserToleratesOneWrapperLevelAndSkipsInvalidManifestIds',
    'DiscoveryResultExposesNoRawOwnershipTicketOrPicsAccessTokenValue',
    'DiscoveredSummaryReportsDepotAndManifestCounts',
):
    if marker not in test_text:
        raise SystemExit(f'ERROR: Step 08 test marker missing: {marker}')
if 'ThrowsException<' in test_text or '[DataTestMethod]' in test_text:
    raise SystemExit('ERROR: incompatible/obsolete MSTest API detected.')

patcher = Path('tools/StS2Launcher.SteamKitIosPatcher/Program.cs').read_text()
for marker in ('ExpectedVersionPrefix = "3.4.0"', 'matches.Length > 1'):
    if marker not in patcher:
        raise SystemExit(f'ERROR: SteamKit patcher guard missing: {marker}')

codemagic = Path('codemagic.yaml').read_text()
for marker in ('ios-step-08:', 'artifacts/StS2-Launcher-Step-08.ipa', 'artifacts/step08-build-summary.txt'):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Codemagic Step 08 marker missing: {marker}')

print('Step 08 source validation: PASS')
print('  Steps 01-07 regressions preserved')
print('  Target AppID fixed to 2868840')
print('  Step 08 requests PICS access metadata + product info only')
print('  Numeric depot IDs + visible branch manifest IDs are parsed')
print('  PICS access-token value is never exposed or persisted')
print('  No depot key / manifest body / CDN / chunk / file download capability added')
PY
