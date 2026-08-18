#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

# Step 15 is the first accelerated multi-gate subsystem release. Every already-
# proven Step 01-14 boundary remains a regression guard.
STS2_VALIDATE_AS_PARENT=1 bash scripts/validate-step14.sh

python3 - <<'PY'
from pathlib import Path
import os
import plistlib
import re

parent_mode = os.environ.get('STS2_VALIDATE_AS_PARENT') == '1'

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
    Path('native/step15/godot_module/sts2_ios_host/apple_embedded_plugin_stubs.cpp'),
    Path('native/step15/smoke_project/project.godot'),
    Path('native/step15/smoke_project/Main.tscn'),
    Path('native/step15/smoke_project/Step15Smoke.gd'),
    Path('tests/StS2Launcher.Core.Tests/GodotFoundationGateTests.cs'),
    Path('scripts/build-godot-step15.sh'),
    Path('scripts/build-step15.sh'),
    Path('scripts/preflight-godot-link-step15.sh'),
    Path('scripts/verify-step15-ipa.sh'),
    Path('docs/STEP-15-TEST.md'),
    Path('docs/STEP-15.0.4-FIX.md'),
    Path('docs/STEP-15.1-PREFLIGHT-STABILITY.md'),
]
for path in required_paths:
    if not path.exists():
        raise SystemExit(f'ERROR: Step 15 artifact missing: {path}')

with Path('src/StS2Launcher.Step05.iOS/Info.plist').open('rb') as f:
    plist = plistlib.load(f)
if not parent_mode:
    if plist.get('CFBundleShortVersionString') != '0.0.43' or str(plist.get('CFBundleVersion')) != '43':
        raise SystemExit('ERROR: standalone Step 15.1 must be version 0.0.43 (43).')
else:
    if int(str(plist.get('CFBundleVersion') or '0')) < 43:
        raise SystemExit('ERROR: later-step Step 15 regression validation requires build version >= 43.')

csproj = Path('src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj').read_text()
project_markers = [
    '<TrimMode>full</TrimMode>',
    '<TrimmerRootAssembly Include="SteamKit2" />',
    '<TrimmerRootAssembly Include="protobuf-net" />',
    '<TrimmerRootAssembly Include="protobuf-net.Core" />',
    '<_LinkerFrameworks Remove="DiskArbitration" />',
    "Condition=\"Exists('NativeBuild/libgodot-step15.a')\"",
    '<NativeReference Include="NativeBuild/libgodot-step15.a">',
    '<Kind>Static</Kind>',
    '<ForceLoad>false</ForceLoad>',
    '<SmartLink>false</SmartLink>',
    '<IsCxx>true</IsCxx>',
    'MetalFX',
    'MetalKit',
]
if not parent_mode:
    project_markers.extend([
        '<ApplicationVersion>43</ApplicationVersion>',
        '<ApplicationDisplayVersion>0.0.43</ApplicationDisplayVersion>',
    ])
for marker in project_markers:
    if marker not in csproj:
        raise SystemExit(f'ERROR: Step 15 project marker missing: {marker}')
if 'DiskArbitration' not in csproj or '<_LinkerFrameworks Remove="DiskArbitration" />' not in csproj:
    raise SystemExit('ERROR: proven Step 05 DiskArbitration filter was not preserved.')
if 'Mono.Cecil' in csproj:
    raise SystemExit('ERROR: Step 15 must not add Mono.Cecil to the runtime project.')
framework_match = re.search(r'<Frameworks>([^<]+)</Frameworks>', csproj)
if not framework_match:
    raise SystemExit('ERROR: Step 15 Godot NativeReference framework list missing.')
link_frameworks = framework_match.group(1).split()
if 'AudioToolbox' not in link_frameworks:
    raise SystemExit('ERROR: Step 15 must retain AudioToolbox for Godot iOS audio symbols.')
