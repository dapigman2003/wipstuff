#!/usr/bin/env bash
set -euo pipefail

IPA="${1:-artifacts/StS2-Launcher-Step-15.ipa}"

if [[ ! -f "$IPA" ]]; then
  echo "ERROR: IPA not found: $IPA" >&2
  exit 2
fi
if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: IPA verification requires macOS plist/Mach-O tools." >&2
  exit 2
fi

unzip -tq "$IPA" >/dev/null
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
unzip -q "$IPA" -d "$TMP"

APP="$(find "$TMP/Payload" -maxdepth 1 -type d -name '*.app' | head -1)"
[[ -n "$APP" ]] || { echo "ERROR: Payload/*.app missing." >&2; exit 3; }
PLIST="$APP/Info.plist"
PLISTBUDDY=/usr/libexec/PlistBuddy

BUNDLE_ID="$($PLISTBUDDY -c 'Print :CFBundleIdentifier' "$PLIST")"
VERSION="$($PLISTBUDDY -c 'Print :CFBundleShortVersionString' "$PLIST")"
BUILD_VERSION="$($PLISTBUDDY -c 'Print :CFBundleVersion' "$PLIST")"
EXEC_NAME="$($PLISTBUDDY -c 'Print :CFBundleExecutable' "$PLIST")"
EXECUTABLE="$APP/$EXEC_NAME"

[[ "$BUNDLE_ID" == "com.community.sts2launcher" ]] || { echo "ERROR: wrong bundle ID: $BUNDLE_ID" >&2; exit 4; }
[[ "$VERSION" == "0.0.42" ]] || { echo "ERROR: wrong Step 15 version: $VERSION" >&2; exit 4; }
[[ "$BUILD_VERSION" == "42" ]] || { echo "ERROR: wrong Step 15 build version: $BUILD_VERSION" >&2; exit 4; }
[[ -f "$EXECUTABLE" ]] || { echo "ERROR: executable missing: $EXECUTABLE" >&2; exit 4; }
grep -qi 'arm64' <<<"$(file "$EXECUTABLE")" || { echo "ERROR: executable is not arm64." >&2; exit 4; }

SMOKE="$APP/Step15GodotSmokeProject"
for required in project.godot Main.tscn Step15Smoke.gd; do
  [[ -f "$SMOKE/$required" ]] || { echo "ERROR: Step 15 smoke-project file missing from IPA: $required" >&2; exit 5; }
done

grep -Fq 'StS2 Launcher Step 15 Smoke' "$SMOKE/project.godot" || { echo "ERROR: smoke project identity marker missing." >&2; exit 5; }
grep -Fq 'sts2_step15_render_ready.txt' "$SMOKE/Step15Smoke.gd" || { echo "ERROR: render marker contract missing." >&2; exit 5; }
grep -Fq 'InputEventScreenTouch' "$SMOKE/Step15Smoke.gd" || { echo "ERROR: touch marker contract missing." >&2; exit 5; }

# The native bridge must remain externally visible because the managed host calls
# it through DllImport("__Internal").
NATIVE_SYMBOLS="$(nm -gU "$EXECUTABLE" 2>/dev/null || true)"
for symbol in \
  _sts2_step15_get_engine_version \
  _sts2_step15_start \
  _sts2_step15_is_metal_layer_ready \
  _sts2_step15_touch_marker_ready; do
  grep -Fq "$symbol" <<<"$NATIVE_SYMBOLS" || {
    echo "ERROR: final executable does not export required Step 15 native symbol: $symbol" >&2
    exit 6
  }
done

# Godot is now intentional. Game payloads, Cecil and build/test tooling are not.
if find "$APP" -type f | grep -Ei \
  '(mono\.cecil|libsts2godothost|SlayTheSpire2\.app|(^|/)sts2\.dll$|libfmod|fmodstudio|spine_godot|StS2Launcher\.SteamKitIosPatcher|MSTest|TestAdapter|TestFramework)' >/dev/null; then
  echo "ERROR: IPA contains a forbidden game/proprietary/build/test/later-stage component." >&2
  exit 7
fi

if otool -l "$EXECUTABLE" 2>/dev/null | grep -Fq 'DiskArbitration'; then
  echo "ERROR: proven DiskArbitration iOS linker filter regressed." >&2
  exit 8
fi

if ! strings "$EXECUTABLE" | grep -Fq '4.5.1-stable'; then
  echo "ERROR: pinned Godot 4.5.1-stable native version marker not found in executable." >&2
  exit 9
fi

echo "Step 15 IPA verification passed."
echo "  Bundle ID: $BUNDLE_ID"
echo "  Version: $VERSION ($BUILD_VERSION)"
echo "  Architecture: arm64"
echo "  Godot: pinned 4.5.1-stable static host"
echo "  Smoke project: bundled project-owned GDScript scene"
echo "  Required native bridge exports: present"
echo "  Expected device UI: STEP 15 — GODOT FOUNDATION"
