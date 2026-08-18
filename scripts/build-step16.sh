#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: iOS device builds require macOS/Xcode." >&2
  exit 2
fi

bash scripts/validate-step16.sh

rm -rf artifacts/publish artifacts/Payload
mkdir -p artifacts/publish artifacts/logs

APP="$ROOT/artifacts/publish/StS2Launcher.Step05.iOS.app"
IPA="$ROOT/artifacts/StS2-Launcher-Step-16.1.ipa"
PROJECT="src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"
PATCHER="tools/StS2Launcher.SteamKitIosPatcher/StS2Launcher.SteamKitIosPatcher.csproj"
FIXTURE_PROJECT="fixtures/StS2Launcher.Step16.Fixture/StS2Launcher.Step16.Fixture.csproj"
FIXTURE_DLL="$ROOT/fixtures/StS2Launcher.Step16.Fixture/bin/Release/net9.0/StS2Launcher.Step16.Fixture.dll"
PUBLISH_LOG="artifacts/logs/step16-publish.log"
PATCH_LOG="artifacts/logs/step16-steamkit-patch.log"

# Patch only a disposable repository-local SteamKit package copy.
export NUGET_PACKAGES="$ROOT/.nuget/packages"
rm -rf "$NUGET_PACKAGES/steamkit2/3.4.0"
mkdir -p "$NUGET_PACKAGES"

SDK_ROOT="$(xcrun --sdk iphoneos --show-sdk-path)"
if [[ -d "$SDK_ROOT/System/Library/Frameworks/DiskArbitration.framework" ]]; then
  echo "ERROR: DiskArbitration.framework unexpectedly exists in iPhoneOS SDK." >&2
  exit 3
fi

echo "Restoring Step 16 projects into isolated NuGet package root..."
dotnet restore "$PROJECT" 2>&1 | tee artifacts/logs/step16-restore.log
dotnet restore "$PATCHER" 2>&1 | tee artifacts/logs/step16-patcher-restore.log
dotnet restore "$FIXTURE_PROJECT" 2>&1 | tee artifacts/logs/step16-fixture-restore.log

STEAMKIT_DLL="$NUGET_PACKAGES/steamkit2/3.4.0/lib/net8.0/SteamKit2.dll"
if [[ ! -f "$STEAMKIT_DLL" ]]; then
  echo "ERROR: restored SteamKit2 3.4.0 assembly not found: $STEAMKIT_DLL" >&2
  exit 6
fi

{
  echo "=== Step 16 SteamKit iOS compatibility patch ==="
  echo "Input: $STEAMKIT_DLL"
  if command -v shasum >/dev/null 2>&1; then
    echo "Before SHA-256: $(shasum -a 256 "$STEAMKIT_DLL" | awk '{print $1}')"
  fi
} | tee "$PATCH_LOG"

dotnet run --project "$PATCHER" -c Release --no-restore -- "$STEAMKIT_DLL" \
  2>&1 | tee -a "$PATCH_LOG"

if command -v shasum >/dev/null 2>&1; then
  echo "After SHA-256: $(shasum -a 256 "$STEAMKIT_DLL" | awk '{print $1}')" \
    | tee -a "$PATCH_LOG"
fi

for required in \
  'STEP05.16 STEAMKIT IOS PATCH: PASS' \
  'Assembly: SteamKit2 3.4.0' \
  'Process.StartTime status:'; do
  if ! grep -Fq "$required" "$PATCH_LOG"; then
    echo "ERROR: SteamKit patch telemetry missing: $required" >&2
    exit 7
  fi
done

if ! grep -Eq '^Replacement count: [01]$' "$PATCH_LOG"; then
  echo "ERROR: SteamKit patch telemetry must report replacement count 0 or 1." >&2
  exit 7
fi

echo "Building project-owned Step 16 managed fixture..."
dotnet build "$FIXTURE_PROJECT" -c Release --no-restore 2>&1 | tee artifacts/logs/step16-fixture-build.log
[[ -f "$FIXTURE_DLL" ]] || { echo "ERROR: Step 16 fixture assembly missing after build: $FIXTURE_DLL" >&2; exit 11; }

echo "Building/restoring pinned Godot 4.5.1 iOS static host (Step 15 regression dependency)..."
bash scripts/build-godot-step15.sh

echo "Publishing Step 16 Managed Preparation Foundation build..."
set +e
dotnet publish "$PROJECT" \
  --no-restore \
  -c Release \
  -f net9.0-ios \
  -r ios-arm64 \
  -p:BuildIpa=false \
  -p:EnableCodeSigning=false \
  -p:CodesignKey="" \
  -p:CodesignProvision="" \
  -p:AppBundleDir="$APP" \
  -bl:artifacts/logs/step16-dotnet-ios.binlog \
  2>&1 | tee "$PUBLISH_LOG"
PUBLISH_STATUS=${PIPESTATUS[0]}
set -e

if [[ "$PUBLISH_STATUS" != "0" ]]; then
  echo "=== Step 16 publish failed: focused scan ===" \
    | tee artifacts/logs/step16-failure-scan.log
  grep -E -m 160 \
    '(^|: )(error|fatal error)|undefined symbol|Undefined symbols|framework .+ not found|DiskArbitration|PlatformNotSupported|Authentication' \
    "$PUBLISH_LOG" | tee -a artifacts/logs/step16-failure-scan.log || true
  exit "$PUBLISH_STATUS"
fi

BEFORE_LINE="$(grep 'STEP05.2 LINKER FRAMEWORKS BEFORE:' "$PUBLISH_LOG" | tail -1 || true)"
AFTER_LINE="$(grep 'STEP05.2 LINKER FRAMEWORKS AFTER:' "$PUBLISH_LOG" | tail -1 || true)"
if [[ -z "$BEFORE_LINE" || -z "$AFTER_LINE" ]]; then
  echo "ERROR: framework-filter telemetry was not emitted." >&2
  exit 8
fi
if [[ "$BEFORE_LINE" != *"DiskArbitration"* || "$AFTER_LINE" == *"DiskArbitration"* ]]; then
  echo "ERROR: proven DiskArbitration filter did not behave as expected." >&2
  exit 8
fi

if [[ ! -d "$APP" ]]; then
  echo "ERROR: publish completed but expected .app was not created: $APP" >&2
  exit 9
fi

SMOKE_DIR="$APP/Step15GodotSmokeProject"
rm -rf "$SMOKE_DIR"
cp -R "$ROOT/native/step15/smoke_project" "$SMOKE_DIR"
for required in project.godot Main.tscn Step15Smoke.gd; do
  [[ -f "$SMOKE_DIR/$required" ]] || { echo "ERROR: bundled Step 15 regression smoke-project file missing: $required" >&2; exit 10; }
done

FIXTURE_DIR="$APP/Step16Fixtures"
rm -rf "$FIXTURE_DIR"
mkdir -p "$FIXTURE_DIR"
cp "$FIXTURE_DLL" "$FIXTURE_DIR/StS2Launcher.Step16.Fixture.dll"
[[ -f "$FIXTURE_DIR/StS2Launcher.Step16.Fixture.dll" ]] || { echo "ERROR: bundled Step 16 fixture missing after copy." >&2; exit 12; }

mkdir -p artifacts/Payload
cp -R "$APP" artifacts/Payload/
(
  cd artifacts
  rm -f StS2-Launcher-Step-16.1.ipa
  /usr/bin/zip -qry StS2-Launcher-Step-16.1.ipa Payload
)

echo "Created $IPA"