if 'AudioUnit' in link_frameworks:
    raise SystemExit('ERROR: Step 15 must not request standalone AudioUnit.framework; Xcode 26.5 iPhoneOS link failed with framework not found.')
if '<ForceLoad>true</ForceLoad>' in csproj:
    raise SystemExit('ERROR: Step 15 must not force-load the combined Godot archive; doing so pulls mutually exclusive PCRE2 width objects and produces duplicate __pcre2_ckd_smul.')
if '<ForceLoad>false</ForceLoad>' not in csproj or '<SmartLink>false</SmartLink>' not in csproj:
    raise SystemExit('ERROR: Step 15 Godot NativeReference must use explicit normal static-archive member selection (ForceLoad=false, SmartLink=false).')
if '<LinkerFlags>-ObjC -lz</LinkerFlags>' not in csproj:
    raise SystemExit('ERROR: Step 15 must retain exactly -ObjC -lz while using normal Godot archive selection; native bridge rooting belongs in ReferenceNativeSymbol.')
root_group_match = re.search(r'<ItemGroup Condition="Exists\(\'NativeBuild/libgodot-step15\.a\'\)">\s*(?:<ReferenceNativeSymbol[^>]+/>\s*)+</ItemGroup>', csproj, re.S)
if not root_group_match:
    raise SystemExit('ERROR: Step 15 ReferenceNativeSymbol roots must be conditioned on the generated Godot archive existing, matching NativeReference availability.')

