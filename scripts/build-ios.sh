#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
source scripts/lib/current-release.sh
[[ "$(uname -s)" == "Darwin" ]] || { echo "ERROR: iOS builds require macOS/Xcode." >&2; exit 2; }
mkdir -p artifacts/reports artifacts/logs artifacts/test-results
if [[ "${STS2_SKIP_STATIC_VALIDATION:-0}" != "1" ]]; then
  bash scripts/validate.sh
fi

APP="$ROOT/artifacts/publish/$STS2_APP_BUNDLE_NAME"
IPA="$ROOT/$STS2_IPA_REL"
PROJECT="$STS2_IOS_PROJECT"
PATCHER="tools/StS2Launcher.SteamKitIosPatcher/StS2Launcher.SteamKitIosPatcher.csproj"
FIXTURE_PROJECT="fixtures/StS2Launcher.Step16.Fixture/StS2Launcher.Step16.Fixture.csproj"
FIXTURE_DLL="$ROOT/fixtures/StS2Launcher.Step16.Fixture/bin/Release/net9.0/StS2Launcher.Step16.Fixture.dll"
STEP20_DYNAMIC_DLL="$ROOT/fixtures/StS2Launcher.Step20.DynamicFixture/bin/Release/net9.0/StS2Launcher.Step20.DynamicFixture.dll"
STEP20_DEPENDENCY_DLL="$ROOT/fixtures/StS2Launcher.Step20.DependencyFixture/bin/Release/net9.0/StS2Launcher.Step20.DependencyFixture.dll"
STEP20_ROOT_DLL="$ROOT/fixtures/StS2Launcher.Step20.RootFixture/bin/Release/net9.0/StS2Launcher.Step20.RootFixture.dll"
PUBLISH_LOG="artifacts/logs/ios-publish.log"
PATCH_LOG="artifacts/logs/steamkit-ios-patch.log"

rm -rf artifacts/publish artifacts/Payload
mkdir -p artifacts/publish artifacts/logs
export NUGET_PACKAGES="$ROOT/.nuget/packages"
rm -rf "$NUGET_PACKAGES/steamkit2/3.4.0"
mkdir -p "$NUGET_PACKAGES"
SDK_ROOT="$(xcrun --sdk iphoneos --show-sdk-path)"
[[ ! -d "$SDK_ROOT/System/Library/Frameworks/DiskArbitration.framework" ]] || { echo "ERROR: DiskArbitration unexpectedly exists in iPhoneOS SDK." >&2; exit 3; }

echo "Restoring iOS project and build-only SteamKit patcher..."
dotnet restore "$PROJECT" 2>&1 | tee artifacts/logs/ios-restore.log
dotnet restore "$PATCHER" 2>&1 | tee artifacts/logs/patcher-restore.log

STEAMKIT_DLL="$NUGET_PACKAGES/steamkit2/3.4.0/lib/net8.0/SteamKit2.dll"
[[ -f "$STEAMKIT_DLL" ]] || { echo "ERROR: restored SteamKit2 assembly missing: $STEAMKIT_DLL" >&2; exit 6; }
{
  echo "=== SteamKit iOS compatibility patch ==="
  echo "Input: $STEAMKIT_DLL"
  echo "Before SHA-256: $(shasum -a 256 "$STEAMKIT_DLL" | awk '{print $1}')"
} | tee "$PATCH_LOG"
dotnet run --project "$PATCHER" -c Release --no-restore -- "$STEAMKIT_DLL" 2>&1 | tee -a "$PATCH_LOG"
echo "After SHA-256: $(shasum -a 256 "$STEAMKIT_DLL" | awk '{print $1}')" | tee -a "$PATCH_LOG"
grep -Fq 'STEP05.16 STEAMKIT IOS PATCH: PASS' "$PATCH_LOG" || { echo "ERROR: SteamKit patch did not pass." >&2; exit 7; }
grep -Eq '^Replacement count: [01]$' "$PATCH_LOG" || { echo "ERROR: unexpected SteamKit patch replacement count." >&2; exit 7; }

# Host tests normally built these once already. Standalone builds fill only missing fixture outputs.
if [[ ! -f "$FIXTURE_DLL" ]]; then
  dotnet build "$FIXTURE_PROJECT" -c Release --nologo
