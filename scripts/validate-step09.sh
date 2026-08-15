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

if plist.get('CFBundleShortVersionString') != '0.0.30' or str(plist.get('CFBundleVersion')) != '30':
    raise SystemExit('ERROR: source Info.plist must be Step 09 version 0.0.30 (30).')
if '<PackageReference Include="SteamKit2" Version="3.4.0" />' not in core_proj:
    raise SystemExit('ERROR: SteamKit2 must remain pinned to 3.4.0.')
for marker in (
    '<ApplicationVersion>30</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.30</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    'Step052RemoveMacOnlyDiskArbitrationFramework',
):
    if marker not in ios_proj:
        raise SystemExit(f'ERROR: iOS project marker missing: {marker}')

# Steps 06-06.3.1 regressions.
auth = Path('src/StS2Launcher.Core/SteamAuthenticationAttempt.cs').read_text()
for marker in (
    'BeginAuthSessionViaCredentialsAsync', 'PollingWaitForResultAsync(token)',
    'IsPersistentSession = true', 'SteamPersistentLogOnDetails.Create(',
    '_sessionStore.Save(new SteamSavedSession(', 'MobileApprovalCompleted',
):
    if marker not in auth:
        raise SystemExit(f'ERROR: authentication regression marker missing: {marker}')
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

