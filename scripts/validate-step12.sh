#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

bash scripts/validate-foundation.sh

python3 - <<'PY'
from pathlib import Path
import plistlib
import re

core_proj = Path('src/StS2Launcher.Core/StS2Launcher.Core.csproj').read_text()
ios_proj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)

if plist.get('CFBundleShortVersionString') != '0.0.40' or str(plist.get('CFBundleVersion')) != '40':
    raise SystemExit('ERROR: source Info.plist must be Step 13 version 0.0.40 (40).')
for marker in (
    '<ApplicationVersion>40</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.40</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    'Step052RemoveMacOnlyDiskArbitrationFramework',
):
    if marker not in ios_proj:
        raise SystemExit(f'ERROR: Step 12 iOS project marker missing: {marker}')
if '<PackageReference Include="SteamKit2" Version="3.4.0" />' not in core_proj:
    raise SystemExit('ERROR: SteamKit2 must remain pinned to 3.4.0.')

# Step 11 implementation stays present as the acquisition regression.
step11 = Path('src/StS2Launcher.Core/SteamResumableDepotDownloadAttempt.cs').read_text()
for marker in ('BuildResumeRelativePath(selectedDepot)', 'SteamDepotResumeValidation.ComputeAdler32Async(', 'Directory.Move(stagingPath, finalPath)', 'resumeDataPreserved'):
    if marker not in step11:
        raise SystemExit(f'ERROR: Step 11 regression marker missing: {marker}')

# Step 12.2: the iOS platform HTTP stack can surface SteamKit's bounded CDN
# cancellation as TimeoutException. Both manifest and chunk paths, including
# authenticated retries, must fail over to another bounded CDN server instead
# of abandoning the entire resumable source attempt.
timeout_catch = 'catch (TimeoutException) when (!cancellationToken.IsCancellationRequested)'
if step11.count(timeout_catch) < 4:
    raise SystemExit('ERROR: Step 12.2 must retain TimeoutException failover in Step 11 manifest/chunk initial and authenticated CDN requests.')

# Step 12.2.1: SteamKitWebRequestException derives from HttpRequestException.
# In each authenticated retry chain the derived catch must precede the base catch,
# otherwise C# reports CS0160 and Codemagic stops at host compilation.
auth_retry_anchor = 'cdnTokensByHost[server.Host] = tokenValue;'
auth_retry_positions = [m.start() for m in re.finditer(re.escape(auth_retry_anchor), step11)]
if len(auth_retry_positions) != 2:
    raise SystemExit(f'ERROR: expected exactly two authenticated CDN retry chains, found {len(auth_retry_positions)}.')
expected_retry_catches = [
    'TimeoutException',
    'TaskCanceledException',
    'SteamKitWebRequestException',
    'HttpRequestException',
    'IOException',
]
for index, pos in enumerate(auth_retry_positions, start=1):
    signatures = re.findall(r'catch \(([^ )]+)', step11[pos:])[:5]
    if signatures != expected_retry_catches:
        raise SystemExit(
            f'ERROR: authenticated CDN retry catch chain {index} has invalid ordering {signatures}; '
            f'expected {expected_retry_catches}. SteamKitWebRequestException must precede HttpRequestException.'
        )

# Step 12.4 cleanup backports the already-proven iOS TimeoutException handling
# to the still-exposed Step 09 and Step 10 regression download buttons. Their
# authenticated retries must keep the same derived-before-base catch ordering.
for legacy_name in (
    'src/StS2Launcher.Core/SteamSingleFileDownloadAttempt.cs',
    'src/StS2Launcher.Core/SteamFullDepotDownloadAttempt.cs',
):
    legacy = Path(legacy_name).read_text()
    if legacy.count(timeout_catch) < 4:
        raise SystemExit(f'ERROR: Step 12.4 must retain iOS TimeoutException failover in legacy CDN path: {legacy_name}')
    positions = [m.start() for m in re.finditer(re.escape(auth_retry_anchor), legacy)]
    if len(positions) != 2:
        raise SystemExit(f'ERROR: expected two authenticated retry chains in {legacy_name}, found {len(positions)}.')
    for index, pos in enumerate(positions, start=1):
        signatures = re.findall(r'catch \(([^ )]+)', legacy[pos:])[:5]
        if signatures != expected_retry_catches:
            raise SystemExit(
                f'ERROR: legacy authenticated retry chain {index} in {legacy_name} has invalid ordering {signatures}; '
                f'expected {expected_retry_catches}.'
            )
factory = Path('src/StS2Launcher.Core/SteamHttpClientFactory.cs').read_text()
if 'purpose == HttpClientPurpose.CMWebSocket' not in factory:
    raise SystemExit('ERROR: Step 12.2 must retain the proven CMWebSocket-only SocketsHttpHandler policy.')
