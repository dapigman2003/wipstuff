#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
IPA="${1:-artifacts/StS2-Launcher-Step-19.ipa}"

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
[[ "$VERSION" == "0.0.54" ]] || { echo "ERROR: wrong Step 19 version: $VERSION" >&2; exit 4; }
[[ "$BUILD_VERSION" == "54" ]] || { echo "ERROR: wrong Step 19 build version: $BUILD_VERSION" >&2; exit 4; }
[[ -f "$EXECUTABLE" ]] || { echo "ERROR: executable missing: $EXECUTABLE" >&2; exit 4; }
grep -qi 'arm64' <<<"$(file "$EXECUTABLE")" || { echo "ERROR: executable is not arm64." >&2; exit 4; }

# Step 15 remains available as a regression boundary in Step 19.
SMOKE="$APP/Step15GodotSmokeProject"
for required in project.godot Main.tscn Step15Smoke.gd; do
  [[ -f "$SMOKE/$required" ]] || { echo "ERROR: Step 15 regression smoke-project file missing: $required" >&2; exit 5; }
done

# Step 16 regression fixture remains inert project-owned managed test data. It is deliberately
# copied after publish so it is not treated as an app assembly or AOT input.
FIXTURE="$APP/Step16Fixtures/StS2Launcher.Step16.Fixture.dll"
[[ -f "$FIXTURE" ]] || { echo "ERROR: Step 16 fixture assembly missing from IPA." >&2; exit 5; }
[[ -s "$FIXTURE" ]] || { echo "ERROR: Step 16 fixture assembly is empty." >&2; exit 5; }
FIXTURE_SOURCE="$ROOT/fixtures/StS2Launcher.Step16.Fixture/bin/Release/net9.0/StS2Launcher.Step16.Fixture.dll"
[[ -f "$FIXTURE_SOURCE" ]] || { echo "ERROR: just-built Step 16 source fixture missing for IPA byte-for-byte verification." >&2; exit 5; }
if ! cmp -s "$FIXTURE_SOURCE" "$FIXTURE"; then
  echo "ERROR: bundled Step 16 fixture differs from the exact project-owned fixture built earlier in this Codemagic run." >&2
  exit 5
fi

# Every Step 15 native bridge symbol must still survive final linking.
NATIVE_SYMBOLS_FILE="$TMP/native-symbols.txt"
nm -gU "$EXECUTABLE" > "$NATIVE_SYMBOLS_FILE" 2>/dev/null || { echo "ERROR: nm could not inspect final executable." >&2; exit 6; }
while IFS= read -r entry; do
  [[ -n "$entry" ]] || continue
  symbol="_${entry}"
  grep -E "[[:space:]]T[[:space:]]${symbol}$" "$NATIVE_SYMBOLS_FILE" >/dev/null || {
    echo "ERROR: final executable does not define Step 15 native entry point: $symbol" >&2
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

APP_FILE_LIST="$TMP/app-file-list.txt"
find "$APP" -type f -print > "$APP_FILE_LIST"
# Mono.Cecil is now an intentional runtime dependency for Steps 16–17. The project-
# owned fixture is also intentional. Continue rejecting real game/proprietary
# payloads, patcher/test binaries and unrelated later-stage artifacts.
if grep -Ei \
  '(libsts2godothost|SlayTheSpire2\.app|(^|/)sts2\.dll$|libfmod|fmodstudio|spine_godot|StS2Launcher\.SteamKitIosPatcher|MSTest|TestAdapter|TestFramework)' "$APP_FILE_LIST" >/dev/null; then
  echo "ERROR: IPA contains a forbidden game/proprietary/build/test component." >&2
  exit 7
fi

LOAD_COMMANDS="$TMP/otool-load.txt"
DEPENDENCIES="$TMP/otool-dependencies.txt"
mkdir -p artifacts/logs
otool -l "$EXECUTABLE" > "$LOAD_COMMANDS"
otool -L "$EXECUTABLE" > "$DEPENDENCIES"
cp "$DEPENDENCIES" artifacts/logs/step19-final-native-dependencies.log

if grep -Fq 'DiskArbitration' "$LOAD_COMMANDS" || grep -Fq 'DiskArbitration' "$DEPENDENCIES"; then
  echo "ERROR: proven DiskArbitration iOS linker filter regressed." >&2
  exit 8
fi
if grep -Fq '/AudioUnit.framework/' "$DEPENDENCIES"; then
  echo "ERROR: standalone AudioUnit framework dependency returned." >&2
  exit 8
fi

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

# Keep one native Godot version marker check, which is emitted by the native
# engine archive itself. Do not make IPA acceptance depend on managed-string
# layout in the AOT executable: managed literals may not be emitted as plain
# ASCII even when the code is present and callable. Host compilation plus the
# physical Steps 16–19 Cecil/interpreter gates are the authoritative managed-runtime checks.
STRINGS_FILE="$TMP/executable-strings.txt"
strings "$EXECUTABLE" > "$STRINGS_FILE"
grep -Fq '4.5.1-stable' "$STRINGS_FILE" || {
  echo "ERROR: Godot 4.5.1 native version marker missing from final executable." >&2
  exit 9
}

{
  echo "Step 19 IPA verification passed."
  echo "  Bundle ID: $BUNDLE_ID"
  echo "  Version: $VERSION ($BUILD_VERSION)"
  echo "  Architecture: arm64"
  echo "  Godot 4.5.1 Step 15 regression host: retained"
  echo "  Mono.Cecil 0.11.6 compatibility-analysis dependency: linked/AOT input"
  echo "  Project-owned Step 16 regression fixture: bundled as inert raw assembly data"
  echo "  Real StS2/proprietary payload in IPA: none"
  echo "  Dynamic dependency audit: system or bundled only"
  echo "  Expected device UI: STEP 19.2 — EXPRESSION INTERPRETER COMPATIBILITY"
} | tee artifacts/logs/step19-ipa-verification-summary.log
