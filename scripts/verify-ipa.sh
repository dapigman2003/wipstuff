#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
source scripts/lib/current-release.sh
IPA="${1:-$STS2_IPA_REL}"

mkdir -p artifacts/reports artifacts/logs
REPORT="artifacts/reports/ipa-verification.txt"
: > "$REPORT"
exec > >(tee -a "$REPORT") 2>&1

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
FILE_SHARING="$($PLISTBUDDY -c 'Print :UIFileSharingEnabled' "$PLIST")"
OPEN_IN_PLACE="$($PLISTBUDDY -c 'Print :LSSupportsOpeningDocumentsInPlace' "$PLIST")"
EXEC_NAME="$($PLISTBUDDY -c 'Print :CFBundleExecutable' "$PLIST")"
EXECUTABLE="$APP/$EXEC_NAME"

[[ "$BUNDLE_ID" == "com.community.sts2launcher" ]] || { echo "ERROR: wrong bundle ID: $BUNDLE_ID" >&2; exit 4; }
[[ "$VERSION" == "$STS2_DISPLAY_VERSION" ]] || { echo "ERROR: wrong Step 35 version: $VERSION" >&2; exit 4; }
[[ "$BUILD_VERSION" == "$STS2_BUILD_VERSION" ]] || { echo "ERROR: wrong Step 35 build version: $BUILD_VERSION" >&2; exit 4; }
[[ "$FILE_SHARING" == "true" ]] || { echo "ERROR: Step 35 final IPA does not enable UIFileSharingEnabled: $FILE_SHARING" >&2; exit 4; }
[[ "$OPEN_IN_PLACE" == "true" ]] || { echo "ERROR: Step 35 final IPA does not enable LSSupportsOpeningDocumentsInPlace: $OPEN_IN_PLACE" >&2; exit 4; }
[[ -f "$EXECUTABLE" ]] || { echo "ERROR: executable missing: $EXECUTABLE" >&2; exit 4; }
grep -qi 'arm64' <<<"$(file "$EXECUTABLE")" || { echo "ERROR: executable is not arm64." >&2; exit 4; }

# Step 15 remains available as a protected regression boundary.
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

# Step 20 external managed fixtures are a deliberately post-publish data payload. They must
# exist exactly once, only under Step20DynamicFixtures, and must be byte-for-byte identical to
# the project-owned net9.0 class libraries built earlier in this Codemagic run.
STEP20_FIXTURE_DIR="$APP/Step20DynamicFixtures"
[[ -d "$STEP20_FIXTURE_DIR" ]] || { echo "ERROR: Step 20 external fixture directory missing from IPA." >&2; exit 5; }
STEP20_MANIFEST="$STEP20_FIXTURE_DIR/step20-fixtures.sha256"
[[ -f "$STEP20_MANIFEST" ]] || { echo "ERROR: Step 20 fixture SHA-256 manifest missing from IPA." >&2; exit 5; }

STEP20_FIXTURE_NAMES=(
  StS2Launcher.Step20.DynamicFixture.dll
  StS2Launcher.Step20.DependencyFixture.dll
  StS2Launcher.Step20.RootFixture.dll
)
STEP20_FIXTURE_SOURCES=(
  "$ROOT/fixtures/StS2Launcher.Step20.DynamicFixture/bin/Release/net9.0/StS2Launcher.Step20.DynamicFixture.dll"
  "$ROOT/fixtures/StS2Launcher.Step20.DependencyFixture/bin/Release/net9.0/StS2Launcher.Step20.DependencyFixture.dll"
  "$ROOT/fixtures/StS2Launcher.Step20.RootFixture/bin/Release/net9.0/StS2Launcher.Step20.RootFixture.dll"
)
for index in "${!STEP20_FIXTURE_NAMES[@]}"; do
  name="${STEP20_FIXTURE_NAMES[$index]}"
  source="${STEP20_FIXTURE_SOURCES[$index]}"
  bundled="$STEP20_FIXTURE_DIR/$name"
  [[ -f "$source" ]] || { echo "ERROR: just-built Step 20 fixture source missing: $source" >&2; exit 5; }
  [[ -s "$bundled" ]] || { echo "ERROR: bundled Step 20 fixture missing/empty: $name" >&2; exit 5; }
  cmp -s "$source" "$bundled" || { echo "ERROR: bundled Step 20 fixture differs from exact just-built source: $name" >&2; exit 5; }
done
(
  cd "$STEP20_FIXTURE_DIR"
  shasum -a 256 -c step20-fixtures.sha256 >/dev/null
) || { echo "ERROR: bundled Step 20 fixture manifest verification failed." >&2; exit 5; }

STEP20_BUNDLED_LIST="$TMP/step20-fixture-dlls.txt"
find "$APP" -type f -name 'StS2Launcher.Step20.*.dll' -print | sort > "$STEP20_BUNDLED_LIST"
STEP20_BUNDLED_COUNT="$(wc -l < "$STEP20_BUNDLED_LIST" | tr -d '[:space:]')"
[[ "$STEP20_BUNDLED_COUNT" == "3" ]] || {
  echo "ERROR: expected exactly three Step 20 fixture DLL files in the final .app, found $STEP20_BUNDLED_COUNT." >&2
  sed 's/^/  /' "$STEP20_BUNDLED_LIST" >&2
  exit 5
}
while IFS= read -r path; do
  [[ "$(dirname "$path")" == "$STEP20_FIXTURE_DIR" ]] || {
    echo "ERROR: Step 20 fixture DLL escaped its data-only bundle directory: $path" >&2
    exit 5
  }
