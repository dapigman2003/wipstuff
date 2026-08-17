#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Step 15 is the first accelerated multi-gate subsystem release. Every already-
# proven Step 01-14 boundary remains a regression guard.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step14.sh

python3 - <<'PY'
from pathlib import Path
import plistlib
import re

required_paths = [
    Path('src/StS2Launcher.Core/GodotFoundationGate.cs'),
    Path('src/StS2Launcher.Core/GodotFoundationGateResult.cs'),
    Path('src/StS2Launcher.Core/GodotFoundationSummary.cs'),
    Path('src/StS2Launcher.Core/GodotFoundationGateSequence.cs'),
    Path('src/StS2Launcher.Step05.iOS/Platform/GodotStep15NativeBridge.cs'),
    Path('native/step15/godot_module/sts2_ios_host/config.py'),
    Path('native/step15/godot_module/sts2_ios_host/SCsub'),
    Path('native/step15/godot_module/sts2_ios_host/register_types.h'),
    Path('native/step15/godot_module/sts2_ios_host/register_types.cpp'),
    Path('native/step15/godot_module/sts2_ios_host/step15_ios_host_bridge.mm'),
    Path('native/step15/smoke_project/project.godot'),
    Path('native/step15/smoke_project/Main.tscn'),
    Path('native/step15/smoke_project/Step15Smoke.gd'),
    Path('tests/StS2Launcher.Core.Tests/GodotFoundationGateTests.cs'),
    Path('scripts/build-godot-step15.sh'),
    Path('scripts/build-step15.sh'),
    Path('scripts/verify-step15-ipa.sh'),
    Path('docs/STEP-15-TEST.md'),
]
for path in required_paths:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 15 artifact missing: {path}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if plist.get('CFBundleShortVersionString') != '0.0.42' or str(plist.get('CFBundleVersion')) != '42':
    raise SystemExit('ERROR: Step 15 must be version 0.0.42 (42).')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
for marker in (
    '<ApplicationVersion>42</ApplicationVersion>',
    '<ApplicationDisplayVersion>0.0.42</ApplicationDisplayVersion>',
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
    "Condition=\"Exists('NativeBuild/libgodot-step15.a')\"",
    '<NativeReference Include="NativeBuild/libgodot-step15.a">',
    '<Kind>Static</Kind>',
    '<ForceLoad>true</ForceLoad>',
    '<SmartLink>false</SmartLink>',
    '<IsCxx>true</IsCxx>',
    'MetalFX',
    'MetalKit',
):
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 15 project marker missing: {marker}')
if 'DiskArbitration' not in csproj or '<_LinkerFrameworks Remove="DiskArbitration" />' not in csproj:
    raise SystemExit('ERROR: proven Step 05 DiskArbitration filter was not preserved.')
if 'Mono.Cecil' in csproj:
    raise SystemExit('ERROR: Step 15 must not add Mono.Cecil to the runtime project.')

bridge = Path('native/step15/godot_module/sts2_ios_host/step15_ios_host_bridge.mm').read_text()
for marker in (
    'extern int apple_embedded_main(int argc, char **argv);',
    'sts2_step15_get_engine_version',
    'GODOT_VERSION_NUMBER "-" GODOT_VERSION_STATUS',
    'sts2_step15_start',
    'GDTViewController',
    'g_view.useCADisplayLink = YES;',
    '[GDTAppDelegateService sts2_setEmbeddedViewController:g_controller];',
    'UIWindow *host_window = parent.view.window ?: container.window;',
    '[app_delegate respondsToSelector:@selector(setWindow:)]',
    'return 15;',
    'return 16;',
    '[(id)app_delegate setWindow:host_window];',
    '[g_view stopRendering];',
    '[g_view startRendering];',
    'hasFinishedSetup',
    'renderingLayer',
    '@"Metal"',
    'sts2_step15_render_ready.txt',
    'sts2_step15_touch_ready.txt',
    'UIApplicationDidEnterBackgroundNotification',
    'UIApplicationWillEnterForegroundNotification',
    'on_enter_background()',
    'on_exit_background()',
    'on_focus_out()',
    'on_focus_in()',
):
    if marker not in bridge:
        raise SystemExit(f'ERROR: Step 15 native-host marker missing: {marker}')

for forbidden in (
    'SlayTheSpire2.app',
    'sts2.dll',
    'libfmod',
    'fmodstudio',
    'spine_godot',
    'Mono.Cecil',
    'SteamKit2',
    'SteamClient',
    'HttpClient',
):
    if forbidden in bridge:
        raise SystemExit(f'ERROR: Step 15 native bridge broadened into game/Steam/Cecil work: {forbidden}')