# Step 07 remains exact ownership proof only.
ownership = Path('src/StS2Launcher.Core/SteamOwnershipVerificationAttempt.cs').read_text()
decision = Path('src/StS2Launcher.Core/SteamOwnershipDecision.cs').read_text()
ownership_result = Path('src/StS2Launcher.Core/SteamOwnershipVerificationResult.cs').read_text()
for marker in (
    'public const uint TargetAppId = 2868840;', 'GetAppOwnershipTicket(TargetAppId)',
    'AppOwnershipTicketCallback', 'SteamOwnershipDecision.EvaluateTicket(',
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

# Step 08 remains metadata-only when run independently.
discovery_attempt = Path('src/StS2Launcher.Core/SteamContentDiscoveryAttempt.cs').read_text()
parser = Path('src/StS2Launcher.Core/SteamContentDiscoveryParser.cs').read_text()
discovery_result = Path('src/StS2Launcher.Core/SteamContentDiscoveryResult.cs').read_text()
models = Path('src/StS2Launcher.Core/SteamDepotDiscovery.cs').read_text()
for marker in (
    'GetAppOwnershipTicket(TargetAppId)', 'PICSGetAccessTokens(TargetAppId, package: null)',
    'PICSGetProductInfo(', 'SteamContentDiscoveryParser.Parse(appInfo.KeyValues)',
    'callback.ResponsePending', 'tokenCallback.AppTokensDenied.Contains(TargetAppId)',
):
    if marker not in discovery_attempt:
        raise SystemExit(f'ERROR: Step 08 discovery regression marker missing: {marker}')
for forbidden in (
    'GetDepotDecryptionKey', 'DepotManifest', 'CDN.Client', 'GetServersForSteamPipe',
    'GetManifestRequestCode', 'DownloadManifest', 'DownloadDepotChunk', 'File.Write', 'FileStream',
):
    if forbidden in discovery_attempt:
        raise SystemExit(f'ERROR: standalone Step 08 operation broadened unexpectedly: {forbidden}')
for marker in (
    'uint.TryParse(depotNode.Name', 'Child(depotNode, "config")', 'Child(depotNode, "manifests")',
    'Child(branchNode, "gid")', 'ulong.TryParse(rawManifestId', 'Child(depotNode, "depotfromapp")',
):
    if marker not in parser:
        raise SystemExit(f'ERROR: Step 08 parser marker missing: {marker}')
for marker in ('SteamManifestDiscovery', 'SteamDepotDiscovery', 'DepotFromAppId', 'OsList', 'OsArch', 'Language', 'Manifests'):
    if marker not in models:
        raise SystemExit(f'ERROR: Step 08 model marker missing: {marker}')
if 'byte[]' in discovery_result or 'ulong AccessToken' in discovery_result or 'ulong PicsAccessToken' in discovery_result:
    raise SystemExit('ERROR: Step 08 result exposes secret/raw payload data.')

# Step 09: one bounded content test after re-proving every prerequisite.
attempt = Path('src/StS2Launcher.Core/SteamSingleFileDownloadAttempt.cs').read_text()
selector = Path('src/StS2Launcher.Core/SteamSingleFileTargetSelector.cs').read_text()
result = Path('src/StS2Launcher.Core/SteamSingleFileDownloadResult.cs').read_text()

for marker in (
    'public const uint TargetAppId = SteamOwnershipVerificationAttempt.TargetAppId;',
    '_sessionStore.Load()', 'SteamPersistentLogOnDetails.Create(', 'GetAppOwnershipTicket(TargetAppId)',
    'SteamOwnershipDecision.EvaluateTicket(', 'PICSGetAccessTokens(TargetAppId, package: null)',
    'PICSGetProductInfo(', 'SteamContentDiscoveryParser.Parse(appInfo.KeyValues)',
    'SteamSingleFileTargetSelector.SelectDepot(depots, TargetAppId)',
    'GetDepotDecryptionKey(selectedDepot.DepotId, TargetAppId)',
    'GetManifestRequestCode(', 'GetServersForSteamPipe()', 'new Client(steamClient)',
    'DownloadManifestAsync(', 'DownloadDepotChunkAsync(', 'GetCDNAuthToken(TargetAppId, depotId, host)',
    'SteamSingleFileTargetSelector.SelectFile(manifest)', 'SHA1.HashData(fileBytes)',
    'CryptographicOperations.FixedTimeEquals(', 'File.WriteAllBytesAsync(tempPath, fileBytes, token)',
    'File.Move(tempPath, finalPath, overwrite: true)', 'Step09-SingleFile',
):
    if marker not in attempt:
        raise SystemExit(f'ERROR: Step 09 implementation marker missing: {marker}')

for marker in (
    'MaxTargetFileBytes = 2UL * 1024UL * 1024UL',
    '.Where(depot => !depot.DepotFromAppId.HasValue)',
    'string.Equals(manifest.Branch, "public"',
    '!file.Flags.HasFlag(EDepotFileFlag.Directory)',
    'file.TotalSize > 0 && file.TotalSize <= maxBytes',
    'file.Chunks.Count > 0', 'IsSafeRelativePath(file.FileName)', 'ChunksFitFile',
    'string.Equals(value, "macos"',
):
    if marker not in selector:
        raise SystemExit(f'ERROR: Step 09 selection/bounds marker missing: {marker}')

# The result contract may expose status booleans/EResult only, never keys/tokens/request codes/raw bytes.
if 'byte[]' in result:
    raise SystemExit('ERROR: Step 09 result must not expose downloaded bytes or secret byte arrays.')
for forbidden in ('string? DepotKey', 'byte[] DepotKey', 'ulong ManifestRequestCode', 'string? CdnAuthToken', 'ulong PicsAccessToken'):
    if forbidden in result:
        raise SystemExit(f'ERROR: Step 09 result exposes a secret: {forbidden}')
for marker in (
    'SteamSingleFileDownloadOutcome.Downloaded', 'DepotKeyRequested', 'DepotKeyReceived',
    'ManifestRequestCodeRequested', 'ManifestRequestCodeReceived', 'ManifestDownloaded',
    'SelectedFileBytes', 'ChunksDownloaded', 'DownloadedUncompressedBytes',
    'FileHashMatched', 'FileWritten', 'OutputRelativePath', 'SINGLE-FILE PASS —',
):
    if marker not in result:
        raise SystemExit(f'ERROR: Step 09 result telemetry marker missing: {marker}')

# Guard against accidentally growing this into Step 10+.
for forbidden in (
    'Parallel.ForEach', 'Parallel.ForEachAsync', 'ConcurrentQueue', 'Channel.Create',
    'DownloadAppAsync', 'DownloadDepotAsync', 'InstalledManifestIDs', 'RequestFreeLicense',
    'RepairInstall', 'UpdateInstall', 'ResumeDownload', 'Godot', 'Workshop', 'SteamCloud',
):
    if forbidden in attempt:
        raise SystemExit(f'ERROR: Step 09 broadened into a later boundary: {forbidden}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 09 — ONE CONTROLLED SMALL FILE', 'Version 0.0.30',
    'APP ID 2868840 • ONE FILE ≤ 2 MiB • SHA-1 VERIFIED',
    'Discover StS2 Depots + Manifests', 'Download One Small StS2 File',
    'RunSingleFileDownloadAsync', 'FormatSingleFileDownloadDetail',
    'PICS access-token value display/logging/persistence: NONE',
    'Depot-key value display/logging/persistence: NONE',
    'Manifest request-code value display/logging/persistence: NONE',
    'CDN auth-token value display/logging/persistence: NONE',
    'Manifest body persistence: NONE', 'Chunk cache/partial-file persistence: NONE',
    'Full-depot queue: NOT IMPLEMENTED', 'Resume/update/install/repair: NOT IMPLEMENTED',
    'Run Foundation 5/5 Regression',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 09 UI marker missing: {marker}')

# Unit tests retain older gates and cover new pure policies/result secrecy.
test_text = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))
for marker in (
    'ExistingCoreRegressionSelfTestStillPassesTwelveOfTwelve', 'RoundTripPassesAndLeavesStoreClean',
    'AllFiveFoundationGatesProducePass', 'OkMatchingNonEmptyTicketProvesOwnership',
    'ParserExtractsDepotPlatformAndVisibleBranchManifests', 'DepotFromAppId',
    'Step09TargetAppIdRemainsSlayTheSpire2', 'ControlledFileCapIsExactlyTwoMiB',
    'DepotSelectorPrefersDirectPublicMacosDepot', 'DepotSelectorRequiresVisiblePublicManifest',
    'ManifestPathsRejectTraversalAndRootedPaths', 'Step09ResultExposesNoRawSecretOrDownloadedByteArrays',
):
    if marker not in test_text:
        raise SystemExit(f'ERROR: test marker missing: {marker}')
if 'ThrowsException<' in test_text or '[DataTestMethod]' in test_text:
    raise SystemExit('ERROR: incompatible/obsolete MSTest API detected.')

patcher = Path('tools/StS2Launcher.SteamKitIosPatcher/Program.cs').read_text()
for marker in ('ExpectedVersionPrefix = "3.4.0"', 'matches.Length > 1'):
    if marker not in patcher:
        raise SystemExit(f'ERROR: SteamKit patcher guard missing: {marker}')

codemagic = Path('codemagic.yaml').read_text()
for marker in ('ios-step-09:', 'artifacts/StS2-Launcher-Step-09.ipa', 'artifacts/step09-build-summary.txt'):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Codemagic Step 09 marker missing: {marker}')

print('Step 09 source validation: PASS')
print('  Steps 01-08 regressions preserved')
print('  Target AppID fixed to 2868840')
print('  Step 09 selects one direct public depot, preferring macOS metadata')
print('  Exactly one safe regular file <= 2 MiB is selected from one manifest')
print('  Depot/PICS/CDN secret values remain in memory and are excluded from result telemetry')
print('  Final file is SHA-1 verified before atomic persistence')
print('  No full-depot queue, resume, update/install/repair, Godot, Cloud, or Workshop capability added')
PY
