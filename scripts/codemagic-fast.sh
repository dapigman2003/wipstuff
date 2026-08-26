#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
source scripts/lib/current-release.sh
source scripts/lib/codemagic-common.sh
mkdir -p artifacts/reports artifacts/logs artifacts/test-results
[[ "$(uname -s)" == "Darwin" ]] || { echo "ERROR: Codemagic fast preflight is pinned to the free macOS M2 workflow." >&2; exit 2; }

sts2_timing_init
sts2_write_environment_report "FAST HOST PREFLIGHT"
sts2_run_timed "Ensure pinned .NET SDK" sts2_ensure_dotnet
sts2_run_timed "Canonical static validation" bash scripts/validate.sh
sts2_run_timed "Complete host regression suite" bash scripts/test.sh
sts2_report_cache_sizes

{
  echo "StS2 Launcher iOS — Step 32 FAST HOST PREFLIGHT PASS"
  echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
  echo "Commit: ${CM_COMMIT:-unknown}"
  echo "Branch: ${CM_BRANCH:-unknown}"
  echo ".NET SDK: $(dotnet --version)"
  echo "Static validation: PASS"
  echo "Complete host unit tests: PASS"
  echo "Device workflow prerequisite: PASS for this exact commit only"
  echo "NEXT: run ios-step-32 on the exact same commit. Do not install an IPA unless that workflow also passes."
} | tee artifacts/reports/fast-preflight-summary.txt
