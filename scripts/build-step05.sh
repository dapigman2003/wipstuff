#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: iOS device builds require macOS/Xcode." >&2
  exit 2
fi

bash scripts/validate-step05.sh

rm -rf artifacts/publish artifacts/Payload
mkdir -p artifacts/publish artifacts/logs

APP="$ROOT/artifacts/publish/StS2Launcher.Step05.iOS.app"
IPA="$ROOT/artifacts/StS2-Launcher-Step-05.1.ipa"
PROJECT="src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"

echo "=== Step 05.1 native-framework preflight ===" \
  | tee artifacts/logs/step05-1-native-preflight.log

SDK_ROOT="$(xcrun --sdk iphoneos --show-sdk-path)"
echo "iPhoneOS SDK: $SDK_ROOT" \
  | tee -a artifacts/logs/step05-1-native-preflight.log

if [[ -d "$SDK_ROOT/System/Library/Frameworks/DiskArbitration.framework" ]]; then
  echo "DiskArbitration.framework unexpectedly exists in iPhoneOS SDK." \
    | tee -a artifacts/logs/step05-1-native-preflight.log
else
  echo "Confirmed: DiskArbitration.framework is NOT in the active iPhoneOS SDK." \
    | tee -a artifacts/logs/step05-1-native-preflight.log
fi

echo "SteamKit2 NuGet binaries containing the string 'DiskArbitration' before trimming:" \
  | tee -a artifacts/logs/step05-1-native-preflight.log

FOUND_BEFORE=0
STEAMKIT_PACKAGE="$HOME/.nuget/packages/steamkit2/3.3.1"
if [[ -d "$STEAMKIT_PACKAGE" ]]; then
  while IFS= read -r file; do
    if strings "$file" 2>/dev/null | grep -q 'DiskArbitration'; then
      echo "  $file" | tee -a artifacts/logs/step05-1-native-preflight.log
      FOUND_BEFORE=1
    fi
  done < <(find "$STEAMKIT_PACKAGE" -type f \( -name '*.dll' -o -name '*.xml' \) -print)
fi

if [[ "$FOUND_BEFORE" == "0" ]]; then
  echo "  (none found in SteamKit2 package files by simple strings scan)" \
    | tee -a artifacts/logs/step05-1-native-preflight.log
fi

echo
echo "Publishing Step 05.1 with TrimMode=full..."

set +e
dotnet publish "$PROJECT" \
  -c Release \
  -f net9.0-ios \
  -r ios-arm64 \
  -p:BuildIpa=false \
  -p:EnableCodeSigning=false \
  -p:CodesignKey="" \
  -p:CodesignProvision="" \
  -p:AppBundleDir="$APP" \
  -bl:artifacts/logs/step05-1-dotnet-ios.binlog \
  2>&1 | tee artifacts/logs/step05-1-publish.log
PUBLISH_STATUS=${PIPESTATUS[0]}
set -e

if [[ "$PUBLISH_STATUS" != "0" ]]; then
  {
    echo
    echo "=== Step 05.1 publish failed: DiskArbitration survival scan ==="
    echo "Searching relevant build/package files for the literal native framework name."
  } | tee artifacts/logs/step05-1-failure-scan.log

  SCAN_ROOTS=(
    "$ROOT/src/StS2Launcher.Step05.iOS/obj"
    "$ROOT/src/StS2Launcher.Core/obj"
    "$HOME/.nuget/packages/steamkit2/3.3.1"
    "$HOME/.nuget/packages/microsoft.win32.registry"
  )

  MATCHES=0
  for scan_root in "${SCAN_ROOTS[@]}"; do
    [[ -d "$scan_root" ]] || continue

    while IFS= read -r file; do
      if strings "$file" 2>/dev/null | grep -q 'DiskArbitration'; then
        echo "CONTAINS DiskArbitration: $file" \
          | tee -a artifacts/logs/step05-1-failure-scan.log
        MATCHES=$((MATCHES + 1))
      fi
    done < <(
      find "$scan_root" -type f \
        \( -name '*.dll' -o -name '*.xml' -o -name '*.json' -o -name '*.rsp' \
           -o -name '*.txt' -o -name '*.a' -o -name '*.cs' \) \
        -print 2>/dev/null
    )
  done

  if [[ "$MATCHES" == "0" ]]; then
    echo "No literal DiskArbitration string found by the post-failure scan." \
      | tee -a artifacts/logs/step05-1-failure-scan.log
  fi

  echo "dotnet publish exit code: $PUBLISH_STATUS" \
    | tee -a artifacts/logs/step05-1-failure-scan.log

  exit "$PUBLISH_STATUS"
fi

if [[ ! -d "$APP" ]]; then
  echo "ERROR: publish completed but the expected .app was not created:" >&2
  echo "  $APP" >&2
  find "$ROOT/src/StS2Launcher.Step05.iOS" -type d -name '*.app' -print >&2 || true
  exit 4
fi

mkdir -p artifacts/Payload
cp -R "$APP" artifacts/Payload/

(
  cd artifacts
  rm -f StS2-Launcher-Step-05.1.ipa
  /usr/bin/zip -qry StS2-Launcher-Step-05.1.ipa Payload
)

echo "Created $IPA"