managed_bridge = Path('src/StS2Launcher.Step05.iOS/Platform/GodotStep15NativeBridge.cs').read_text()
for marker in (
    'DllImport(InternalLibrary, EntryPoint = "sts2_step15_get_engine_version")',
    'DllImport(InternalLibrary, EntryPoint = "sts2_step15_start")',
    'DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_metal_layer_ready")',
    'DllImport(InternalLibrary, EntryPoint = "sts2_step15_touch_marker_ready")',
    'private const string InternalLibrary = "__Internal";',
):
    if marker not in managed_bridge:
        raise SystemExit(f'ERROR: Step 15 managed/native bridge marker missing: {marker}')

build_godot = Path('scripts/build-godot-step15.sh').read_text()
for marker in (
    'GODOT_TAG="4.5.1-stable"',
    'git clone --depth 1 --branch "$GODOT_TAG"',
    'int sts2_godot_template_main_disabled(int argc, char *argv[]) {',
    "text.count('int apple_embedded_main(int argc, char **argv) {') != 1",
    'godot-embedded-view-controller-service-patch=v1',
    '+ (void)sts2_setEmbeddedViewController:(GDTViewController *)viewController;',
    'mainViewController = viewController;',
    'platform=ios',
    'target=template_release',
    'arch=arm64',
    'metal=yes',
    'vulkan=no',
    'opengl3=yes',
    'lto=none',
    '_sts2_step15_get_engine_version',
    "grep -F 'apple_embedded_main' \"$symbols_file\" >/dev/null",
):
    if marker not in build_godot:
        raise SystemExit(f'ERROR: pinned Godot build marker missing: {marker}')
if 'generate_bundle=yes' in build_godot:
    raise SystemExit('ERROR: Step 15 should build only the static Godot archive, not a competing Godot app bundle.')
if 'nm -gU "$lib" 2>/dev/null | grep -q' in build_godot:
    raise SystemExit('ERROR: Step 15 archive validation must not use early-exit grep -q pipelines under pipefail.')
if "grep -q '_apple_embedded_main'" in build_godot:
    raise SystemExit('ERROR: Step 15 must not assume apple_embedded_main has unmangled C linkage.')

smoke_project = Path('native/step15/smoke_project/project.godot').read_text()
smoke_scene = Path('native/step15/smoke_project/Main.tscn').read_text()
smoke_script = Path('native/step15/smoke_project/Step15Smoke.gd').read_text()
for marker in (
    'config/name="StS2 Launcher Step 15 Smoke"',
    'run/main_scene="res://Main.tscn"',
    'renderer/rendering_method="mobile"',
    'rendering_device/driver.ios="metal"',
):
    if marker not in smoke_project:
        raise SystemExit(f'ERROR: Step 15 smoke-project marker missing: {marker}')
for marker in ('GODOT 4.5.1 / METAL', 'STEP 15 SMOKE SCENE'):
    if marker not in smoke_scene:
        raise SystemExit(f'ERROR: Step 15 smoke-scene marker missing: {marker}')
for marker in (
    'sts2_step15_render_ready.txt',
    'sts2_step15_touch_ready.txt',
    'InputEventScreenTouch',
    '_write_marker(RENDER_MARKER, "ready")',
):
    if marker not in smoke_script:
        raise SystemExit(f'ERROR: Step 15 smoke-script marker missing: {marker}')

# Project-owned smoke content must remain standalone and non-proprietary.
for path in Path('native/step15').rglob('*'):
    if not path.is_file():
        continue
    text = path.read_text(errors='ignore')
    for forbidden in ('SlayTheSpire2.app', 'sts2.dll', 'libfmod.dylib', 'libfmodstudio.dylib', 'libspine_godot'):
        if forbidden in text:
            raise SystemExit(f'ERROR: Step 15 repository-owned native/smoke content references game/proprietary payload: {path}: {forbidden}')

gates = Path('src/StS2Launcher.Core/GodotFoundationGateSequence.cs').read_text()
summary = Path('src/StS2Launcher.Core/GodotFoundationSummary.cs').read_text()
tests = Path('tests/StS2Launcher.Core.Tests/GodotFoundationGateTests.cs').read_text()
for marker in (
    'var expected = (GodotFoundationGate)(_results.Count + 1);',
    'Cannot advance after the first failed Godot foundation gate.',
    '_results.Count == 4',
):
    if marker not in gates + summary:
        raise SystemExit(f'ERROR: Step 15 ordered-gate contract missing: {marker}')
