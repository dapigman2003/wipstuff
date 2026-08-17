#!/usr/bin/env bash
set -euo pipefail

IPA="${1:-artifacts/StS2-Launcher-Step-13.ipa}"

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

APP="$(find "$TMP/Payload" -maxdepth 1 -type d -name '*.app' -print -quit)"
[[ -n "$APP" ]] || { echo "ERROR: Payload/*.app missing." >&2; exit 3; }
PLIST="$APP/Info.plist"
PLISTBUDDY=/usr/libexec/PlistBuddy

BUNDLE_ID="$($PLISTBUDDY -c 'Print :CFBundleIdentifier' "$PLIST")"
VERSION="$($PLISTBUDDY -c 'Print :CFBundleShortVersionString' "$PLIST")"
BUILD_VERSION="$($PLISTBUDDY -c 'Print :CFBundleVersion' "$PLIST")"
EXEC_NAME="$($PLISTBUDDY -c 'Print :CFBundleExecutable' "$PLIST")"
EXECUTABLE="$APP/$EXEC_NAME"

[[ "$BUNDLE_ID" == "com.community.sts2launcher" ]] || { echo "ERROR: wrong bundle ID: $BUNDLE_ID" >&2; exit 4; }
[[ "$VERSION" == "0.0.40" ]] || { echo "ERROR: wrong Step 13 version: $VERSION" >&2; exit 4; }
[[ "$BUILD_VERSION" == "40" ]] || { echo "ERROR: wrong Step 13 build version: $BUILD_VERSION" >&2; exit 4; }
[[ -f "$EXECUTABLE" ]] || { echo "ERROR: executable missing: $EXECUTABLE" >&2; exit 4; }
grep -qi 'arm64' <<<"$(file "$EXECUTABLE")" || { echo "ERROR: executable is not arm64." >&2; exit 4; }

APP_FILE_LIST="$TMP/app-file-list.txt"
find "$APP" -type f -print > "$APP_FILE_LIST"
if grep -Ei \
  '(godot|mono\.cecil|libsts2godothost|slay.*spire.*2|StS2Launcher\.SteamKitIosPatcher|MSTest|TestAdapter|TestFramework)' "$APP_FILE_LIST" >/dev/null; then
  echo "ERROR: IPA contains a forbidden build/test/later-stage/game component." >&2
  exit 5
fi

echo "Step 13 IPA verification passed."
echo "  Bundle ID: $BUNDLE_ID"
echo "  Version: $VERSION ($BUILD_VERSION)"
echo "  Architecture: arm64"
echo "  Expected device UI: STEP 13 — OFFLINE LAUNCHER STATE"
