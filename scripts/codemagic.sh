#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
source scripts/lib/current-release.sh
mkdir -p artifacts/reports artifacts/logs artifacts/test-results
DOTNET_SDK_VERSION="${DOTNET_SDK_VERSION:-9.0.314}"
DOTNET_WORKLOAD_SET="${DOTNET_WORKLOAD_SET:-9.0.314.3}"
DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export DOTNET_ROOT PATH="$DOTNET_ROOT:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 NUGET_XMLDOC_MODE=skip DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=1
[[ "$(uname -s)" == "Darwin" ]] || { echo "ERROR: Codemagic iOS build must run on macOS." >&2; exit 2; }

# Cache telemetry is evidence only. It never changes build inputs or bypasses validation.
IOS_CACHE_ROOT="$ROOT/src/StS2Launcher.iOS/obj/Release/net9.0-ios/ios-arm64"
AOT_CACHE_DIR="$IOS_CACHE_ROOT/nativelibraries/aot-output"
CACHE_REPORT="artifacts/reports/cache-state.txt"
cache_path_line() {
  local label="$1" path="$2"
  if [[ -e "$path" ]]; then
    local size
    size="$(du -sh "$path" 2>/dev/null | awk '{print $1}' || true)"
    echo "$label: RESTORED/PRESENT size=${size:-unknown} path=$path"
  else
    echo "$label: COLD/MISSING path=$path"
  fi
}
{
  echo "StS2 Launcher — Codemagic cache state before Step 36 build"
  echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
  cache_path_line "Home NuGet" "$HOME/.nuget/packages"
  cache_path_line "Isolated iOS NuGet" "$ROOT/.nuget/packages"
  cache_path_line "Godot Step 15" "$HOME/.cache/sts2launcher/godot-step15"
  cache_path_line "Pinned .NET SDK/workloads" "$DOTNET_ROOT"
  cache_path_line "iOS arm64 obj" "$IOS_CACHE_ROOT"
  if [[ -d "$AOT_CACHE_DIR" ]]; then
    echo "AOT output files before build: $(find "$AOT_CACHE_DIR" -type f | wc -l | tr -d ' ')"
  else
    echo "AOT output files before build: 0"
  fi
} | tee "$CACHE_REPORT"

BUILD_START_EPOCH="$(date +%s)"
elapsed_seconds() {
  local start="$1"
  echo $(( $(date +%s) - start ))
}

{
  echo "StS2 Launcher — Step 35.0.31 / Step 36.0.2 Exact ExecuteEssential Failure-Chain Capture build environment"
  date -u
  uname -a
  xcodebuild -version
  xcrun --sdk iphoneos --show-sdk-version
  sw_vers
  python3 --version
} | tee artifacts/reports/build-environment.txt

SDK_START_EPOCH="$(date +%s)"
if [[ ! -x "$DOTNET_ROOT/dotnet" || "$($DOTNET_ROOT/dotnet --version 2>/dev/null || true)" != "$DOTNET_SDK_VERSION" ]]; then
  rm -rf "$DOTNET_ROOT"; mkdir -p "$DOTNET_ROOT"
  installer="$(mktemp)"
  curl -fsSL --retry 4 --retry-delay 3 --retry-all-errors https://dot.net/v1/dotnet-install.sh -o "$installer"
  bash "$installer" --version "$DOTNET_SDK_VERSION" --install-dir "$DOTNET_ROOT" --no-path
  rm -f "$installer"
fi
[[ "$(dotnet --version)" == "$DOTNET_SDK_VERSION" ]] || { echo "ERROR: unexpected .NET SDK $(dotnet --version)" >&2; exit 3; }
SDK_SECONDS="$(elapsed_seconds "$SDK_START_EPOCH")"

VALIDATE_START_EPOCH="$(date +%s)"
bash scripts/validate.sh
VALIDATE_SECONDS="$(elapsed_seconds "$VALIDATE_START_EPOCH")"

HOST_TEST_START_EPOCH="$(date +%s)"
bash scripts/test.sh
HOST_TEST_SECONDS="$(elapsed_seconds "$HOST_TEST_START_EPOCH")"

