#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# First prove that the completed Steps 01-12 foundation/content-manager guards
# are still present under the new app version/workflow.
bash scripts/validate-step12.sh

python3 - <<'PY'
from pathlib import Path
import os
import plistlib

parent_mode = os.environ.get('STS2_VALIDATE_AS_PARENT') == '1'

inspection_path = Path('src/StS2Launcher.Core/SteamOfflineInstallInspection.cs')
result_path = Path('src/StS2Launcher.Core/SteamOfflineInstallResult.cs')
progress_path = Path('src/StS2Launcher.Core/SteamOfflineInstallProgress.cs')
root_path = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs')
tests_path = Path('tests/StS2Launcher.Core.Tests/SteamOfflineInstallTests.cs')

for path in (inspection_path, result_path, progress_path, tests_path):
    if not path.exists():
        raise SystemExit(f'ERROR: Step 13 source file missing: {path}')

inspection = inspection_path.read_text()
result = result_path.read_text()
progress = progress_path.read_text()
root = root_path.read_text()
tests = tests_path.read_text()

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if not parent_mode:
    if plist.get('CFBundleShortVersionString') != '0.0.40' or str(plist.get('CFBundleVersion')) != '40':
        raise SystemExit('ERROR: standalone Step 13 must be version 0.0.40 (40).')
else:
    build = int(str(plist.get('CFBundleVersion') or '0'))
    if build < 40:
        raise SystemExit('ERROR: later-step Step 13 regression validation requires build version >= 40.')

# The Step 13 implementation is local-only by construction. It accepts only an
# output-root path and must not acquire a session/network client dependency.
for marker in (
    'public sealed class SteamOfflineInstallInspection',
    'public const uint TargetAppId = SteamManagedInstallAttempt.TargetAppId;',
    'public const string ManagedRootRelativePath = "Step12-ManagedInstall";',
    'SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt',
    'Directory.EnumerateFiles(managedPath, "*", SearchOption.AllDirectories)',
    'SteamSingleFileTargetSelector.IsSafeRelativePath',
    'SHA1.Create()',
    'ComputeHashAsync(stream, token)',
    'actual.Count != expected.Count',
    'SteamSessionConsulted: false',
    'NetworkAccessAttempted: false',
    'OnlineManifestFreshnessKnown: false',
    'SteamOfflineInstallState.OnlineSetupRequired',
    'SteamOfflineInstallState.OfflineReady',
    'SteamOfflineInstallState.RepairRequired',
):
    if marker not in inspection:
        raise SystemExit(f'ERROR: Step 13 local-inspection marker missing: {marker}')

for forbidden in (
    'SteamSessionStore',
    'SteamClient',
    'SteamKit2',
    'HttpClient',
    'ClientWebSocket',
    'SocketsHttpHandler',
    'SteamContentDiscoveryAttempt',
    'SteamResumableDepotDownloadAttempt',
    'SteamOwnershipVerificationAttempt',
    '.Connect(',
    'PICSGet',
    'GetAppOwnershipTicket',
):
    if forbidden in inspection:
        raise SystemExit(f'ERROR: Step 13 local inspector gained a Steam/network dependency: {forbidden}')

for marker in (
    'OnlineSetupRequired = 1',
    'OfflineReady = 2',
    'RepairRequired = 3',
    'ExactManagedTreeVerified',
    'SteamSessionConsulted',
    'NetworkAccessAttempted',
    'OnlineManifestFreshnessKnown',
    'OFFLINE READY PASS',
):
    if marker not in result:
        raise SystemExit(f'ERROR: Step 13 result-contract marker missing: {marker}')

for marker in (
    'SteamOfflineInstallPhase.Locating',
    'SteamOfflineInstallPhase.ReadingReceipt',
    'SteamOfflineInstallPhase.VerifyingFiles',
):
    # enum members are declared without qualification, but usages must exist in inspection
    member = marker.split('.')[-1]
    if member not in progress or marker not in inspection:
        raise SystemExit(f'ERROR: Step 13 progress marker missing: {marker}')

ui_markers = [
    'Verify Offline-Ready Install (Local Only)',
    'RunOfflineInstallInspectionAsync',
    'OFFLINE STATE: NOT CHECKED',
    'Steam session consulted:',
    'Network access attempted by Step 13 check:',
    'Online manifest freshness known:',
    'Game launch / compatibility preparation: NOT IMPLEMENTED',
    'Step 12.4.1 — completed install/update/repair + cache regression controls',
    'Run Foundation 5/5 Regression',
]
if not parent_mode:
    ui_markers.extend([
        'STEP 13 — OFFLINE LAUNCHER STATE',
        'Version 0.0.40',
    ])
for marker in ui_markers:
    if marker not in root:
        raise SystemExit(f'ERROR: Step 13 UI/regression marker missing: {marker}')

for marker in (
    'OfflineInspectorProvesReadyFromLocalReceiptAndHashesOnly',
    'OfflineInspectorRequiresOnlineSetupWhenManagedInstallIsAbsent',
    'OfflineInspectorRejectsCorruptOrUnexpectedManagedContent',
    'OfflineInspectorRejectsForeignReceiptWithoutContactingSteam',
    'OfflineResultContractExplicitlySeparatesLocalReadinessFromOnlineFreshness',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 13 host-test marker missing: {marker}')

# Step 13.0.1 compile hotfix guard: Enumerable.Sum has no UInt64 selector overload,
# so a lambda explicitly returning ulong can become ambiguous between float/decimal.
# Keep the expected-byte calculation on the compile-safe UInt64 Aggregate path.
if 'Values.Sum(bytes => (ulong)bytes.Length)' in tests:
    raise SystemExit('ERROR: ambiguous UInt64 Enumerable.Sum selector returned to SteamOfflineInstallTests.')
if 'Values.Aggregate(0UL, (total, bytes) => total + (ulong)bytes.Length)' not in tests:
    raise SystemExit('ERROR: Step 13 offline byte-count host test must use the compile-safe UInt64 Aggregate expression.')

# Step 13 must not silently become game launch / compatibility / later Steam features.
for forbidden in (
    'GodotSharp',
    'Mono.Cecil',
    'LaunchGame',
    'StartGodot',
    'SteamWorkshop',
    'SteamCloud',
    'WorkshopItem',
    'CloudSave',
):
    if forbidden in inspection or forbidden in result or forbidden in progress:
        raise SystemExit(f'ERROR: Step 13 broadened into a later boundary: {forbidden}')

if not parent_mode:
    codemagic = Path('codemagic.yaml').read_text()
    for marker in (
        'ios-step-13:',
        'Step 13 - offline launcher state',
        'artifacts/StS2-Launcher-Step-13.ipa',
        'artifacts/step13-build-summary.txt',
    ):
        if marker not in codemagic:
            raise SystemExit(f'ERROR: Step 13 Codemagic marker missing: {marker}')

print('Step 13 offline-launcher-state regression validation: PASS' if parent_mode else 'Step 13 offline-launcher-state source validation: PASS')
print('  Steps 01-12 regression guards retained')
print('  Local state is derived only from the existing Step 12 managed directory + source-generated receipt')
print('  Exact local file set, lengths and SHA-1 hashes are verified before OfflineReady')
print('  Step 13 inspector has no Steam session/client/HTTP/WebSocket dependency')
print('  Online manifest freshness is explicitly unknown while offline')
print('  Step 13 itself adds no game launch, compatibility inventory, Godot, Cloud or Workshop capability')
PY