bridge = Path('native/step15/godot_module/sts2_ios_host/step15_ios_host_bridge.mm').read_text()
for marker in (
    'extern int apple_embedded_main(int argc, char **argv);',
    'sts2_step15_get_engine_version',
    'GODOT_VERSION_NUMBER "-" GODOT_VERSION_STATUS',
    'sts2_step15_start',
    'sts2_step15_requires_process_restart',
    'g_process_restart_required.store(1);',
    'CGRectGetWidth(container.bounds) < 1.0',
    'Could not reset the Step 15 render marker',
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
    '#import <QuartzCore/CAMetalLayer.h>',
    '[CAMetalLayer class]',
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
managed_entry_points = set(re.findall(r'EntryPoint\s*=\s*"([^"]+)"', managed_bridge))
managed_cdecl_entry_points = re.findall(
    r'\[DllImport\(InternalLibrary,\s*EntryPoint\s*=\s*"([^"]+)",\s*CallingConvention\s*=\s*CallingConvention\.Cdecl\)\]',
    managed_bridge,
)
if len(managed_cdecl_entry_points) != len(managed_entry_points) or set(managed_cdecl_entry_points) != managed_entry_points:
    raise SystemExit(
        f'ERROR: every Step 15 managed bridge DllImport must target __Internal with explicit Cdecl. '
        f'entryPoints={sorted(managed_entry_points)}, cdecl={sorted(managed_cdecl_entry_points)}'
    )
rooted_native_symbols = set(re.findall(r'<ReferenceNativeSymbol Include="([^"]+)" SymbolType="Function"\s*/>', csproj))
if managed_entry_points != rooted_native_symbols:
    missing = sorted(managed_entry_points - rooted_native_symbols)
    extra = sorted(rooted_native_symbols - managed_entry_points)
    raise SystemExit(f'ERROR: Step 15 ReferenceNativeSymbol set does not exactly match managed bridge entry points. missing={missing}, extra={extra}')
if not managed_entry_points:
    raise SystemExit('ERROR: Step 15 managed bridge has no __Internal entry points to root.')
for marker in (
    'DllImport(InternalLibrary, EntryPoint = "sts2_step15_get_engine_version", CallingConvention = CallingConvention.Cdecl)',
    'DllImport(InternalLibrary, EntryPoint = "sts2_step15_start", CallingConvention = CallingConvention.Cdecl)',
    'DllImport(InternalLibrary, EntryPoint = "sts2_step15_requires_process_restart", CallingConvention = CallingConvention.Cdecl)',
    '[MarshalAs(UnmanagedType.LPUTF8Str)] string projectPathUtf8',
    'DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_metal_layer_ready", CallingConvention = CallingConvention.Cdecl)',
    'DllImport(InternalLibrary, EntryPoint = "sts2_step15_touch_marker_ready", CallingConvention = CallingConvention.Cdecl)',
    'private const string InternalLibrary = "__Internal";',
):
    if marker not in managed_bridge:
        raise SystemExit(f'ERROR: Step 15 managed/native bridge marker missing: {marker}')

build_godot = Path('scripts/build-godot-step15.sh').read_text()
for marker in (
    'GODOT_TAG="4.5.1-stable"',
    'GODOT_COMMIT="f62fdbde15035c5576dad93e586201f4d41ef0cb"',
    'git -c advice.detachedHead=false clone --depth 1 --branch "$GODOT_TAG"',
    'int sts2_godot_template_main_disabled(int argc, char *argv[]) {',
    "text.count('int apple_embedded_main(int argc, char **argv) {') != 1",
    'godot-embedded-view-controller-service-patch=v1',
    'godot-empty-ios-plugin-glue=v1',
    '+ (void)sts2_setEmbeddedViewController:(GDTViewController *)viewController;',
    'mainViewController = viewController;',
    'platform=ios',
    'target=template_release',
    'arch=arm64',
    'metal=yes',
    'vulkan=no',
    'opengl3=yes',
    'lto=none',
    'STEP15_ROOT_SYMBOLS=',
    'for root_symbol in $STEP15_ROOT_SYMBOLS; do',
    "grep -E '[[:space:]]T[[:space:]].*apple_embedded_main' \"$symbols_file\" >/dev/null",
    "grep -E '[[:space:]]T[[:space:]].*godot_apple_embedded_plugins_initialize' \"$symbols_file\" >/dev/null",
    "grep -E '[[:space:]]T[[:space:]].*godot_apple_embedded_plugins_deinitialize' \"$symbols_file\" >/dev/null",
):
    if marker not in build_godot:
        raise SystemExit(f'ERROR: pinned Godot build marker missing: {marker}')
if 'generate_bundle=yes' in build_godot:
    raise SystemExit('ERROR: Step 15 should build only the static Godot archive, not a competing Godot app bundle.')
if 'nm -gU "$lib" 2>/dev/null | grep -q' in build_godot:
    raise SystemExit('ERROR: Step 15 archive validation must not use early-exit grep -q pipelines under pipefail.')
if "grep -q '_apple_embedded_main'" in build_godot:
    raise SystemExit('ERROR: Step 15 must not assume apple_embedded_main has unmangled C linkage.')
if re.search(r'Accelerate AudioToolbox\s+AudioUnit\s+AVFoundation', build_godot):
    raise SystemExit('ERROR: Step 15 build-side framework sanity list must not classify AudioUnit as a standalone link framework.')
if 'libgodot.ios.template_release.arm64.a' not in build_godot:
    raise SystemExit('ERROR: Step 15 must select the deterministic Godot 4.5.1 combined archive path.')
if "find \"$SOURCE_DIR/bin\"" in build_godot and "tail -1" in build_godot:
    raise SystemExit('ERROR: Step 15 must not choose an arbitrary libgodot*.a via find/tail.')
if 'SCONS_VENV="$ROOT/artifacts/step15-scons-venv"' not in build_godot:
    raise SystemExit('ERROR: Step 15 SCons venv must remain outside the Codemagic cached directory.')
if 'rm -rf "$CACHE_ROOT/scons-venv"' not in build_godot:
    raise SystemExit('ERROR: Step 15 must purge the legacy cached symlink-containing SCons venv.')
if 'bash scripts/preflight-godot-link-step15.sh "$OUT_LIB"' not in build_godot:
    raise SystemExit('ERROR: Step 15 must native-link preflight the Godot archive before dotnet publish.')
if 'ACTUAL_COMMIT' not in build_godot or 'GODOT_COMMIT' not in build_godot:
    raise SystemExit('ERROR: Step 15 must verify the immutable Godot commit in addition to the tag.')
if 'Path("scripts/build-godot-step15.sh")' not in build_godot:
    raise SystemExit('ERROR: Step 15 cache fingerprint must include the integration build/patch script itself.')
if 'rm -rf "$SOURCE_DIR" "$SCONS_VENV"' not in build_godot:
    raise SystemExit('ERROR: Step 15 must release the large Godot source checkout/SCons venv before .NET iOS AOT after caching the validated archive.')
if 'rm -rf "$SOURCE_DIR" "$ROOT/artifacts/step15-scons-venv"' not in build_godot:
    raise SystemExit('ERROR: Step 15 cache-hit path must also purge stale source/SCons workspace remnants before .NET iOS AOT.')
if "grep -E '[[:space:]]T[[:space:]]_main$'" not in build_godot:
    raise SystemExit('ERROR: Step 15 archive validator must reject only a defined _main symbol, not unrelated undefined references.')

plugin_glue = Path('native/step15/godot_module/sts2_ios_host/apple_embedded_plugin_stubs.cpp').read_text()
for marker in (
    'void godot_apple_embedded_plugins_initialize() {',
    'void godot_apple_embedded_plugins_deinitialize() {',
    '__attribute__((visibility("default")))',
    'deliberately has zero iOS plugins',
):
    if marker not in plugin_glue:
        raise SystemExit(f'ERROR: Step 15 no-plugin glue marker missing: {marker}')
for forbidden in (
    'extern "C"',
    'plugin_init(',
    'register_inappstore',
    'register_gamecenter',
    'SteamKit2',
    'SlayTheSpire2',
):
    if forbidden in plugin_glue:
        raise SystemExit(f'ERROR: Step 15 no-plugin glue broadened beyond empty plugin hooks: {forbidden}')

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
    'await RenderingServer.frame_post_draw',
    '_write_marker(RENDER_MARKER, "frame-post-draw-ready")',
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
root_markers = [
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
    'EvaluateGodotConditionOnMainThread',
    'NSThread.IsMain',
    'InvokeOnMainThread(() => result = condition());',
    '_godotProcessRequiresRestart',
    'GodotStep15NativeBridge.RequiresProcessRestart',
    'View?.LayoutIfNeeded();',
    'GODOT FOUNDATION IN PROGRESS — 3/4',
    'PASS: STEP 15 GODOT FOUNDATION — 4/4',
    'Step 15 project: launcher-owned smoke scene only',
    'Run Foundation 5/5 Regression',
    'Inventory Installed Game Compatibility (Read Only)',
]
if not parent_mode:
    root_markers.extend([
        'STEP 15.1 — GODOT FOUNDATION HARDENING',
        'Version 0.0.43',
        'Steps 01–14 are complete on the physical iPhone.',
    ])
for marker in root_markers:
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

for older_verify in ('scripts/verify-step10-ipa.sh', 'scripts/verify-step11-ipa.sh', 'scripts/verify-step12-ipa.sh', 'scripts/verify-step13-ipa.sh', 'scripts/verify-step14-ipa.sh'):
    older_text = Path(older_verify).read_text()
    if "find \"$TMP/Payload\" -maxdepth 1 -type d -name '*.app' | head -1" in older_text:
        raise SystemExit(f'ERROR: {older_verify} retains a find|head pipeline that can fail spuriously under set -o pipefail.')
    if 'find "$APP" -type f | grep -Ei' in older_text:
        raise SystemExit(f'ERROR: {older_verify} retains a find|grep early-exit pipeline that can bypass forbidden-payload detection under set -o pipefail.')

verify = Path('scripts/verify-step15-ipa.sh').read_text()
if 'find "$APP" -type f | grep -Ei' in verify:
    raise SystemExit('ERROR: Step 15 IPA verifier retains a find|grep early-exit pipeline under set -o pipefail.')
for marker in (
    '0.0.43',
    'BUILD_VERSION" == "43"',
    'Step15GodotSmokeProject',
    'Every DllImport("__Internal") Step 15 entry point',
    '[[:space:]]T[[:space:]]${symbol}$',
    'step15-final-native-dependencies.log',
    '4.5.1-stable',
    'DiskArbitration',
    'AudioUnit.framework',
):
    if marker not in verify:
        raise SystemExit(f'ERROR: Step 15 IPA verification marker missing: {marker}')

if not parent_mode:
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


preflight = Path('scripts/preflight-godot-link-step15.sh').read_text()
for marker in (
    'xcrun --sdk iphoneos clang++',
    'SRC="$TMP/preflight.cc"',
    'OBJ="$TMP/preflight.o"',
    '-std=c++17',
    '-x c++',
    '-c "$SRC"',
    '"$OBJ"',
    '-miphoneos-version-min="$DEPLOYMENT_TARGET"',
    'NativeBuild/libgodot-step15',
    'sts2_step15_requires_process_restart',
    'STEP 15 STANDALONE NATIVE LINK PREFLIGHT: PASS',
    '<SupportedOSPlatformVersion>',
    'STEP15_ROOT_SYMBOLS',
    '-Wl,-u,_${symbol}',
):
    if marker not in preflight:
        raise SystemExit(f'ERROR: Step 15 native-link preflight marker missing: {marker}')
if '-force_load' in preflight or '-all_load' in preflight:
    raise SystemExit('ERROR: Step 15 native-link preflight must mirror normal archive-member selection.')
if 'preflight.mm' in preflight:
    raise SystemExit('ERROR: Step 15 native-link preflight must not depend on Objective-C++ filename inference; use explicit C++ compile mode.')

app_delegate = Path('src/StS2Launcher.Step05.iOS/AppDelegate.cs').read_text()
if 'public override UIWindow? Window { get; set; }' not in app_delegate:
    raise SystemExit('ERROR: Step 15 AppDelegate must expose window/setWindow: explicitly for Godot Apple-embedded window queries.')

scene_delegate = Path('src/StS2Launcher.Step05.iOS/SceneDelegate.cs').read_text()
if 'Step 13 startup exception' in scene_delegate:
    raise SystemExit('ERROR: Step 15+ SceneDelegate startup telemetry regressed to the old Step 13 label.')
if not parent_mode and 'Step 15 startup exception' not in scene_delegate:
    raise SystemExit('ERROR: standalone Step 15 SceneDelegate startup telemetry is stale/mislabeled.')

codemagic_build = Path('scripts/codemagic-build.sh').read_text()
for marker in (
    '--retry 4 --retry-delay 3 --retry-all-errors',
    'WORKLOAD_OK=0',
    'Godot source pin: 4.5.1-stable @ f62fdbde15035c5576dad93e586201f4d41ef0cb',
):
    if marker not in codemagic_build:
        raise SystemExit(f'ERROR: Step 15.1 Codemagic resilience marker missing: {marker}')

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

print('Step 15.1 Godot Foundation regression validation: PASS' if parent_mode else 'Step 15.1 Godot Foundation preflight/stability source validation: PASS')
print('  Steps 01-14 regression guards retained')
print('  Godot is pinned to 4.5.1-stable + immutable commit and source-built as an iOS arm64 static archive')
print('  Competing Godot UIApplicationMain symbol is renamed; apple_embedded_main remains')
print('  Standalone Apple native-link preflight runs before dotnet publish; runtime retries are blocked after Godot touches process-global state')
print('  Ordered gates enforce stop-at-first-failure: A native, B engine/render loop, C Metal scene, D touch/lifecycle')
print('  Smoke project is launcher-owned and contains no StS2/FMOD/Spine payload')
print('  No Cecil rewrite, StS2 game execution, Cloud, or Workshop capability added')
PY
