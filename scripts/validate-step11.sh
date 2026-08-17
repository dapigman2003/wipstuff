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

if plist.get('CFBundleShortVersionString') != '0.0.32' or str(plist.get('CFBundleVersion')) != '32':
    raise SystemExit('ERROR: source Info.plist must be Step 11 version 0.0.32 (32).')
if '<PackageReference Include="SteamKit2" Version="3.4.0" />' not in core_proj:
    raise SystemExit('ERROR: SteamKit2 must remain pinned to 3.4.0.')
for marker in (
    '<ApplicationVersion>32</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.32</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    'Step052RemoveMacOnlyDiskArbitrationFramework',
):
    if marker not in ios_proj:
        raise SystemExit(f'ERROR: iOS project marker missing: {marker}')

# Proven prerequisite boundaries remain present.
regression_files = {
    'Step 06 auth': ('src/StS2Launcher.Core/SteamAuthenticationAttempt.cs', ['BeginAuthSessionViaCredentialsAsync', 'IsPersistentSession = true']),
    'Step 06.3 session': ('src/StS2Launcher.Core/SteamSessionResumeAttempt.cs', ['_sessionStore.Load()', 'SteamPersistentLogOnDetails.Create(']),
    'Step 07 ownership': ('src/StS2Launcher.Core/SteamOwnershipVerificationAttempt.cs', ['public const uint TargetAppId = 2868840;', 'GetAppOwnershipTicket(TargetAppId)']),
    'Step 08 discovery': ('src/StS2Launcher.Core/SteamContentDiscoveryAttempt.cs', ['PICSGetAccessTokens(TargetAppId, package: null)', 'PICSGetProductInfo(']),
    'Step 09 single file': ('src/StS2Launcher.Core/SteamSingleFileDownloadAttempt.cs', ['DownloadManifestAsync(', 'DownloadDepotChunkAsync(', 'Step09-SingleFile']),
    'Step 10 full depot': ('src/StS2Launcher.Core/SteamFullDepotDownloadAttempt.cs', ['foreach (var file in plan.Files)', 'CleanupStaging()', 'Directory.Move(stagingPath, finalPath)', 'Step10-FullDepot']),
}
for name, (path, markers) in regression_files.items():
    text = Path(path).read_text()
    for marker in markers:
        if marker not in text:
            raise SystemExit(f'ERROR: {name} regression marker missing: {marker}')

# Step 10 must remain cleanup-on-cancel and must not itself become resumable.
step10 = Path('src/StS2Launcher.Core/SteamFullDepotDownloadAttempt.cs').read_text()
for forbidden in ('SteamDepotResumeValidation', '.step11.part', 'BuildResumeRelativePath', 'ResumeStagingFoundAtStart'):
    if forbidden in step10:
        raise SystemExit(f'ERROR: Step 10 regression was modified into Step 11 behavior: {forbidden}')

resume_attempt = Path('src/StS2Launcher.Core/SteamResumableDepotDownloadAttempt.cs').read_text()
resume_validation = Path('src/StS2Launcher.Core/SteamDepotResumeValidation.cs').read_text()
resume_result = Path('src/StS2Launcher.Core/SteamResumableDepotDownloadResult.cs').read_text()
progress = Path('src/StS2Launcher.Core/SteamDepotDownloadProgress.cs').read_text()
root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
tests = '\n'.join(p.read_text() for p in Path('tests/StS2Launcher.Core.Tests').glob('*.cs'))

for marker in (
    'public sealed class SteamResumableDepotDownloadAttempt',
    'TargetAppId = SteamOwnershipVerificationAttempt.TargetAppId',
    'SteamDepotDownloadPlanner.SelectDepot(depots, TargetAppId)',
    'GetDepotDecryptionKey(selectedDepot.DepotId, TargetAppId)',
    'GetManifestRequestCode(', 'DownloadManifestAsync(', 'DownloadDepotChunkAsync(',
    'BuildResumeRelativePath(selectedDepot)', 'Step11-ResumableDepot', '".resume"', '.step11.part',
    'FileMatchesManifestAsync(stagedFilePath, file, token)',
    'ChunkMatchesManifestAsync(existingPart, chunk, token)',
    'SteamDepotResumeValidation.ComputeAdler32Async(', 'checksum == chunk.Checksum',
    'reusedVerifiedFileCount++', 'reusedChunkCount++', 'newlyDownloadedChunkCount++',
    'await output.FlushAsync(token)', 'ValidateCommitTree(stagingPath, plan)',
    'Directory.Move(stagingPath, finalPath)', 'resumeDataPreserved',
):
    if marker not in resume_attempt:
        raise SystemExit(f'ERROR: Step 11 implementation marker missing: {marker}')

