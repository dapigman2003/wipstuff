#!/usr/bin/env bash
set -euo pipefail

IPA="${1:-artifacts/StS2-Launcher-Step-06.3.ipa}"

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
[[ "$VERSION" == "0.0.26" ]] || { echo "ERROR: wrong Step 06.3 version: $VERSION" >&2; exit 4; }
[[ "$BUILD_VERSION" == "26" ]] || { echo "ERROR: wrong Step 06.3 build version: $BUILD_VERSION" >&2; exit 4; }
[[ -f "$EXECUTABLE" ]] || { echo "ERROR: executable missing: $EXECUTABLE" >&2; exit 4; }
grep -qi 'arm64' <<<"$(file "$EXECUTABLE")" || { echo "ERROR: executable is not arm64." >&2; exit 4; }

if find "$APP" -type f | grep -Ei \
  '(godot|mono\.cecil|libsts2godothost|slay.*spire.*2|StS2Launcher\.SteamKitIosPatcher|MSTest|TestAdapter|TestFramework)' >/dev/null; then
  echo "ERROR: IPA contains a forbidden build/test/later-stage/game component." >&2
  exit 5
fi

echo "Step 06.3 IPA verification passed."
echo "  Bundle ID: $BUNDLE_ID"
echo "  Version: $VERSION ($BUILD_VERSION)"
echo "  Architecture: arm64"
echo "  Expected device UI: STEP 06.3 — SESSION RECOVERY"
