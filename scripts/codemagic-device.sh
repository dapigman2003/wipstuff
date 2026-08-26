#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
source scripts/lib/current-release.sh
source scripts/lib/codemagic-common.sh
mkdir -p artifacts/reports artifacts/logs artifacts/test-results
[[ "$(uname -s)" == "Darwin" ]] || { echo "ERROR: iOS device candidate build must run on macOS/Xcode." >&2; exit 2; }

sts2_timing_init
trap 'sts2_report_cache_sizes' EXIT
sts2_write_environment_report "DEVICE CANDIDATE"
sts2_run_timed "Ensure pinned .NET SDK" sts2_ensure_dotnet
# The complete host suite is deliberately not repeated here. The user must run step32-fast
# first on the exact same commit; this workflow retains static validation as a cheap source-integrity guard.
sts2_run_timed "Canonical static validation" bash scripts/validate.sh

install_ios_workload() {
  local workload_cwd ok attempt
  workload_cwd="$(mktemp -d)"
  ok=0
  (
    cd "$workload_cwd"
    for attempt in 1 2; do
      if "$DOTNET_ROOT/dotnet" workload install ios --version "$DOTNET_WORKLOAD_SET" --skip-manifest-update; then
        ok=1
        break
      fi
      sleep $((attempt * 5))
    done
    [[ "$ok" == 1 ]] || exit 4
    "$DOTNET_ROOT/dotnet" workload --info
  ) | tee artifacts/reports/ios-workload.txt
  local status=${PIPESTATUS[0]}
  rm -rf "$workload_cwd"
  return "$status"
}

sts2_run_timed "Install pinned iOS workload" install_ios_workload
sts2_run_timed "Build iOS app / IPA" bash -c 'set -o pipefail; STS2_SKIP_STATIC_VALIDATION=1 bash scripts/build-ios.sh 2>&1 | tee artifacts/reports/ios-build.txt'
sts2_run_timed "Verify IPA" bash scripts/verify-ipa.sh "$STS2_IPA_REL"

{
  echo "StS2 Launcher iOS — Step 32 DEVICE CANDIDATE PASS"
  echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
  echo "Commit: ${CM_COMMIT:-unknown}"
  echo "Branch: ${CM_BRANCH:-unknown}"
  echo "Xcode: $(xcodebuild -version | tr '\n' ' ')"
  echo ".NET SDK: $(dotnet --version)"
  echo "iOS workload set: $DOTNET_WORKLOAD_SET"
  echo "Static validation: PASS"
  echo "Host unit tests: NOT REPEATED — REQUIRE step32-fast PASS on this exact commit"
  echo "IPA verification: PASS"
  echo "Device text reports: Documents/StS2Launcher/Reports/*.txt"
  echo "IPA: $STS2_IPA_REL"
  echo "IPA SHA-256: $(shasum -a 256 "$STS2_IPA_REL" | awk '{print $1}')"
} | tee artifacts/reports/build-summary.txt
