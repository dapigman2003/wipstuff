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
[[ "$VERSION" == "0.0.43" ]] || { echo "ERROR: wrong Step 15.1 version: $VERSION" >&2; exit 4; }
[[ "$BUILD_VERSION" == "43" ]] || { echo "ERROR: wrong Step 15.1 build version: $BUILD_VERSION" >&2; exit 4; }
[[ -f "$EXECUTABLE" ]] || { echo "ERROR: executable missing: $EXECUTABLE" >&2; exit 4; }
grep -qi 'arm64' <<<"$(file "$EXECUTABLE")" || { echo "ERROR: executable is not arm64." >&2; exit 4; }

SMOKE="$APP/Step15GodotSmokeProject"
for required in project.godot Main.tscn Step15Smoke.gd; do
  [[ -f "$SMOKE/$required" ]] || { echo "ERROR: Step 15 smoke-project file missing from IPA: $required" >&2; exit 5; }
done

grep -Fq 'StS2 Launcher Step 15 Smoke' "$SMOKE/project.godot" || { echo "ERROR: smoke project identity marker missing." >&2; exit 5; }
grep -Fq 'sts2_step15_render_ready.txt' "$SMOKE/Step15Smoke.gd" || { echo "ERROR: render marker contract missing." >&2; exit 5; }
grep -Fq 'await RenderingServer.frame_post_draw' "$SMOKE/Step15Smoke.gd" || { echo "ERROR: render marker must wait for a completed render frame." >&2; exit 5; }
grep -Fq 'InputEventScreenTouch' "$SMOKE/Step15Smoke.gd" || { echo "ERROR: touch marker contract missing." >&2; exit 5; }

# Every DllImport("__Internal") Step 15 entry point must survive native linking.
NATIVE_SYMBOLS_FILE="$TMP/native-symbols.txt"
nm -gU "$EXECUTABLE" > "$NATIVE_SYMBOLS_FILE" 2>/dev/null || { echo "ERROR: nm could not inspect final executable." >&2; exit 6; }
while IFS= read -r entry; do
  [[ -n "$entry" ]] || continue
  symbol="_${entry}"
  grep -E "[[:space:]]T[[:space:]]${symbol}$" "$NATIVE_SYMBOLS_FILE" >/dev/null || {
    echo "ERROR: final executable does not define managed Step 15 entry point: $symbol" >&2
    exit 6
  }
done < <(python3 - <<'PY'
from pathlib import Path
import re
text = Path('src/StS2Launcher.Step05.iOS/Platform/GodotStep15NativeBridge.cs').read_text()
for value in sorted(set(re.findall(r'EntryPoint\s*=\s*"([^"]+)"', text))):
    print(value)
PY
)

# Godot is intentional. Game payloads, Cecil and build/test tooling are not.
APP_FILE_LIST="$TMP/app-file-list.txt"
find "$APP" -type f -print > "$APP_FILE_LIST"
if grep -Ei \
  '(mono\.cecil|libsts2godothost|SlayTheSpire2\.app|(^|/)sts2\.dll$|libfmod|fmodstudio|spine_godot|StS2Launcher\.SteamKitIosPatcher|MSTest|TestAdapter|TestFramework)' "$APP_FILE_LIST" >/dev/null; then
  echo "ERROR: IPA contains a forbidden game/proprietary/build/test/later-stage component." >&2
  exit 7
fi

LOAD_COMMANDS="$TMP/otool-load.txt"
DEPENDENCIES="$TMP/otool-dependencies.txt"
mkdir -p artifacts/logs
otool -l "$EXECUTABLE" > "$LOAD_COMMANDS"
otool -L "$EXECUTABLE" > "$DEPENDENCIES"
cp "$DEPENDENCIES" artifacts/logs/step15-final-native-dependencies.log

if grep -Fq 'DiskArbitration' "$LOAD_COMMANDS" || grep -Fq 'DiskArbitration' "$DEPENDENCIES"; then
  echo "ERROR: proven DiskArbitration iOS linker filter regressed." >&2
  exit 8
fi
if grep -Fq '/AudioUnit.framework/' "$DEPENDENCIES"; then
  echo "ERROR: standalone AudioUnit framework dependency returned." >&2
  exit 8
fi

# Reject unbundled or non-system dynamic dependencies. Static Godot must not
# smuggle a desktop dylib/framework into the app at runtime.
tail -n +2 "$DEPENDENCIES" | sed -E 's/^[[:space:]]+([^[:space:]]+).*/\1/' | while IFS= read -r dep; do
  [[ -n "$dep" ]] || continue
  case "$dep" in
    /System/Library/*|/usr/lib/*)
      ;;
    @rpath/*)
      rel="${dep#@rpath/}"
      [[ -e "$APP/Frameworks/$rel" || -e "$APP/$rel" ]] || {
        echo "ERROR: @rpath dependency is not bundled in the IPA: $dep" >&2
        exit 10
      }
      ;;
    @executable_path/*)
      rel="${dep#@executable_path/}"
      [[ -e "$APP/$rel" ]] || {
        echo "ERROR: @executable_path dependency is not bundled in the IPA: $dep" >&2
        exit 10
      }
      ;;
    @loader_path/*)
      rel="${dep#@loader_path/}"
      [[ -e "$APP/$rel" || -e "$APP/Frameworks/$rel" ]] || {
        echo "ERROR: @loader_path dependency is not bundled in the IPA: $dep" >&2
        exit 10
      }
      ;;
    *)
      echo "ERROR: unexpected non-system absolute dynamic dependency: $dep" >&2
      exit 10
      ;;
  esac
done

STRINGS_FILE="$TMP/executable-strings.txt"
strings "$EXECUTABLE" > "$STRINGS_FILE"
if ! grep -Fq '4.5.1-stable' "$STRINGS_FILE"; then
  echo "ERROR: pinned Godot 4.5.1-stable native version marker not found in executable." >&2
  exit 9
fi

mkdir -p artifacts/logs
{
  echo "Step 15.1 IPA verification passed."
  echo "  Bundle ID: $BUNDLE_ID"
  echo "  Version: $VERSION ($BUILD_VERSION)"
  echo "  Architecture: arm64"
  echo "  Godot: pinned 4.5.1-stable static host"
  echo "  Smoke project: bundled project-owned GDScript scene"
  echo "  Every managed __Internal Step 15 entry point: present"
  echo "  Dynamic dependency audit: system or bundled only"
  echo "  Expected device UI: STEP 15.1 — GODOT FOUNDATION HARDENING"
} | tee artifacts/logs/step15-ipa-verification-summary.log