if 'HttpClientPurpose.CDN' in factory:
    raise SystemExit('ERROR: Step 12.2 must not broaden SocketsHttpHandler to SteamKit CDN traffic.')

# Step 12.3: a completed manifest-specific Step 11 cache is reusable only after
# the freshly downloaded current Steam manifest directly re-proves its exact tree.
# A deliberately stale Step 12 receipt must no longer be the trust anchor for that cache.
step11_result = Path('src/StS2Launcher.Core/SteamResumableDepotDownloadResult.cs').read_text()
for marker in (
    'existingFinalVerifiedAgainstManifest = await VerifyExistingFinalAgainstManifestAsync(',
    'ValidateCommitTree(finalPath, plan)',
    'FileMatchesManifestAsync(path, file, cancellationToken)',
    'ExistingFinalVerifiedAgainstManifest: existingFinalVerifiedAgainstManifest',
):
    if marker not in step11:
        raise SystemExit(f'ERROR: Step 12.3 verified-cache regression marker missing from Step 11: {marker}')
if 'bool ExistingFinalVerifiedAgainstManifest' not in step11_result:
    raise SystemExit('ERROR: Step 12.3 Step 11 result must expose current-manifest cache verification telemetry.')

attempt = Path('src/StS2Launcher.Core/SteamManagedInstallAttempt.cs').read_text()
receipt = Path('src/StS2Launcher.Core/SteamManagedInstallReceipt.cs').read_text()
result = Path('src/StS2Launcher.Core/SteamManagedInstallResult.cs').read_text()
root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
tests = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))
maintenance = Path('src/StS2Launcher.Core/SteamDownloadCacheMaintenance.cs').read_text()

acquire_start = attempt.index('private async Task<(bool Success, SteamResumableDepotDownloadResult? Result, string? SourcePath, string? Error)> AcquireVerifiedSourceAsync(')
acquire_end = attempt.index('private static async Task<SteamManagedInstallReceipt> BuildReceiptAsync(', acquire_start)
acquire_block = attempt[acquire_start:acquire_end]
for marker in (
    'if (result.ExistingFinalVerifiedAgainstManifest)',
    'Revalidating the existing Step 11 cache against the current Steam manifest',
    'CleanupDirectory(existingPath)',
):
    if marker not in acquire_block:
        raise SystemExit(f'ERROR: Step 12.3 source-acquisition marker missing: {marker}')
if 'installedReceipt' in acquire_block:
    raise SystemExit('ERROR: Step 12.3 must not require the Step 12 install receipt to vouch for the Step 11 source cache.')

for marker in (
    'CreateSyntheticUpdateReceipt(receipt)',
    'Sha1Hex = syntheticSha1',
    'OrderBy(item => item.file.Length)',
    'SteamResumableDepotDownloadOutcome.Cancelled => SteamManagedInstallOutcome.Cancelled',
    'SteamResumableDepotDownloadOutcome.TimedOut => SteamManagedInstallOutcome.TimedOut',
    'if (receipt.ManifestId != current.ManifestId)',
    'return SteamManagedInstallState.UpdateAvailable;',
):
    if marker not in attempt:
        raise SystemExit(f'ERROR: Step 12.3 update/telemetry regression marker missing: {marker}')

for marker in (
    'public sealed class SteamManagedInstallAttempt',
    'TargetAppId = SteamOwnershipVerificationAttempt.TargetAppId',
    'new SteamContentDiscoveryAttempt(_sessionStore)',
    'SteamDepotDownloadPlanner.SelectDepot(discovery.Depots, TargetAppId)',
    'new SteamResumableDepotDownloadAttempt(_sessionStore, _outputRootDirectory)',
    'SteamManagedInstallState.NotInstalled',
    'SteamManagedInstallState.UpToDate',
    'SteamManagedInstallState.UpdateAvailable',
    'SteamManagedInstallState.RepairNeeded',
    'SteamManagedInstallAction.Install',
    'SteamManagedInstallAction.Update',
    'SteamManagedInstallAction.Repair',
    'VerifyTreeAgainstReceiptAsync',
    'ComputeSha1HexAsync',
    'Directory.Move(managedPath, backupPath)',
    'Directory.Move(stagingPath, managedPath)',
    'Directory.Move(backupPath, managedPath)',
    'PrepareRepairTestAsync',
    'PrepareUpdateStateTestAsync',
    'CreateSyntheticUpdateReceipt',
    'result.ExistingFinalVerifiedAgainstManifest',
    'sourceCacheReverified',
    'sourceNewlyDownloadedBytes',
):
    if marker not in attempt:
        raise SystemExit(f'ERROR: Step 12 implementation marker missing: {marker}')