fi
for project in \
  fixtures/StS2Launcher.Step20.DynamicFixture/StS2Launcher.Step20.DynamicFixture.csproj \
  fixtures/StS2Launcher.Step20.DependencyFixture/StS2Launcher.Step20.DependencyFixture.csproj \
  fixtures/StS2Launcher.Step20.RootFixture/StS2Launcher.Step20.RootFixture.csproj; do
  dll="${project%/*.csproj}/bin/Release/net9.0/$(basename "${project%.csproj}").dll"
  [[ -f "$dll" ]] || dotnet build "$project" -c Release --nologo
done
for fixture in "$FIXTURE_DLL" "$STEP20_DYNAMIC_DLL" "$STEP20_DEPENDENCY_DLL" "$STEP20_ROOT_DLL"; do
  [[ -f "$fixture" ]] || { echo "ERROR: required project-owned fixture missing: $fixture" >&2; exit 11; }
done

bash scripts/build-godot.sh

echo "Publishing Step 22.4.1 Canonical Foundation Test Fix..."
set +e
dotnet publish "$PROJECT" --no-restore -c Release -f net9.0-ios -r ios-arm64 \
  -p:BuildIpa=false -p:EnableCodeSigning=false -p:CodesignKey="" -p:CodesignProvision="" \
  -p:AppBundleDir="$APP" -bl:artifacts/logs/dotnet-ios.binlog 2>&1 | tee "$PUBLISH_LOG"
status=${PIPESTATUS[0]}
set -e
[[ "$status" == "0" ]] || exit "$status"

grep -Fq "$STS2_RUNTIME_POLICY_MARKER MtouchInterpreter=-all" "$PUBLISH_LOG" || { echo "ERROR: runtime-policy telemetry missing." >&2; exit 15; }
if grep -F "$STS2_RUNTIME_POLICY_MARKER" "$PUBLISH_LOG" | grep -Fq 'UseInterpreter=true'; then echo "ERROR: broad UseInterpreter=true policy resolved." >&2; exit 15; fi
if grep -F "$STS2_RUNTIME_POLICY_MARKER" "$PUBLISH_LOG" | grep -Fq 'PublishAot=true'; then echo "ERROR: NativeAOT unexpectedly enabled." >&2; exit 15; fi
BEFORE_LINE="$(grep 'STEP05.2 LINKER FRAMEWORKS BEFORE:' "$PUBLISH_LOG" | tail -1 || true)"
AFTER_LINE="$(grep 'STEP05.2 LINKER FRAMEWORKS AFTER:' "$PUBLISH_LOG" | tail -1 || true)"
[[ "$BEFORE_LINE" == *DiskArbitration* && "$AFTER_LINE" != *DiskArbitration* ]] || { echo "ERROR: DiskArbitration filter regression." >&2; exit 8; }
[[ -d "$APP" ]] || { echo "ERROR: expected app bundle missing: $APP" >&2; exit 9; }

rm -rf "$APP/Step15GodotSmokeProject" "$APP/Step16Fixtures" "$APP/Step20DynamicFixtures"
cp -R "$ROOT/native/step15/smoke_project" "$APP/Step15GodotSmokeProject"
mkdir -p "$APP/Step16Fixtures" "$APP/Step20DynamicFixtures"
cp "$FIXTURE_DLL" "$APP/Step16Fixtures/StS2Launcher.Step16.Fixture.dll"
cp "$STEP20_DYNAMIC_DLL" "$APP/Step20DynamicFixtures/StS2Launcher.Step20.DynamicFixture.dll"
cp "$STEP20_DEPENDENCY_DLL" "$APP/Step20DynamicFixtures/StS2Launcher.Step20.DependencyFixture.dll"
cp "$STEP20_ROOT_DLL" "$APP/Step20DynamicFixtures/StS2Launcher.Step20.RootFixture.dll"
(
  cd "$APP/Step20DynamicFixtures"
  shasum -a 256 StS2Launcher.Step20.DynamicFixture.dll StS2Launcher.Step20.DependencyFixture.dll StS2Launcher.Step20.RootFixture.dll > step20-fixtures.sha256
)

mkdir -p artifacts/Payload
cp -R "$APP" artifacts/Payload/
(
  cd artifacts
  rm -f "$(basename "$STS2_IPA_REL")"
  /usr/bin/zip -qry "$(basename "$STS2_IPA_REL")" Payload
)
echo "Created $IPA"
