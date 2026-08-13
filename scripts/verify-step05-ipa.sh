#!/usr/bin/env bash
set -euo pipefail

IPA="${1:-artifacts/StS2-Launcher-Step-05.9.ipa}"

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

APP_COUNT="$(find "$TMP/Payload" -maxdepth 1 -type d -name '*.app' | wc -l | tr -d ' ')"
if [[ "$APP_COUNT" != "1" ]]; then
  echo "ERROR: expected exactly one Payload/*.app; found $APP_COUNT" >&2
  exit 3
fi

APP="$(find "$TMP/Payload" -maxdepth 1 -type d -name '*.app' | head -1)"
PLIST="$APP/Info.plist"
PLISTBUDDY=/usr/libexec/PlistBuddy

[[ -f "$PLIST" ]] || {
  echo "ERROR: packaged app has no Info.plist." >&2
  exit 3
}

BUNDLE_ID="$($PLISTBUDDY -c 'Print :CFBundleIdentifier' "$PLIST")"
VERSION="$($PLISTBUDDY -c 'Print :CFBundleShortVersionString' "$PLIST")"
EXEC_NAME="$($PLISTBUDDY -c 'Print :CFBundleExecutable' "$PLIST")"
MIN_IOS="$($PLISTBUDDY -c 'Print :MinimumOSVersion' "$PLIST" 2>/dev/null || true)"
EXECUTABLE="$APP/$EXEC_NAME"

[[ "$BUNDLE_ID" == "com.community.sts2launcher" ]] || {
  echo "ERROR: wrong bundle ID: $BUNDLE_ID" >&2
  exit 4
}

[[ "$VERSION" == "0.0.15" ]] || {
  echo "ERROR: wrong Step 05.9 version: $VERSION" >&2
  exit 4
}

[[ -f "$EXECUTABLE" ]] || {
  echo "ERROR: executable missing: $EXECUTABLE" >&2
  exit 4
}

FILE_INFO="$(file "$EXECUTABLE")"
grep -qi 'arm64' <<<"$FILE_INFO" || {
  echo "ERROR: executable is not arm64: $FILE_INFO" >&2
  exit 4
}

# The build-only Cecil patcher must never enter the device package.
if find "$APP" -type f | grep -Ei \
  '(godot|mono\.cecil|libsts2godothost|slay.*spire.*2|StS2Launcher\.SteamKitIosPatcher)' >/dev/null; then
  echo "ERROR: Step 05.9 IPA contains a forbidden build/later-stage/game component." >&2
  exit 5
fi

SIGNING="unsigned"
if codesign -dvv "$APP" >/dev/null 2>&1; then
  SIGNING="signed"
fi

SIZE="$(du -h "$IPA" | awk '{print $1}')"

echo "Step 05.9 IPA verification passed."
echo "  Bundle ID: $BUNDLE_ID"
echo "  Version: $VERSION"
echo "  Minimum iOS: ${MIN_IOS:-unknown}"
echo "  Architecture: arm64"
echo "  Package signing: $SIGNING"
echo "  IPA size: $SIZE"
echo "  Expected device UI: STEP 05.9 — EXACT STEAMKIT ENDPOINT REPLAY"