for marker in (
    'OrderedGodotFoundationGatesReachFourOfFourPass',
    'GodotFoundationStopsAtFirstFailingGate',
    'GodotFoundationRejectsOutOfOrderGate',
    'GodotFoundationCanResetForFreshProcessRun',
    'Assert.ThrowsExactly<InvalidOperationException>',
    'GODOT FOUNDATION PASS — 4/4',
):
    if marker not in tests:
        raise SystemExit(f'ERROR: Step 15 host gate test marker missing: {marker}')

root = Path('src/StS2Launcher.Step05.iOS/RootViewController.cs').read_text()
for marker in (
    'STEP 15 — GODOT FOUNDATION',
    'Version 0.0.42',
    'Steps 01–14 are complete on the physical iPhone.',
    'Step 15 — Godot Foundation (ordered gates A–D)',
    'Run Gates A–C — Native → Engine Init → Metal Render',
    'Verify Gate D — Touch + Background / Foreground',
    'RunGodotFoundationGatesABCAsync',
    'VerifyGodotFoundationGateD',
    'GodotFoundationGate.NativeAvailability',
    'GodotFoundationGate.EngineInitializeRenderLoop',
    'GodotFoundationGate.MetalRender',
    'GodotFoundationGate.TouchLifecycle',
    'WaitForGodotConditionAsync',
    'GODOT FOUNDATION IN PROGRESS — 3/4',
    'PASS: STEP 15 GODOT FOUNDATION — 4/4',
    'Step 15 project: launcher-owned smoke scene only',
    'Run Foundation 5/5 Regression',
    'Inventory Installed Game Compatibility (Read Only)',
):
    if marker not in root:
        raise SystemExit(f'ERROR: Step 15 UI/gate marker missing: {marker}')

build = Path('scripts/build-step15.sh').read_text()
for marker in (
    'bash scripts/validate-step15.sh',
    'bash scripts/build-godot-step15.sh',
    'native/step15/smoke_project',
    'Step15GodotSmokeProject',
    'StS2-Launcher-Step-15.ipa',
):
    if marker not in build:
        raise SystemExit(f'ERROR: Step 15 build wrapper marker missing: {marker}')

verify = Path('scripts/verify-step15-ipa.sh').read_text()
for marker in (
    '0.0.42',
    'BUILD_VERSION" == "42"',
    'Step15GodotSmokeProject',
    '_sts2_step15_get_engine_version',
    '_sts2_step15_start',
    '4.5.1-stable',
    'DiskArbitration',
):
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 15 IPA verification marker missing: {marker}')

codemagic = Path('codemagic.yaml').read_text()
for marker in (
    'ios-step-15:',
    'Step 15 - Godot Foundation',
    'max_build_duration: 120',
    '$HOME/.cache/sts2launcher/godot-step15',
    'artifacts/StS2-Launcher-Step-15.ipa',
    'artifacts/step15-build-summary.txt',
):
    if marker not in codemagic:
        raise SystemExit(f'ERROR: Step 15 Codemagic marker missing: {marker}')

# Step 15 deliberately proves only a project-owned Godot foundation. No Cecil,
# StS2 assembly execution, game-native library integration, Cloud, or Workshop.
step15_sources = '\n'.join([
    managed_bridge,
    bridge,
    gates,
    smoke_project,
    smoke_scene,
    smoke_script,
])
for forbidden in (
    'Assembly.Load(',
    'AssemblyLoadContext',
    'Mono.Cecil',
    'SteamWorkshop',
    'SteamCloud',
    'LaunchGame',
    'RunStS2',
):
    if forbidden in step15_sources:
        raise SystemExit(f'ERROR: Step 15 broadened into a later subsystem: {forbidden}')

print('Step 15 Godot Foundation source validation: PASS')
print('  Steps 01-14 regression guards retained')
print('  Godot is pinned to 4.5.1-stable and source-built as an iOS arm64 static archive')
print('  Competing Godot UIApplicationMain symbol is renamed; apple_embedded_main remains')
print('  Ordered gates enforce stop-at-first-failure: A native, B engine/render loop, C Metal scene, D touch/lifecycle')
print('  Smoke project is launcher-owned and contains no StS2/FMOD/Spine payload')
print('  No Cecil rewrite, StS2 game execution, Cloud, or Workshop capability added')
PY
