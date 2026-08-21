#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
source scripts/lib/current-release.sh
mkdir -p artifacts/reports artifacts/logs artifacts/test-results
DOTNET_SDK_VERSION="${DOTNET_SDK_VERSION:-9.0.314}"
DOTNET_WORKLOAD_SET="${DOTNET_WORKLOAD_SET:-9.0.314.3}"
DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export DOTNET_ROOT PATH="$DOTNET_ROOT:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 NUGET_XMLDOC_MODE=skip
[[ "$(uname -s)" == "Darwin" ]] || { echo "ERROR: Codemagic iOS build must run on macOS." >&2; exit 2; }

{
  echo "StS2 Launcher — Step 25 Controlled Harmony API Resolution + Type Initialization + Instance Construction Boundary build environment"
  date -u
  uname -a
  xcodebuild -version
  xcrun --sdk iphoneos --show-sdk-version
  sw_vers
  python3 --version
} | tee artifacts/reports/build-environment.txt

if [[ ! -x "$DOTNET_ROOT/dotnet" || "$($DOTNET_ROOT/dotnet --version 2>/dev/null || true)" != "$DOTNET_SDK_VERSION" ]]; then
  rm -rf "$DOTNET_ROOT"; mkdir -p "$DOTNET_ROOT"
  installer="$(mktemp)"
  curl -fsSL --retry 4 --retry-delay 3 --retry-all-errors https://dot.net/v1/dotnet-install.sh -o "$installer"
  bash "$installer" --version "$DOTNET_SDK_VERSION" --install-dir "$DOTNET_ROOT" --no-path
  rm -f "$installer"
fi
[[ "$(dotnet --version)" == "$DOTNET_SDK_VERSION" ]] || { echo "ERROR: unexpected .NET SDK $(dotnet --version)" >&2; exit 3; }

bash scripts/validate.sh
bash scripts/test.sh

workload_cwd="$(mktemp -d)"
trap 'rm -rf "$workload_cwd"' EXIT
(
  cd "$workload_cwd"
  ok=0
  for attempt in 1 2; do
    if "$DOTNET_ROOT/dotnet" workload install ios --version "$DOTNET_WORKLOAD_SET"; then ok=1; break; fi
    sleep $((attempt * 5))
  done
  [[ "$ok" == 1 ]] || exit 4
  "$DOTNET_ROOT/dotnet" workload --info
) | tee artifacts/reports/ios-workload.txt
rm -rf "$workload_cwd"; trap - EXIT

STS2_SKIP_STATIC_VALIDATION=1 bash scripts/build-ios.sh 2>&1 | tee artifacts/reports/ios-build.txt
bash scripts/verify-ipa.sh "$STS2_IPA_REL"

{
  echo "StS2 Launcher iOS — Step 25 Controlled Harmony API Resolution + Type Initialization + Instance Construction Boundary"
  echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
  echo "Commit: ${CM_COMMIT:-unknown}"
  echo "Branch: ${CM_BRANCH:-unknown}"
  echo "Xcode: $(xcodebuild -version | tr '\n' ' ')"
  echo ".NET SDK: $(dotnet --version)"
  echo "iOS workload set: $DOTNET_WORKLOAD_SET"
  echo "Static validation: PASS"
  echo "Host unit tests: PASS"
  echo "IPA verification: PASS"
  echo "Physically proven Step 22.2 Core behavior: byte-for-byte protected by manifest"
  echo "Device text reports: Documents/StS2Launcher/Reports/*.txt"
  echo "Step 23 production behavior: first real sts2.dll CLR load is available only as an explicit on-device gate; build/CI never bundles or loads game payload"
  echo "IPA: $STS2_IPA_REL"
  echo "IPA SHA-256: $(shasum -a 256 "$STS2_IPA_REL" | awk '{print $1}')"
} | tee artifacts/reports/build-summary.txt
