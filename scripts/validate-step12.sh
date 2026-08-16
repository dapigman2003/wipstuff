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

if plist.get('CFBundleShortVersionString') != '0.0.34' or str(plist.get('CFBundleVersion')) != '34':
    raise SystemExit('ERROR: source Info.plist must be Step 12.1 version 0.0.34 (34).')
for marker in (
    '<ApplicationVersion>34</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.34</ApplicationDisplayVersion>',
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

attempt = Path('src/StS2Launcher.Core/SteamManagedInstallAttempt.cs').read_text()
receipt = Path('src/StS2Launcher.Core/SteamManagedInstallReceipt.cs').read_text()
result = Path('src/StS2Launcher.Core/SteamManagedInstallResult.cs').read_text()
root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
tests = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))

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
        raise SystemExit(f'ERROR: Step 12.1 source-generated receipt JSON marker missing: {marker}')
for marker in (
    'SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt',
    'JsonSerializer.DeserializeAsync(',
    'JsonSerializer.Deserialize(',
    'JsonSerializer.SerializeAsync(',
):
    if marker not in attempt:
        raise SystemExit(f'ERROR: Step 12.1 receipt JSON call-site marker missing: {marker}')
for forbidden in (
    'DeserializeAsync<SteamManagedInstallReceipt>',
    'Deserialize<SteamManagedInstallReceipt>',
    'SteamManagedInstallReceipt.JsonOptions',
):
    if forbidden in attempt or forbidden in receipt:
        raise SystemExit(f'ERROR: Step 12.1 must not return to reflection/options-based receipt serialization: {forbidden}')

for marker in (
    'ExistingInstallPreservedUntilCommit', 'AtomicCommitCompleted', 'RollbackRestoredPreviousInstall',
    'StagingAbsentAfterResult', 'BackupAbsentAfterResult', 'INSTALL PASS —', 'UPDATE PASS —', 'REPAIR PASS —',
):
    if marker not in result:
        raise SystemExit(f'ERROR: Step 12 result marker missing: {marker}')
if 'byte[]' in result:
    raise SystemExit('ERROR: Step 12 result must not expose raw content/secret arrays.')

for marker in (
    'STEP 12.1 — AOT RECEIPT HOTFIX', 'Version 0.0.34',
    'Inspect + Install / Update / Repair', 'Prepare Repair Test (Corrupt One Managed File)',
    'Prepare Update-State Test (Stale Local Receipt Only)', 'RunManagedInstallAsync',
    'FormatManagedInstallDetail', 'State before:', 'Action taken:', 'State after:',
    'Multi-depot app composition: NOT IMPLEMENTED', 'Compatibility inventory / Cecil / Godot / game launch: NOT RUN',
    'Run Foundation 5/5 Regression',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 12 UI marker missing: {marker}')

for marker in (
    'Step12TargetAppIdRemainsSlayTheSpire2',
    'StateClassifierDistinguishesInstallUpdateRepairAndCurrent',
    'ReceiptContainsOnlyNonSecretIntegrityMetadata',
    'ReceiptJsonUsesSourceGeneratedMetadataAndRoundTrips',
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

codemagic = Path('codemagic.yaml').read_text()
for marker in ('ios-step-12-1:', 'artifacts/StS2-Launcher-Step-12.1.ipa', 'artifacts/step12.1-build-summary.txt'):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Codemagic Step 12 marker missing: {marker}')

print('Step 12.1 source validation: PASS')
print('  Steps 01-11 regressions retained')
print('  One direct public depot is managed as Not Installed / Up To Date / Update Available / Repair Needed')
print('  Step 11 remains the verified Steam source-acquisition engine')
print('  Install/update/repair stage a complete SHA-1 receipt-verified replacement before swap')
print('  Previous good install is preserved until commit and restored on replacement failure')
print('  Receipt JSON uses compile-time System.Text.Json metadata; no runtime constructor-name reflection path remains')
print('  Local receipt contains only non-secret app/depot/manifest/path/length/SHA-1 metadata')
print('  Multi-depot composition, compatibility inspection, Godot/runtime, Cloud and Workshop remain absent')
PY