for marker in ('SchemaVersion', 'AppId', 'DepotId', 'ManifestId', 'Branch', 'RelativePath', 'Length', 'Sha1Hex'):
    if marker not in receipt:
        raise SystemExit(f'ERROR: Step 12 receipt marker missing: {marker}')
for forbidden in ('RefreshToken', 'Password', 'Guard', 'DepotKey', 'CdnAuthToken', 'ManifestRequestCode', 'byte[]'):
    if forbidden in receipt:
        raise SystemExit(f'ERROR: Step 12 receipt may persist a Steam secret/payload: {forbidden}')

for marker in (
    'JsonSourceGenerationOptions',
    'JsonSourceGenerationMode.Metadata',
    'JsonSerializable(typeof(SteamManagedInstallReceipt))',
    'public sealed partial class SteamManagedInstallJsonContext : JsonSerializerContext',
):
    if marker not in receipt:
        raise SystemExit(f'ERROR: Step 12.1 receipt-source-generation regression marker missing: {marker}')
for marker in (
    'SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt',
    'JsonSerializer.DeserializeAsync(',
    'JsonSerializer.Deserialize(',
    'JsonSerializer.SerializeAsync(',
):
    if marker not in attempt:
        raise SystemExit(f'ERROR: Step 12.1 receipt JSON regression call-site marker missing: {marker}')
for forbidden in (
    'DeserializeAsync<SteamManagedInstallReceipt>',
    'Deserialize<SteamManagedInstallReceipt>',
    'SteamManagedInstallReceipt.JsonOptions',
):
    if forbidden in attempt or forbidden in receipt:
        raise SystemExit(f'ERROR: Step 12.1 regression must not return to reflection/options-based receipt serialization: {forbidden}')

# Step 12.4 stabilization: malformed local receipts become RepairNeeded,
# failed receipt writes cannot strand a .tmp file, cleanup failures do not mask
# the original manager result, and Step 11 resume accounting is committed only
# after a complete partial-file checksum scan.
for marker in (
    'IsReceiptStructurallyValid(receipt)',
    'receipt.AppId != TargetAppId',
    '!string.Equals(receipt.Branch, current.Branch, StringComparison.OrdinalIgnoreCase)',
    'TryDeleteFile(tempPath)',
    'Staging cleanup failed:',
    'Backup cleanup/restore failed:',
):
    if marker not in attempt:
        raise SystemExit(f'ERROR: Step 12.4 receipt/cleanup hardening marker missing: {marker}')
for marker in (
    'var scannedReusedChunkCount = 0;',
    'ulong scannedReusedBytes = 0;',
    'reusedChunkCount += scannedReusedChunkCount;',
    'A final cache that cannot be read consistently is not trustworthy.',
):
    if marker not in step11:
        raise SystemExit(f'ERROR: Step 12.4 Step 11 hardening marker missing: {marker}')

# Step 12.4.1 adds maintenance/test controls only. Cache deletion must remain
# narrowly rooted at Step11-ResumableDepot and must not reference the managed
# install or Keychain/session storage. The forced test reuses the existing
# synthetic update preparation, then clears the source cache so the next manager
# run must perform a fresh current-depot acquisition.
for marker in (
    'public const string CacheRelativePath = "Step11-ResumableDepot";',
    'Directory.Delete(cachePath, recursive: true)',
    'CacheAbsentAfterClear',
    'public bool Exists()',
):
    if marker not in maintenance:
        raise SystemExit(f'ERROR: Step 12.4.1 cache-maintenance marker missing: {marker}')
for forbidden in ('Step12-ManagedInstall', 'SteamSessionStore', 'ICredentialStore', 'Keychain', 'RefreshToken'):
    if forbidden in maintenance:
        raise SystemExit(f'ERROR: Step 12.4.1 cache helper broadened beyond Step 11 cache ownership: {forbidden}')