for marker in ('AdlerMod = 65521', 'ComputeAdler32(ReadOnlySpan<byte> data)', 'ComputeAdler32Async(', 'return a | (b << 16)'):
    if marker not in resume_validation:
        raise SystemExit(f'ERROR: Step 11 checksum marker missing: {marker}')

if 'Resuming = 5' not in progress:
    raise SystemExit('ERROR: Step 11 progress must expose the Resuming phase without renumbering Step 10 phases.')

if 'byte[]' in resume_result:
    raise SystemExit('ERROR: Step 11 result must not expose raw byte arrays.')
for forbidden in ('string? DepotKey', 'byte[] DepotKey', 'ulong ManifestRequestCode', 'string? CdnAuthToken', 'ulong PicsAccessToken'):
    if forbidden in resume_result:
        raise SystemExit(f'ERROR: Step 11 result exposes a secret: {forbidden}')
for marker in (
    'ReusedVerifiedFileCount', 'ReusedChunkCount', 'ReusedBytes', 'NewlyDownloadedChunkCount',
    'NewlyDownloadedBytes', 'InvalidResumeFileCount', 'InvalidResumeChunkCount',
    'ResumeStagingFoundAtStart', 'ResumeDataPreserved', 'FinalDirectoryCommitted',
    'RESUME PASS —', 'RESUME INTERRUPTED — staging preserved',
):
    if marker not in resume_result:
        raise SystemExit(f'ERROR: Step 11 result telemetry missing: {marker}')

for forbidden in (
    'InstalledManifestIDs', 'RepairInstall', 'UpdateInstall', 'DownloadAllDepots', 'DownloadAppAsync',
    'RequestFreeLicense', 'GodotSharp', 'SteamWorkshop', 'SteamCloudClient', 'Mono.Cecil.Cil',
):
    if forbidden in resume_attempt:
        raise SystemExit(f'ERROR: Step 11 broadened into a later boundary: {forbidden}')

for marker in (
    'STEP 11 — INTERRUPTED-DOWNLOAD RESUME', 'Version 0.0.32',
    'APP ID 2868840 • ONE PUBLIC DEPOT • CRASH-SAFE RESUME + ATOMIC COMMIT',
    'Step 10 — minimal full-depot downloader', 'Resume / Download One Public Depot',
    'RunResumableDepotDownloadAsync', 'FormatResumableDepotDownloadDetail',
    'Resume staging found at start:', 'Reused Adler-32-valid chunks:',
    'New chunks downloaded this run:', 'Resume data preserved after result:',
    'Manifest delta/update migration: NOT IMPLEMENTED',
    'Update/install/repair orchestration: NOT IMPLEMENTED',
    'Run Foundation 5/5 Regression',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 11 UI marker missing: {marker}')

for marker in (
    'Step10TargetAppIdRemainsSlayTheSpire2', 'Step10ResultExposesNoRawSecretOrDownloadedByteArrays',
    'Step11TargetAppIdRemainsSlayTheSpire2', 'Adler32MatchesStandardKnownVector',
    'Adler32StreamingMatchesSpanImplementation', 'Step11ProgressAddsResumingPhaseWithoutChangingStep10Values',
    'Step11ResultExposesOnlyResumeTelemetryNotSecretsOrPayloads',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: unit-test marker missing: {marker}')
if 'ThrowsException<' in tests or '[DataTestMethod]' in tests:
    raise SystemExit('ERROR: incompatible/obsolete MSTest API detected.')

codemagic = Path('codemagic.yaml').read_text()
for marker in ('ios-step-11:', 'artifacts/StS2-Launcher-Step-11.ipa', 'artifacts/step11-build-summary.txt'):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Codemagic Step 11 marker missing: {marker}')

print('Step 11 source validation: PASS')
print('  Steps 01-10 regression code retained')
print('  Target AppID fixed to 2868840')
print('  Deterministic depot+manifest resume staging survives interruption')
print('  Complete staged files are re-proven by SHA-1')
print('  Partial chunks are revalidated by manifest Adler-32 and only missing/corrupt chunks are downloaded')
print('  Final depot remains hidden until one atomic staging-directory commit')
print('  Steam keys/tokens/payloads are not persisted in resume telemetry')
print('  Update/install/repair, manifest delta migration, multi-depot app install, Godot, Cloud, Workshop remain absent')
PY
