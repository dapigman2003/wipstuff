#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
mkdir -p artifacts/logs artifacts/test-results

DOTNET_SDK_VERSION="${DOTNET_SDK_VERSION:-9.0.314}"
DOTNET_WORKLOAD_SET="${DOTNET_WORKLOAD_SET:-9.0.314.3}"
DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"

export DOTNET_ROOT
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export NUGET_XMLDOC_MODE=skip

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: Codemagic iOS build must run on a macOS worker." >&2
  exit 2
fi

{
  echo "=== StS2 Launcher Step 08 environment ==="
  date -u
  uname -a
  xcodebuild -version
  xcrun --sdk iphoneos --show-sdk-version
  sw_vers
} | tee artifacts/logs/step08-environment.log

NEED_DOTNET=1
if [[ -x "$DOTNET_ROOT/dotnet" ]]; then
  INSTALLED="$("$DOTNET_ROOT/dotnet" --version 2>/dev/null || true)"
  [[ "$INSTALLED" == "$DOTNET_SDK_VERSION" ]] && NEED_DOTNET=0
fi

if [[ "$NEED_DOTNET" == "1" ]]; then
  echo "Installing .NET SDK $DOTNET_SDK_VERSION..."
  rm -rf "$DOTNET_ROOT"
  mkdir -p "$DOTNET_ROOT"
  INSTALLER="$(mktemp)"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$INSTALLER"
  bash "$INSTALLER" --version "$DOTNET_SDK_VERSION" --install-dir "$DOTNET_ROOT" --no-path
  rm -f "$INSTALLER"
fi

[[ "$(dotnet --version)" == "$DOTNET_SDK_VERSION" ]] || {
  echo "ERROR: unexpected .NET SDK version: $(dotnet --version)" >&2
  exit 3
}

bash scripts/validate-step08.sh | tee artifacts/logs/step08-validation.log
bash scripts/run-unit-tests.sh

WORKLOAD_CWD="$(mktemp -d)"
trap 'rm -rf "$WORKLOAD_CWD"' EXIT
(
  cd "$WORKLOAD_CWD"
  "$DOTNET_ROOT/dotnet" workload install ios --version "$DOTNET_WORKLOAD_SET"
  "$DOTNET_ROOT/dotnet" workload --info
) | tee artifacts/logs/step08-workload.log
rm -rf "$WORKLOAD_CWD"
trap - EXIT

bash scripts/build-step08.sh 2>&1 | tee artifacts/logs/step08-wrapper.log
bash scripts/verify-step08-ipa.sh artifacts/StS2-Launcher-Step-08.ipa \
  2>&1 | tee artifacts/logs/step08-ipa-verification.log

{
  echo "StS2 Launcher iOS — Step 08 depot / manifest discovery"
  echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
  echo "Commit: ${CM_COMMIT:-unknown}"
  echo "Branch: ${CM_BRANCH:-unknown}"
  echo "Xcode: $(xcodebuild -version | tr '\n' ' ')"
  echo ".NET SDK: $(dotnet --version)"
  echo "iOS workload set requested: $DOTNET_WORKLOAD_SET"
  echo "Host unit tests: PASS (required before publish)"
  echo "SteamKit: 3.4.0"
  echo "Foundation regression: retained"
  echo "Fix boundary: PICS depot / visible manifest-ID discovery for App ID 2868840"
  echo "Steam Guard: Step 06.1 mobile approval retained; no manual code entry"
  echo "Credential persistence: refresh token + identity in iOS Keychain; ShouldRememberPassword=true; password/Guard secrets never stored"
  echo "Ownership: Step 07 ticket gate retained and re-proven before PICS metadata"
  echo "Discovery: PICS access metadata + product info only; depot keys/manifest bodies/CDN/files not implemented"
  echo "IPA: artifacts/StS2-Launcher-Step-08.ipa"
  if command -v shasum >/dev/null 2>&1; then
    echo "IPA SHA-256: $(shasum -a 256 artifacts/StS2-Launcher-Step-08.ipa | awk '{print $1}')"
  fi
} > artifacts/step08-build-summary.txt

cat artifacts/step08-build-summary.txt