for marker in (
    'Clear Download Cache Only (Keep Managed Install)',
    'Prepare Fresh Download Test (Force Update + Clear Cache)',
    'await _managedInstallAttempt.PrepareUpdateStateTestAsync()',
    'Task.Run(_downloadCacheMaintenance.Clear)',
    'Managed Step 12 install: PRESERVED',
    'Saved Steam session: PRESERVED',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 12.4.1 cache-test UI marker missing: {marker}')
if 'DownloadCacheMaintenanceDeletesOnlyStep11CacheAndIsIdempotent' not in tests:
    raise SystemExit('ERROR: Step 12.4.1 must unit-test that cache clear preserves the managed-install tree and is idempotent.')

for marker in (
    'ExistingInstallPreservedUntilCommit', 'AtomicCommitCompleted', 'RollbackRestoredPreviousInstall',
    'StagingAbsentAfterResult', 'BackupAbsentAfterResult', 'SourceCacheReverifiedAgainstCurrentManifest',
    'SourceNewlyDownloadedBytes', 'INSTALL PASS —', 'UPDATE PASS —', 'REPAIR PASS —',
):
    if marker not in result:
        raise SystemExit(f'ERROR: Step 12 result marker missing: {marker}')
if 'byte[]' in result:
    raise SystemExit('ERROR: Step 12 result must not expose raw content/secret arrays.')

for marker in (
    'Step 12.4.1 — completed install/update/repair + cache regression controls',
    'Inspect + Install / Update / Repair', 'Prepare Repair Test (Corrupt One Managed File)',
    'Prepare Update Test (Stale Receipt + One Changed File Identity)', 'RunManagedInstallAsync',
    'FormatManagedInstallDetail', 'State before:', 'Action taken:', 'State after:',
    'Multi-depot app composition: NOT IMPLEMENTED', 'Compatibility inventory / Cecil / Godot / game launch: NOT RUN',
    'Run Foundation 5/5 Regression',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 12 UI marker missing: {marker}')

for marker in (
    'Step12TargetAppIdRemainsSlayTheSpire2',
    'StateClassifierDistinguishesInstallUpdateRepairAndCurrent',
    'StateClassifierTreatsMalformedOrForeignReceiptAsRepairNeeded',
    'ReceiptContainsOnlyNonSecretIntegrityMetadata',
    'ReceiptJsonUsesSourceGeneratedMetadataAndRoundTrips',
    'SyntheticUpdateReceiptForcesUpdateAndOneSourceReplacementIdentity',
    'SuccessfulResultContractIncludesAtomicReplacementProof',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 12 unit-test marker missing: {marker}')

# Step 12 must not introduce later phases.
for forbidden in (
    'GodotSharp', 'Mono.Cecil.Cil', 'SteamWorkshop', 'SteamCloudClient',
    'LaunchGame', 'StartGodot', 'WorkshopItem', 'CloudSave',
):
    if forbidden in attempt:
        raise SystemExit(f'ERROR: Step 12 broadened into a later boundary: {forbidden}')


program = Path('src/StS2Launcher.Step05.iOS/Program.cs').read_text()
app_delegate = Path('src/StS2Launcher.Step05.iOS/AppDelegate.cs').read_text()
scene_delegate = Path('src/StS2Launcher.Step05.iOS/SceneDelegate.cs').read_text()
for stale in ('Step 08:', 'STEP 06.3.1 STARTUP ERROR', 'Step 06 startup exception'):
    if stale in program or stale in app_delegate or stale in scene_delegate:
        raise SystemExit(f'ERROR: stale startup diagnostic label remains after Step 12.4 cleanup: {stale}')

codemagic = Path('codemagic.yaml').read_text()
for marker in ('ios-step-13:', 'artifacts/StS2-Launcher-Step-13.ipa', 'artifacts/step13-build-summary.txt'):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Codemagic Step 12 marker missing: {marker}')

print('Steps 01-12 regression source validation under Step 13: PASS')
print('  Steps 01-12 completed-device regressions retained under the Step 13 source tree')
print('  One direct public depot is managed as Not Installed / Up To Date / Update Available / Repair Needed')
print('  Step 11 remains the Steam source-acquisition engine and now revalidates an existing final cache directly against the current Steam manifest')
print('  iOS TimeoutException failover is retained in Step 11 and backported to the still-exposed Step 09/10 regression paths')
print('  Step 12.3 current-manifest cache reuse and Step 12.4 receipt/cleanup/resume hardening remain intact')
print('  Step 12.4.1 cache helper deletes only Step11-ResumableDepot and supports a forced fresh-download update regression')
print('  Synthetic update test stales the receipt and changes one smallest-file identity so UPDATE must replace at least one source file before atomic commit')
print('  Planned file/byte telemetry is retained when source acquisition is cancelled or times out')
print('  Install/update/repair stage a complete SHA-1 receipt-verified replacement before swap')
print('  Previous good install is preserved until commit and restored on replacement failure')
print('  Receipt JSON uses compile-time System.Text.Json metadata; no runtime constructor-name reflection path remains')
print('  Local receipt contains only non-secret app/depot/manifest/path/length/SHA-1 metadata')
print('  Multi-depot composition, compatibility inspection, Godot/runtime, Cloud and Workshop remain absent')
PY