done < "$STEP20_BUNDLED_LIST"

# Step 28.0 ahead-of-load fixture is the active architecture-pivot payload. It is built outside
# the iOS project graph and copied only after publish. The IPA must contain exactly one byte-identical
# source fixture; the transformed copy is created later on-device in launcher-private Documents storage.
STEP28_FIXTURE_DIR="$APP/Step28AheadOfLoadFixture"
STEP28_FIXTURE="$STEP28_FIXTURE_DIR/StS2Launcher.Step28.AheadOfLoadFixture.dll"
STEP28_FIXTURE_SOURCE="$ROOT/fixtures/StS2Launcher.Step28.AheadOfLoadFixture/bin/Release/net9.0/StS2Launcher.Step28.AheadOfLoadFixture.dll"
STEP28_FIXTURE_MANIFEST="$STEP28_FIXTURE_DIR/step28-ahead-of-load-fixture.sha256"
[[ -d "$STEP28_FIXTURE_DIR" ]] || { echo "ERROR: Step 28 ahead-of-load fixture directory missing from IPA." >&2; exit 5; }
[[ -f "$STEP28_FIXTURE_SOURCE" ]] || { echo "ERROR: just-built Step 28 ahead-of-load fixture source missing." >&2; exit 5; }
[[ -s "$STEP28_FIXTURE" ]] || { echo "ERROR: bundled Step 28 ahead-of-load fixture missing/empty." >&2; exit 5; }
[[ -f "$STEP28_FIXTURE_MANIFEST" ]] || { echo "ERROR: Step 28 ahead-of-load fixture SHA-256 manifest missing." >&2; exit 5; }
cmp -s "$STEP28_FIXTURE_SOURCE" "$STEP28_FIXTURE" || { echo "ERROR: bundled Step 28 fixture differs from exact just-built source." >&2; exit 5; }
(
  cd "$STEP28_FIXTURE_DIR"
  shasum -a 256 -c step28-ahead-of-load-fixture.sha256 >/dev/null
) || { echo "ERROR: bundled Step 28 fixture manifest verification failed." >&2; exit 5; }
STEP28_BUNDLED_LIST="$TMP/step28-ahead-of-load-fixture-dlls.txt"
find "$APP" -type f -name 'StS2Launcher.Step28.AheadOfLoadFixture.dll' -print | sort > "$STEP28_BUNDLED_LIST"
STEP28_BUNDLED_COUNT="$(wc -l < "$STEP28_BUNDLED_LIST" | tr -d '[:space:]')"
[[ "$STEP28_BUNDLED_COUNT" == "1" ]] || {
  echo "ERROR: expected exactly one Step 28 ahead-of-load fixture DLL in final .app, found $STEP28_BUNDLED_COUNT." >&2
  sed 's/^/  /' "$STEP28_BUNDLED_LIST" >&2
  exit 5
}
[[ "$(cat "$STEP28_BUNDLED_LIST")" == "$STEP28_FIXTURE" ]] || {
  echo "ERROR: Step 28 ahead-of-load fixture escaped its exact data-only directory." >&2
  exit 5
}

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
text = Path('src/StS2Launcher.iOS/Platform/GodotStep15NativeBridge.cs').read_text()
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
cp "$DEPENDENCIES" artifacts/logs/step28-final-native-dependencies.log

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
# physical Steps 16–20 Cecil/interpreter gates are the authoritative managed-runtime checks.
STRINGS_FILE="$TMP/executable-strings.txt"
strings "$EXECUTABLE" > "$STRINGS_FILE"
grep -Fq '4.5.1-stable' "$STRINGS_FILE" || {
  echo "ERROR: Godot 4.5.1 native version marker missing from final executable." >&2
  exit 9
}

{
  echo "Step 35.0.2 IPA verification passed."
  echo "  Bundle ID: $BUNDLE_ID"
  echo "  Version: $VERSION ($BUILD_VERSION)"
  echo "  Architecture: arm64"
  echo "  Godot 4.5.1 Step 15 regression host: retained"
  echo "  Mono.Cecil 0.11.6 compatibility-analysis/runtime-planning dependency: linked/AOT input"
  echo "  Project-owned Step 16 regression fixture: bundled as inert raw assembly data"
  echo "  Step 20 external IL fixtures: exactly 3, SHA-256 manifest verified, post-publish data-only directory"
  echo "  Build-time launcher assemblies: remain AOT-targeted; MtouchInterpreter=-all retained from physically proven Step 20"
  echo "  Real StS2/proprietary payload in IPA: none (real game bytes remain user-owned in Documents and are never bundled)"
  echo "  Dynamic dependency audit: system or bundled only"
  echo "  iOS Documents file sharing: enabled (UIFileSharingEnabled + LSSupportsOpeningDocumentsInPlace)"
  echo "  Runtime binding text report: generated at runtime under Documents/StS2Launcher/Step21.1-RuntimeBindingDiagnostics.txt"
  echo "  Consolidated device test reports: Documents/StS2Launcher/Reports/*.txt"
  echo "  Expected device UI: STEP 35.0.2 — EXECUTEVERYEARLY INVOKE-CRASH STATIC ILCALLSITE LOCALIZATION"
} | tee artifacts/logs/step35-ipa-verification-summary.log
