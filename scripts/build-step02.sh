#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: iOS device builds require macOS/Xcode." >&2
  exit 2
fi

bash scripts/validate-step02.sh

rm -rf artifacts/publish artifacts/Payload
mkdir -p artifacts/publish artifacts/logs

APP="$ROOT/artifacts/publish/StS2Launcher.Step02.iOS.app"
IPA="$ROOT/artifacts/StS2-Launcher-Step-02.ipa"

echo "Publishing Step 02..."
dotnet publish src/StS2Launcher.Step02.iOS/StS2Launcher.Step02.iOS.csproj \
  -c Release \
  -f net9.0-ios \
  -r ios-arm64 \
  -p:BuildIpa=false \
  -p:EnableCodeSigning=false \
  -p:CodesignKey="" \
  -p:CodesignProvision="" \
  -p:AppBundleDir="$APP" \
  -bl:artifacts/logs/step02-dotnet-ios.binlog

if [[ ! -d "$APP" ]]; then
  echo "ERROR: publish completed but the expected .app was not created:" >&2
  echo "  $APP" >&2
  find "$ROOT/src/StS2Launcher.Step02.iOS" -type d -name '*.app' -print >&2 || true
  exit 4
fi

mkdir -p artifacts/Payload
cp -R "$APP" artifacts/Payload/

(
  cd artifacts
  rm -f StS2-Launcher-Step-02.ipa
  /usr/bin/zip -qry StS2-Launcher-Step-02.ipa Payload
)

echo "Created $IPA"