WORKLOAD_START_EPOCH="$(date +%s)"
WORKLOAD_CACHE_MARKER="$DOTNET_ROOT/.sts2launcher-ios-workload-set"
workload_cwd="$(mktemp -d)"
trap 'rm -rf "$workload_cwd"' EXIT
(
  cd "$workload_cwd"
  workload_cached=0
  if [[ -f "$WORKLOAD_CACHE_MARKER" ]] && [[ "$(cat "$WORKLOAD_CACHE_MARKER")" == "$DOTNET_WORKLOAD_SET" ]]; then
    if "$DOTNET_ROOT/dotnet" workload list 2>/dev/null | awk '$1 == "ios" { found=1 } END { exit found ? 0 : 1 }'; then
      workload_cached=1
      echo "Using verified cached iOS workload set $DOTNET_WORKLOAD_SET."
    fi
  fi

  if [[ "$workload_cached" != "1" ]]; then
    ok=0
    for attempt in 1 2; do
      if "$DOTNET_ROOT/dotnet" workload install ios --version "$DOTNET_WORKLOAD_SET"; then ok=1; break; fi
      sleep $((attempt * 5))
    done
    [[ "$ok" == 1 ]] || exit 4
    printf '%s\n' "$DOTNET_WORKLOAD_SET" > "$WORKLOAD_CACHE_MARKER"
  fi

  "$DOTNET_ROOT/dotnet" workload --info
) | tee artifacts/reports/ios-workload.txt
rm -rf "$workload_cwd"; trap - EXIT
WORKLOAD_SECONDS="$(elapsed_seconds "$WORKLOAD_START_EPOCH")"

IOS_BUILD_START_EPOCH="$(date +%s)"
STS2_SKIP_STATIC_VALIDATION=1 bash scripts/build-ios.sh 2>&1 | tee artifacts/reports/ios-build.txt
IOS_BUILD_SECONDS="$(elapsed_seconds "$IOS_BUILD_START_EPOCH")"
{
  echo
  echo "Cache state after iOS publish"
  cache_path_line "Isolated iOS NuGet" "$ROOT/.nuget/packages"
  cache_path_line "Pinned .NET SDK/workloads" "$DOTNET_ROOT"
  cache_path_line "iOS arm64 obj" "$IOS_CACHE_ROOT"
  if [[ -d "$AOT_CACHE_DIR" ]]; then
    echo "AOT output files after build: $(find "$AOT_CACHE_DIR" -type f | wc -l | tr -d ' ')"
  else
    echo "AOT output files after build: 0"
  fi
} | tee -a "$CACHE_REPORT"

IPA_VERIFY_START_EPOCH="$(date +%s)"
bash scripts/verify-ipa.sh "$STS2_IPA_REL"
IPA_VERIFY_SECONDS="$(elapsed_seconds "$IPA_VERIFY_START_EPOCH")"
TOTAL_SECONDS="$(elapsed_seconds "$BUILD_START_EPOCH")"

{
  echo "StS2 Launcher iOS — Step 35.0.31 / Step 36.0.2 Exact ExecuteEssential Failure-Chain Capture"
  echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
  echo "Commit: ${CM_COMMIT:-unknown}"
  echo "Branch: ${CM_BRANCH:-unknown}"
  echo "Xcode: $(xcodebuild -version | tr '\n' ' ')"
  echo ".NET SDK: $(dotnet --version)"
  echo "iOS workload set: $DOTNET_WORKLOAD_SET"
  echo "Static validation: PASS"
  echo "Host unit tests: PASS"
  echo "IPA verification: PASS"
  echo "Timing — .NET SDK setup: ${SDK_SECONDS}s"
  echo "Timing — static validation: ${VALIDATE_SECONDS}s"
  echo "Timing — host tests + active fixture builds: ${HOST_TEST_SECONDS}s"
  echo "Timing — iOS workload install/info: ${WORKLOAD_SECONDS}s"
  echo "Timing — iOS publish/package preparation: ${IOS_BUILD_SECONDS}s"
  echo "Timing — IPA verification: ${IPA_VERIFY_SECONDS}s"
  echo "Timing — total canonical pipeline: ${TOTAL_SECONDS}s"
  echo "Physically proven Step 22.2 Core behavior: byte-for-byte protected by manifest"
  echo "Device text reports: Documents/StS2Launcher/Reports/*.txt"
  echo "Step 23 production behavior: first real sts2.dll CLR load is available only as an explicit on-device gate; build/CI never bundles or loads game payload"
  echo "IPA: $STS2_IPA_REL"
  echo "IPA SHA-256: $(shasum -a 256 "$STS2_IPA_REL" | awk '{print $1}')"
} | tee artifacts/reports/build-summary.txt
